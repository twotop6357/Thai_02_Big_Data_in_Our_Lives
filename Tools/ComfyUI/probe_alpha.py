"""Compare alpha candidates on the exact same decoded RGB; no production settings are changed."""
import argparse
import copy
import json
from pathlib import Path
import time
import urllib.parse
import urllib.request
import uuid
import sys
sys.path.insert(0, str(Path(__file__).resolve().parent))
from build_workflow import build
from prepare_style_reference import ensure


def http(path, data=None, binary=False):
    payload = None if data is None else json.dumps(data).encode()
    request = urllib.request.Request("http://127.0.0.1:8188" + path, data=payload,
                                     headers={"Content-Type": "application/json"} if data is not None else {})
    with urllib.request.urlopen(request, timeout=20) as response:
        return response.read() if binary else json.load(response)


def candidate(api):
    api = copy.deepcopy(api)
    for key in list(api):
        if int(key) >= 40:
            del api[key]
    api["22"]["inputs"]["mask"] = ["21", 0]
    def node(kind, **inputs):
        return {"class_type": kind, "inputs": inputs}
    api["40"] = node("ImageInvert", image=["6", 0])
    api["41"] = node("RemoveBackground", bg_removal_model=["20", 0], image=["40", 0])
    api["42"] = node("MaskComposite", destination=["21", 0], source=["41", 0], x=0, y=0, operation="add")
    api["90"] = node("SaveImage", images=["6", 0], filename_prefix="Thai02/AlphaProbe/rgb")
    api["91"] = node("SaveImage", images=["8", 0], filename_prefix="Thai02/AlphaProbe/baseline")
    for index, value in enumerate([0.1, 0.25, 0.5]):
        base = 100 + index * 10
        api[str(base)] = node("ThresholdMask", mask=["42", 0], value=value)
        api[str(base+1)] = node("GrowMask", mask=[str(base), 0], expand=-1, tapered_corners=True)
        api[str(base+2)] = node("MaskComposite", destination=["42", 0], source=[str(base+1), 0], x=0, y=0, operation="add")
        api[str(base+3)] = node("InvertMask", mask=[str(base+2), 0])
        api[str(base+4)] = node("JoinImageWithAlpha", image=["6", 0], alpha=[str(base+3), 0])
        api[str(base+5)] = node("ImageScale", image=[str(base+4), 0], upscale_method="area", width=480, height=480, crop="disabled")
        api[str(base+6)] = node("SaveImage", images=[str(base+5), 0], filename_prefix="Thai02/AlphaProbe/union"+str(value))
    return api


def run(api, folder, tag):
    queue = http("/queue")
    if queue["queue_running"] or queue["queue_pending"]:
        raise RuntimeError("Shared server is busy; no request submitted.")
    start = time.perf_counter()
    prompt = http("/prompt", {"prompt": api, "client_id": str(uuid.uuid4())})
    if "prompt_id" not in prompt:
        raise RuntimeError(prompt)
    while time.perf_counter() - start < 180:
        history = http("/history/"+prompt["prompt_id"]).get(prompt["prompt_id"])
        if history:
            if history["status"]["status_str"] != "success":
                raise RuntimeError(history["status"])
            files = {}
            for key in ["90", "91", "106", "116", "126"]:
                output = history["outputs"][key]["images"][0]
                data = http("/view?"+urllib.parse.urlencode(output), binary=True)
                name = tag + "_" + key + ".png"
                (folder/name).write_bytes(data)
                files[key] = name
            result = {"tag": tag, "seconds": round(time.perf_counter()-start, 3), "files": files, "prompt_id":prompt["prompt_id"]}
            print(tag, result["seconds"], flush=True)
            return result
        time.sleep(.2)
    raise TimeoutError(prompt)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--backdrop", choices=["white", "gray"], default="white")
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=True)
    ensure("http://127.0.0.1:8188")
    cases = [("umbrellas", "student", "white", "black", 1211320),
             ("shoes", "student", "white", "white", 7042026),
             ("bags", "working_adult", "white", "blue", 7042027),
             ("sunglasses", "student", "white", "black", 1211314),
             ("bicycles", "student", "red", "blue", 1211217)]
    rows = []
    for product, group, first, second, seed in cases:
        api, _, selection = build(group, product, first, second, seed=seed)
        if args.backdrop == "white":
            api["18"]["inputs"]["string_b"] = api["18"]["inputs"]["string_b"].replace("Flat uniform medium-gray background (#808080), opaque object surfaces", "Plain white background")
        result = run(candidate(api), args.output, f"{product}_{first}_{second}")
        result["selection"] = selection
        rows.append(result)
        (args.output/"report.json").write_text(json.dumps(rows, indent=2), encoding="utf-8")
