"""Review generated alpha on a contrasting background; white previews conceal interior transparency."""
import argparse
import json
from pathlib import Path
import numpy as np
from scipy import ndimage as ndi
from PIL import Image, ImageDraw

parser = argparse.ArgumentParser()
parser.add_argument("folder", type=Path)
parser.add_argument("--output", type=Path, required=True)
args = parser.parse_args()
args.output.mkdir(parents=True, exist_ok=True)
rows = []
for path in args.folder.glob("*.png"):
    if path.name.startswith(("sheet", "preview")):
        continue
    image = Image.open(path).convert("RGBA")
    if image.size != (480, 480):
        continue
    alpha = np.asarray(image)[:, :, 3]
    core = alpha >= 192
    inside = ndi.binary_fill_holes(core)
    defects = inside & (alpha < 192)
    labels, count = ndi.label(defects)
    regions = sorted([int((labels == i).sum()) for i in range(1, count + 1)], reverse=True)
    rows.append({"file": path.name, "interior_below_192": int(defects.sum()),
                 "interior_soft_32_191": int((defects & (alpha >= 32)).sum()),
                 "holes_over_8": sum(area >= 8 for area in regions), "largest_regions": regions[:6]})
rows.sort(key=lambda row: row["interior_soft_32_191"], reverse=True)
(args.output / "diagnostics.json").write_text(json.dumps(rows, indent=2), encoding="utf-8")
size = 240
canvas = Image.new("RGB", (size * 5, (min(40, len(rows)) + 4) // 5 * (size + 45)), "#303040")
draw = ImageDraw.Draw(canvas)
for i, row in enumerate(rows[:40]):
    image = Image.open(args.folder / row["file"]).convert("RGBA").resize((size, size))
    background = Image.new("RGBA", image.size, "#585879")
    background.alpha_composite(image)
    x = i % 5 * size
    y = i // 5 * (size + 45)
    canvas.paste(background.convert("RGB"), (x, y))
    draw.text((x + 3, y + size), row["file"].replace("working_adult", "adult")[:34], fill="white")
    draw.text((x + 3, y + size + 16), f'soft interior: {row["interior_soft_32_191"]}', fill="white")
canvas.save(args.output / "sheet.png")
print(json.dumps(rows[:10], indent=2))
