"""Build immutable pose references from the actual product block artwork, excluding its UI border/label."""
from pathlib import Path
import hashlib
import io
import json
import uuid
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
BLOCKS = {"bags": "Bag", "shoes": "Shoes", "hats": "Hat", "sunglasses": "Sunglasses",
          "headphones": "Headphones", "tops": "Top", "bicycles": "Bicycle", "tumblers": "Tumbler",
          "wristwatches": "WristWatch", "umbrellas": "Umbrella"}


def main():
    catalog_path = ROOT / "Assets/StreamingAssets/BlockGeneration/BlockCatalog.json"
    catalog = json.loads(catalog_path.read_text(encoding="utf-8"))
    turbo = json.loads((ROOT / "Tools/ComfyUI/model.json").read_text(encoding="utf-8")).get("active_mode") in ("simple_turbo_single_pass", "reference_illustration")
    output = catalog_path.parent / "ControlGuides"
    evidence = {}
    for product in catalog["products"]:
        source = ROOT / "Assets/UI/Blocks/EnglishBlocks" / (BLOCKS[product["id"]] + "Block_Eng.png")
        image = Image.open(source).convert("RGBA")
        w, h = image.size
        # Existing blocks share this layout: blue frame at the edges, labels below 75% height.
        content = image.crop((int(w*.07), int(h*.07), int(w*.93), int(h*.74)))
        mask = Image.new("L", content.size)
        pixels = content.get_flattened_data() if hasattr(content, "get_flattened_data") else content.getdata()
        mask.putdata([255 if a > 0 and min(r, g, b) < 245 else 0 for r, g, b, a in pixels])
        bounds = mask.getbbox()
        if not bounds:
            raise ValueError("No object found in block resource: " + str(source))
        object_image = content.crop(bounds).convert("RGB")
        object_image.thumbnail((780, 780), Image.Resampling.LANCZOS)
        # thumbnail does not upscale; enlarge the small source icon while keeping its aspect ratio.
        scale = 780 / max(object_image.size)
        object_image = object_image.resize((round(object_image.width*scale), round(object_image.height*scale)), Image.Resampling.LANCZOS)
        reference = Image.new("RGB", (1024, 1024), "white")
        reference.paste(object_image, ((1024-object_image.width)//2, (1024-object_image.height)//2))
        stream = io.BytesIO(); reference.save(stream, format="PNG"); data = stream.getvalue()
        digest = hashlib.sha256(data).hexdigest()
        name = "guide_block_" + product["id"] + "_" + digest[:10] + ".png"
        target = output / name
        if target.exists() and target.read_bytes() != data:
            raise ValueError("Immutable reference collision: " + str(target))
        if not target.exists():
            target.write_bytes(data)
            Path(str(target)+".meta").write_text("fileFormatVersion: 2\nguid: " + uuid.uuid4().hex + "\n")
        if not turbo and (not product.get("poseGuide") or product["poseGuide"].startswith("guide_block_")):
            product["poseGuide"] = name
        # Preserve item-specific tuning established by repeated visual review.
        if not turbo:
            product.setdefault("poseStrength", .4)
            product.setdefault("poseEndPercent", .5)
            product.setdefault("poseLowThreshold", .02)
            product.setdefault("poseHighThreshold", .06)
        evidence[product["id"]] = {"source": str(source.relative_to(ROOT)), "source_sha256": hashlib.sha256(source.read_bytes()).hexdigest(),
            "content_crop": [int(w*.07), int(h*.07), int(w*.93), int(h*.74)], "object_bounds_in_crop": bounds,
            "reference": name, "reference_sha256": digest}
    catalog_path.write_text(json.dumps(catalog, ensure_ascii=False, indent=2)+"\n", encoding="utf-8")
    report = ROOT / "docs/validation/comfy_shoepose/block_references.json"
    report.write_text(json.dumps(evidence, indent=2)+"\n")
    print("Synced references for " + str(len(evidence)) + " product blocks.")


if __name__ == "__main__":
    main()
