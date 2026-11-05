"""Isolated gray-backdrop comparison; same model/style/composition, color-independent alpha core."""
import argparse
import json
from pathlib import Path
import sys
sys.path.insert(0, str(Path(__file__).resolve().parent))
from build_workflow import build
from prepare_style_reference import ensure
from probe_alpha import run

parser = argparse.ArgumentParser()
parser.add_argument("--output", type=Path, required=True)
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
    for key in list(api):
        if int(key) >= 40:
            del api[key]
    api["22"]["inputs"]["mask"] = ["21", 0]
    api["18"]["inputs"]["string_b"] = api["18"]["inputs"]["string_b"].replace("Plain white background", "Flat uniform medium-gray background (#808080), opaque object surfaces")
    def node(kind, **inputs):
        return {"class_type": kind, "inputs": inputs}
    api["90"] = node("SaveImage", images=["6", 0], filename_prefix="Thai02/AlphaGray/rgb")
    api["91"] = node("SaveImage", images=["8", 0], filename_prefix="Thai02/AlphaGray/baseline")
    for index, value in enumerate([0.1, 0.25, 0.5]):
        base = 100 + index * 10
        api[str(base)] = node("ThresholdMask", mask=["21", 0], value=value)
        api[str(base+1)] = node("GrowMask", mask=[str(base), 0], expand=-1, tapered_corners=True)
        api[str(base+2)] = node("MaskComposite", destination=["21", 0], source=[str(base+1), 0], x=0, y=0, operation="add")
        api[str(base+3)] = node("InvertMask", mask=[str(base+2), 0])
        api[str(base+4)] = node("JoinImageWithAlpha", image=["6", 0], alpha=[str(base+3), 0])
        api[str(base+5)] = node("ImageScale", image=[str(base+4), 0], upscale_method="area", width=480, height=480, crop="disabled")
        api[str(base+6)] = node("SaveImage", images=[str(base+5), 0], filename_prefix="Thai02/AlphaGray/core"+str(value))
    result = run(api, args.output, f"{product}_{first}_{second}")
    result["selection"] = selection
    rows.append(result)
    (args.output/"report.json").write_text(json.dumps(rows, indent=2), encoding="utf-8")
