"""Re-evaluate only segmentation on saved RGB; no image regeneration or prompt changes."""
import argparse
import json
from pathlib import Path
import sys
sys.path.insert(0, str(Path(__file__).resolve().parent))
from probe_alpha import http, run
from build_workflow import read_json

parser = argparse.ArgumentParser()
parser.add_argument("--output", type=Path, required=True)
args = parser.parse_args()
args.output.mkdir(parents=True, exist_ok=True)
matrix = json.loads(Path("docs/validation/alpha_opaque/matrix1/report.json").read_text())["results"]
gray = json.loads(Path("docs/validation/alpha_opaque/probe_gray/report.json").read_text())
cases = [(row, "95") for row in matrix if row["checks"]["largest_lost_white_region"] > 16 or row["checks"]["largest_lost_color_region"] > 16]
cases += [(row, "90") for row in gray]
rows = []
for row, raw_key in cases:
    descriptor = http("/history/"+row["prompt_id"])[row["prompt_id"]]["outputs"][raw_key]["images"][0]
    source = descriptor["subfolder"]+"/"+descriptor["filename"]+" [output]"
    def node(kind, **inputs):
        return {"class_type": kind, "inputs": inputs}
    api = {"6": node("LoadImage", image=source),
           "20": node("LoadBackgroundRemovalModel", bg_removal_name=read_json("background_model.json")["filename"]),
           "21": node("RemoveBackground", bg_removal_model=["20", 0], image=["6", 0]),
           "40": node("ImageInvert", image=["6", 0]),
           "41": node("RemoveBackground", bg_removal_model=["20", 0], image=["40", 0]),
           "42": node("MaskComposite", destination=["21", 0], source=["41", 0], x=0, y=0, operation="add"),
           "22": node("InvertMask", mask=["21", 0]),
           "23": node("JoinImageWithAlpha", image=["6", 0], alpha=["22", 0]),
           "8": node("ImageScale", image=["23", 0], upscale_method="area", width=480, height=480, crop="disabled"),
           "90": node("SaveImage", images=["6", 0], filename_prefix="Thai02/AlphaMatting/rgb"),
           "91": node("SaveImage", images=["8", 0], filename_prefix="Thai02/AlphaMatting/before")}
    for index, value in enumerate([0.1, 0.25, 0.5]):
        base = 100+index*10
        api[str(base)] = node("ThresholdMask", mask=["42", 0], value=value)
        api[str(base+1)] = node("GrowMask", mask=[str(base), 0], expand=-1, tapered_corners=True)
        api[str(base+2)] = node("MaskComposite", destination=["42", 0], source=[str(base+1), 0], x=0, y=0, operation="add")
        api[str(base+3)] = node("InvertMask", mask=[str(base+2), 0])
        api[str(base+4)] = node("JoinImageWithAlpha", image=["6", 0], alpha=[str(base+3), 0])
        api[str(base+5)] = node("ImageScale", image=[str(base+4), 0], upscale_method="area", width=480, height=480, crop="disabled")
        api[str(base+6)] = node("SaveImage", images=[str(base+5), 0], filename_prefix="Thai02/AlphaMatting/fused"+str(value))
    result = run(api, args.output, row["tag"])
    rows.append(result)
    (args.output/"report.json").write_text(json.dumps(rows, indent=2), encoding="utf-8")
