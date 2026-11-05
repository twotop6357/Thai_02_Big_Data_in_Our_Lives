"""Test RGB-aware native mask repair on the exact ninety decoded images, without resampling diffusion."""
import argparse
import json
from pathlib import Path
import sys
sys.path.insert(0, str(Path(__file__).resolve().parent))
from alpha_mask_nodes import alpha_nodes
from probe_alpha import http, run
from build_workflow import read_json

parser = argparse.ArgumentParser()
parser.add_argument("--output", type=Path, required=True)
parser.add_argument("--all", action="store_true")
args = parser.parse_args()
args.output.mkdir(parents=True, exist_ok=True)
matrix = json.loads(Path("docs/validation/alpha_opaque/matrix1/report.json").read_text())["results"]
rows = []
for row in matrix:
    if not args.all and row["checks"]["largest_lost_white_region"] <= 16 and row["checks"]["largest_lost_color_region"] <= 16:
        continue
    descriptor = http("/history/"+row["prompt_id"])[row["prompt_id"]]["outputs"]["95"]["images"][0]
    source = descriptor["subfolder"]+"/"+descriptor["filename"]+" [output]"
    def node(kind, **inputs):
        return {"class_type": kind, "inputs": inputs}
    api = {"6": node("LoadImage", image=source), "11": node("PrimitiveString", value=row["selection"]["product"]),
           "20": node("LoadBackgroundRemovalModel", bg_removal_name=read_json("background_model.json")["filename"]),
           "21": node("RemoveBackground", bg_removal_model=["20", 0], image=["6", 0]),
           "22": node("InvertMask", mask=["60", 0]),
           "23": node("JoinImageWithAlpha", image=["6", 0], alpha=["22", 0]),
           "8": node("ImageScale", image=["23", 0], upscale_method="area", width=480, height=480, crop="disabled"),
           "80": node("InvertMask", mask=["21", 0]),
           "81": node("JoinImageWithAlpha", image=["6", 0], alpha=["80", 0]),
           "82": node("ImageScale", image=["81", 0], upscale_method="area", width=480, height=480, crop="disabled"),
           "90": node("SaveImage", images=["6", 0], filename_prefix="Thai02/AlphaRGB/rgb"),
           "91": node("SaveImage", images=["82", 0], filename_prefix="Thai02/AlphaRGB/before"),
           "106": node("SaveImage", images=["8", 0], filename_prefix="Thai02/AlphaRGB/after"),
           "83": node("MaskToImage", mask=["45", 0]),
           "84": node("MaskToImage", mask=["53", 0]),
           "116": node("SaveImage", images=["83", 0], filename_prefix="Thai02/AlphaRGB/white"),
           "126": node("SaveImage", images=["84", 0], filename_prefix="Thai02/AlphaRGB/color")}
    api.update(alpha_nodes())
    result = run(api, args.output, row["tag"])
    result["selection"] = row["selection"]
    rows.append(result)
    (args.output/"report.json").write_text(json.dumps(rows, indent=2), encoding="utf-8")
