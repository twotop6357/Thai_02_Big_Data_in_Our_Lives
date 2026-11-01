"""Serialized real ComfyUI checks: two seeds per product and fixed-seed age comparisons. No global interrupt."""
import argparse
import hashlib
import json
from pathlib import Path
import time
import urllib.error
import urllib.request
import uuid
from benchmark import run, http
from build_workflow import read_json, build

ROOT = Path(__file__).resolve().parents[2]


def config_hash():
    paths = [ROOT / "Assets/StreamingAssets/BlockGeneration/BlockCatalog.json",
             ROOT / "Tools/ComfyUI/model.json", ROOT / "Tools/ComfyUI/presets.json", ROOT / "Tools/ComfyUI/build_workflow.py",
             ROOT / "Tools/ComfyUI/control_model.json", ROOT / "Tools/ComfyUI/background_model.json"]
    model = read_json("model.json")
    if model.get("active_mode") == "reference_illustration":
        paths += [ROOT / "Tools/ComfyUI/text_encoder.json", ROOT / "Tools/ComfyUI/vae.json",
                  ROOT / "Assets/StreamingAssets/BlockGeneration/StyleReferences" / model["style_reference"]["filename"]]
    return hashlib.sha256(b"".join(p.read_bytes() for p in paths)).hexdigest()


def case_hash(product, age, seed):
    api = build(age, product, "red", "blue", "quality", seed)[0]
    return hashlib.sha256(json.dumps(api, sort_keys=True).encode()).hexdigest()


def ensure_reference(server, product):
    name = product.get("poseGuide")
    if not name:
        return
    if Path(name).name != name or ".." in name:
        raise ValueError("Unsafe pose reference")
    data = (ROOT / "Assets/StreamingAssets/BlockGeneration/ControlGuides" / name).read_bytes()
    try:
        existing = http(server, "/view?filename=" + name + "&subfolder=Thai02Quality&type=input", binary=True)
        if hashlib.sha256(existing).digest() != hashlib.sha256(data).digest():
            raise ValueError("Existing immutable reference differs: " + name)
        return
    except urllib.error.HTTPError as ex:
        if ex.code != 404:
            raise
    boundary = uuid.uuid4().hex
    parts = []
    for key, value in (("subfolder", "Thai02Quality"), ("type", "input"), ("overwrite", "false")):
        parts.append(f'--{boundary}\r\nContent-Disposition: form-data; name="{key}"\r\n\r\n{value}\r\n'.encode())
    parts.append(f'--{boundary}\r\nContent-Disposition: form-data; name="image"; filename="{name}"\r\nContent-Type: image/png\r\n\r\n'.encode()
                 + data + f'\r\n--{boundary}--\r\n'.encode())
    request = urllib.request.Request(server + "/upload/image", data=b"".join(parts),
                                    headers={"Content-Type": "multipart/form-data; boundary=" + boundary})
    with urllib.request.urlopen(request, timeout=20) as response:
        reply = json.load(response)
    if reply.get("name") != name or reply.get("subfolder") != "Thai02Quality":
        raise ValueError("Unexpected pose upload path")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--server", default="http://127.0.0.1:8188")
    parser.add_argument("--output", type=Path, default=ROOT / "docs/validation/comfy_blocks")
    parser.add_argument("--suite", choices=("products", "ages", "all"), default="all")
    parser.add_argument("--resume", action="store_true", help="Reuse a saved image only when its exact API graph hash still matches")
    parser.add_argument("--stamp-existing", action="store_true", help="Add per-case graph hashes to a completed, unchanged-config legacy report")
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=True)
    fingerprint = config_hash()
    prior_path = args.output / "report.json"
    prior = json.loads(prior_path.read_text()) if prior_path.exists() else None
    if args.stamp_existing:
        if not prior or not prior.get("completed") or prior["config_sha256"] != fingerprint:
            raise ValueError("Only a completed report with unchanged generation settings can be stamped")
        for item in prior["results"]:
            selection = item["selection"]
            item["api_sha256"] = case_hash(selection["product"], selection["group"], selection["seed"])
        prior_path.write_text(json.dumps(prior, indent=2))
        print("Stamped immutable per-case generation hashes.")
        return
    products = read_json("presets.json")["products"]
    # Fix known problem categories first so visual review can catch an issue before the full matrix.
    order = ["headphones", "tops", "hats", "tumblers", "bags", "shoes", "sunglasses", "bicycles", "wristwatches", "umbrellas"]
    cases = []
    if args.suite in ("products", "all"):
        cases += [(p, "student", seed, "products") for seed in (1206501, 1206502) for p in order]
    if args.suite in ("ages", "all"):
        cases += [(p, age, 1206501, "ages") for p in ("bags", "shoes") for age in ("child", "student", "working_adult", "senior")]
    report = {"config_sha256": fingerprint, "suite": args.suite, "results": [], "failed": [], "total_cases": len(cases)}
    refs = set()
    for product, age, seed, suite in cases:
        if config_hash() != fingerprint:
            report["stopped"] = "Configuration changed during visual verification; remaining cases were not submitted."
            break
        name = f"{product}_{age}_{seed}.png"
        path = args.output / name
        api_digest = case_hash(product, age, seed)
        try:
            # Student age is the identical case already generated in the first product pass.
            previous = next((x for x in report["results"] if x["local_image"] == name), None)
            if previous:
                result = dict(previous, suite=suite, reused=True)
            elif args.resume and prior and next((x for x in prior["results"] if x["local_image"] == name and x.get("api_sha256") == api_digest and path.exists()), None):
                previous = next(x for x in prior["results"] if x["local_image"] == name and x.get("api_sha256") == api_digest)
                result = dict(previous, suite=suite, reused_previous=True)
            else:
                deadline = time.monotonic() + 120
                while True:
                    queue = http(args.server, "/queue")
                    if not queue.get("queue_running") and not queue.get("queue_pending"):
                        break
                    if time.monotonic() > deadline:
                        raise TimeoutError("Shared server remains busy; did not interrupt it")
                    time.sleep(1)
                if product not in refs:
                    ensure_reference(args.server, products[product]); refs.add(product)
                if path.exists():
                    history = args.output / "history"
                    history.mkdir(exist_ok=True)
                    (history / (path.stem + "_" + hashlib.sha256(path.read_bytes()).hexdigest()[:10] + ".png")).write_bytes(path.read_bytes())
                result = run(args.server, age, product, "red", "blue", "quality", seed, 180, path)
                result["suite"] = suite
            result["api_sha256"] = api_digest
            report["results"].append(result)
            print("READY " + suite + ": " + product + "/" + age + "/" + str(seed), flush=True)
        except Exception as ex:
            report["failed"].append({"product": product, "age": age, "seed": seed, "suite": suite, "error": str(ex)})
            print("FAILED " + product + ": " + str(ex), flush=True)
        (args.output / "report.json").write_text(json.dumps(report, indent=2))
    report["completed"] = not report.get("stopped")
    (args.output / "report.json").write_text(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
