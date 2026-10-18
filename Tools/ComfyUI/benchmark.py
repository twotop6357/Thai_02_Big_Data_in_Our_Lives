"""Measure actual ComfyUI request-to-PNG latency. Uses only the Python standard library."""
import argparse
import json
import math
from pathlib import Path
import statistics
import struct
import time
import urllib.parse
import urllib.request
import uuid
from build_workflow import build, read_json


def http(server, path, payload=None, binary=False):
    data = None if payload is None else json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(server.rstrip("/") + path, data=data,
                                 headers={"Content-Type": "application/json"} if data else {})
    with urllib.request.urlopen(req, timeout=20) as response:
        body = response.read()
    return body if binary else json.loads(body or b"null")


def run(server, group, product, color1, color2, profile, seed, timeout, image_path):
    queue = http(server, "/queue")
    if queue.get("queue_running") or queue.get("queue_pending"):
        raise RuntimeError("ComfyUI is busy. No benchmark job was submitted.")
    api, ui, selection = build(group, product, color1, color2, profile, seed)
    started = time.perf_counter()
    queued = http(server, "/prompt", {"prompt": api, "client_id": str(uuid.uuid4()),
                                     "extra_data": {"extra_pnginfo": {"workflow": ui}}})
    if "prompt_id" not in queued:
        raise RuntimeError(f"Workflow was rejected: {queued}")
    prompt_id = queued["prompt_id"]
    print(f"Queued {prompt_id}: {group}/{product}/{color1}/{color2}", flush=True)
    deadline = time.monotonic() + timeout
    history = None
    while time.monotonic() < deadline:
        history = http(server, f"/history/{prompt_id}").get(prompt_id)
        if history:
            status = history.get("status", {})
            if status.get("status_str") == "error":
                raise RuntimeError(f"ComfyUI job failed: {prompt_id}; inspect server history.")
            if status.get("completed"):
                break
        time.sleep(0.15)
    else:
        # Do not issue a global interrupt, which could stop another user's job on old servers.
        raise TimeoutError(f"Timed out waiting for {prompt_id}. Running server work is not automatically cancelled.")
    generated = [image for output in history.get("outputs", {}).values() for image in output.get("images", [])]
    if not generated:
        raise RuntimeError(f"No output image in history for {prompt_id}.")
    image = generated[0]
    png = http(server, "/view?" + urllib.parse.urlencode(image), binary=True)
    elapsed = time.perf_counter() - started
    if png[:8] != b"\x89PNG\r\n\x1a\n" or len(png) < 24:
        raise RuntimeError("Expected a PNG output.")
    width, height = struct.unpack(">II", png[16:24])
    if (width, height) != (480, 480):
        raise RuntimeError(f"Unexpected image size: {width}x{height}.")
    image_path.parent.mkdir(parents=True, exist_ok=True)
    image_path.write_bytes(png)
    print(f"Completed in {elapsed:.3f} seconds: {image_path.name}", flush=True)
    return {"prompt_id": prompt_id, "selection": selection, "seconds_request_to_png": round(elapsed, 3),
            "width": width, "height": height, "local_image": image_path.name, "server_image": image}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--server", default="http://127.0.0.1:8188")
    parser.add_argument("--runs", type=int, default=4)
    parser.add_argument("--warmup", type=int, default=1)
    parser.add_argument("--profile", choices=["balanced", "fast", "quality"], default="balanced")
    parser.add_argument("--timeout", type=float, default=180)
    parser.add_argument("--output-dir", type=Path, default=Path(__file__).parent / "results")
    args = parser.parse_args()
    if args.runs < 1 or args.warmup < 0 or args.timeout <= 0:
        parser.error("runs >= 1, warmup >= 0 and timeout > 0 are required.")
    stats = http(args.server, "/system_stats")
    devices = [device.get("name", "unknown") for device in stats.get("devices", [])]
    queue = http(args.server, "/queue")
    if queue.get("queue_running") or queue.get("queue_pending"):
        raise RuntimeError("ComfyUI is busy. Wait for the current work to finish.")
    report = {"measured_device": devices, "target_device": "RTX 3060 12GB",
              "target_device_match": any("RTX 3060" in name for name in devices),
              "comfyui_version": stats.get("system", {}).get("comfyui_version"),
              "pytorch_version": stats.get("system", {}).get("pytorch_version"),
              "checkpoint": read_json("model.json")["filename"],
              "sampling": read_json("presets.json")["profiles"][args.profile],
              "internal_resolution": [768, 768], "output_resolution": [480, 480],
              "profile": args.profile, "target_seconds": 20, "warmup": [], "runs": []}
    cases = [("student", "bag", "blue", "white"), ("child", "sunglasses", "red", "yellow"),
             ("working_adult", "bag", "black", "blue"), ("senior", "sunglasses", "brown", "green")]
    args.output_dir.mkdir(parents=True, exist_ok=True)
    for i in range(args.warmup + args.runs):
        is_warmup = i < args.warmup
        case = cases[(i - args.warmup) % len(cases)] if not is_warmup else cases[0]
        # Changing seed prevents ComfyUI's cached sampler from invalidating the timing results.
        result = run(args.server, *case, args.profile, 7042026 + i, args.timeout,
                     args.output_dir / f"{'warmup' if is_warmup else 'run'}_{i:02d}.png")
        report["warmup" if is_warmup else "runs"].append(result)
        (args.output_dir / "benchmark.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    values = sorted(item["seconds_request_to_png"] for item in report["runs"])
    report["summary"] = {"count": len(values), "median_seconds": statistics.median(values),
                         "p95_seconds": values[max(0, math.ceil(0.95 * len(values)) - 1)],
                         "max_seconds": max(values), "meets_20s_on_measured_device": max(values) <= 20,
                         "target_gpu_acceptance_verified": report["target_device_match"] and len(values) >= 20 and max(values) <= 20}
    (args.output_dir / "benchmark.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report["summary"], indent=2), flush=True)


if __name__ == "__main__":
    main()
