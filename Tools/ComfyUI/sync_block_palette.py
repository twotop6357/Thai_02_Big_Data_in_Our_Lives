"""Sample the flat circle interior of the six block PNGs; sync/check catalog RGB, never the border/text."""
import argparse
from collections import Counter
import json
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]


def sample_palette():
    result = {}
    for name in ("Black", "White", "Blue", "Red", "Green", "Yellow"):
        path = ROOT / "Assets/UI/Blocks" / f"{name}Block.png"
        with Image.open(path) as source:
            image = source.convert("RGBA")
            w, h = image.size
            # Common artwork layout: circle center at (50%, 43%); text below 75%.
            crop = image.crop((int(w*.40), int(h*.36), int(w*.60), int(h*.50)))
            counts = Counter(crop.get_flattened_data() if hasattr(crop, "get_flattened_data") else crop.getdata())
            rgba, count = counts.most_common(1)[0]
            if rgba[3] != 255 or count < crop.width * crop.height * .95:
                raise ValueError(f"Circle interior is not flat/opaque; review sampling bounds: {path}")
            result[name.lower()] = "#" + "".join(f"{v:02X}" for v in rgba[:3])
    return result


def sync(check=False):
    palette = sample_palette()
    # Short color words prevent literal-object associations in the fast model.
    descriptions = {"black": "near-black", "white": "white", "blue": "light blue",
                    "red": "soft red", "green": "light pastel green", "yellow": "pale pastel yellow"}
    for filename in ("BlockCatalog.json", "BlockColoring.json"):
        path = ROOT / "Assets/StreamingAssets/BlockGeneration" / filename
        data = json.loads(path.read_text(encoding="utf-8"))
        for color in data["colors"]:
            expected = palette[color["id"]]
            if check:
                if color.get("hex") != expected:
                    raise ValueError(f"{filename}: {color['id']} must match circle RGB {expected}")
            else:
                color["hex"] = expected
                if filename == "BlockCatalog.json":
                    color["prompt"] = descriptions[color["id"]]
        if not check:
            path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    return palette


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", action="store_true")
    print(json.dumps(sync(parser.parse_args().check), indent=2))
