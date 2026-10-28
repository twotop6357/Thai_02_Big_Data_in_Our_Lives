"""Build ComfyUI UI/API workflows from audience, product and two named colors. No custom nodes."""
import argparse
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parent


def read_json(name):
    data = json.loads((ROOT / name).read_text(encoding="utf-8"))
    if name == "presets.json":
        catalog = json.loads((ROOT / data["catalog"]).read_text(encoding="utf-8"))
        data.update(style_prefix=catalog["stylePrefix"], style=catalog["styleSuffix"], negative=catalog["negativePrompt"],
                    groups={item["id"]: item for item in catalog["groups"]}, colors=[item["id"] for item in catalog["colors"]],
                    products={item["id"]: dict(item, primary="({color1} " + item["primaryPart"] + ":1.15)",
                        secondary="({color2} " + item["secondaryPart"] + ":1.15)") for item in catalog["products"]},
                    equipment_variants=catalog["equipmentVariants"])
    return data


def build(group="student", product="bags", color1="blue", color2="white", profile="quality", seed=7042026):
    presets, model = read_json("presets.json"), read_json("model.json")
    product = {"bag": "bags", "cap": "hats", "sneakers": "shoes", "watch": "wristwatches"}.get(product, product)
    for value, allowed in ((group, presets["groups"]), (product, presets["products"]),
                           (color1, presets["colors"]), (color2, presets["colors"]),
                           (profile, presets["profiles"])):
        if value not in allowed:
            raise ValueError(f"Unsupported input: {value}")
    if not isinstance(seed, int) or not 0 <= seed <= 18446744073709551615:
        raise ValueError("Seed must be an unsigned 64-bit integer.")
    item = presets["products"][product]
    audience = presets["groups"][group]["prompt"]
    primary = item["primary"].format(color1=color1)
    secondary = item["secondary"].format(color2=color2)
    sampling = presets["profiles"][profile]
    negative = presets["negative"] + (", " + item["negativePrompt"] if item.get("negativePrompt") else "")
    subject = item["prompt"]
    resolved = product
    if product == "equipment":
        variant = presets["equipment_variants"][seed % len(presets["equipment_variants"])]
        subject, resolved = variant["prompt"], variant["id"]
    width, height = model["internal_resolution"]

    def node(kind, title, **inputs):
        return {"class_type": kind, "inputs": inputs, "_meta": {"title": title}}

    api = {
        "1": node("CheckpointLoaderSimple", model["name"], ckpt_name=model["filename"]),
        "2": node("CLIPTextEncode", "Positive product conditioning", text=["18", 0], clip=["1", 1]),
        "3": node("CLIPTextEncode", "Negative / exclude people and text", text=negative, clip=["1", 1]),
        "4": node("EmptyLatentImage", f"{width} x {height} internal / batch 1", width=width, height=height, batch_size=1),
        "5": node("KSampler", f"{profile} / {sampling['steps']} steps", model=["1", 0], positive=["26", 0],
                  negative=["26", 1], latent_image=["4", 0], seed=seed, steps=sampling["steps"],
                  cfg=sampling["cfg"], sampler_name=sampling["sampler"], scheduler=sampling["scheduler"], denoise=1.0),
        "6": node("VAEDecode", "Decode final image", samples=["5", 0], vae=["1", 2]),
        "7": node("SaveImage", "Final 480 x 480 RGBA PNG", images=["8", 0], filename_prefix="Thai02/product"),
        "8": node("ImageScale", "Downsample RGBA to 480 x 480", image=["23", 0], upscale_method="area", width=480, height=480, crop="disabled"),
        "10": node("PrimitiveString", f"INPUT 1 / audience: {group}", value=audience),
        "11": node("PrimitiveString", f"INPUT 2 / product: {product}", value=subject),
        "12": node("PrimitiveString", f"INPUT 3 / primary: {color1}", value=primary),
        "13": node("PrimitiveString", f"INPUT 4 / accent: {color2}", value=secondary),
        "14": node("StringConcatenate", "Product + colors", string_a=["11", 0], string_b=["15", 0], delimiter=", "),
        "15": node("StringConcatenate", "Product-specific color parts", string_a=["12", 0], string_b=["13", 0], delimiter=", "),
        "16": node("StringConcatenate", "Combine four choices", string_a=["14", 0], string_b=["10", 0], delimiter=", "),
        "17": node("StringConcatenate", "Fixed mobile RPG item style", string_a=presets["style_prefix"], string_b=["16", 0], delimiter=", "),
        "18": node("StringConcatenate", "Fixed product composition", string_a=["17", 0], string_b=presets["style"], delimiter=", "),
        "20": node("LoadBackgroundRemovalModel", "BiRefNet foreground segmentation", bg_removal_name=read_json("background_model.json")["filename"]),
        "21": node("RemoveBackground", "Foreground mask at native resolution", bg_removal_model=["20", 0], image=["6", 0]),
        "22": node("InvertMask", "Foreground to Comfy alpha convention", mask=["21", 0]),
        "23": node("JoinImageWithAlpha", "Apply actual alpha (not painted transparency)", image=["6", 0], alpha=["22", 0]),
        "24": node("LoadImage", "Single product structure guide", image=f"Thai02Quality/guide_{resolved}.png"),
        "25": node("ControlNetLoader", "SDXL Canny structure control", control_net_name=read_json("control_model.json")["filename"]),
        "26": node("ControlNetApplyAdvanced", "Keep one complete product", positive=["2", 0], negative=["3", 0],
                   control_net=["25", 0], image=["24", 0], vae=["1", 2], strength=1.4, start_percent=0.0, end_percent=1.0)
    }
    inputs = {"group": group, "product": product, "color1": color1, "color2": color2,
              "profile": profile, "seed": seed, "width": 480, "height": 480,
              "resolved_product": resolved, "alpha": True,
              "positive_prompt": ", ".join([presets["style_prefix"], subject, primary, secondary, audience, presets["style"]])}
    return api, to_ui(api, inputs), inputs


def to_ui(api, selection):
    """Version 0.4 workflow format, supported by the installed ComfyUI frontend."""
    outputs = {"CheckpointLoaderSimple": [("MODEL", "MODEL"), ("CLIP", "CLIP"), ("VAE", "VAE")],
               "CLIPTextEncode": [("CONDITIONING", "CONDITIONING")],
               "EmptyLatentImage": [("LATENT", "LATENT")], "KSampler": [("LATENT", "LATENT")],
               "VAEDecode": [("IMAGE", "IMAGE")], "SaveImage": [("IMAGE", "IMAGE")], "ImageScale": [("IMAGE", "IMAGE")],
               "PrimitiveString": [("STRING", "STRING")], "StringConcatenate": [("STRING", "STRING")],
               "LoadBackgroundRemovalModel": [("bg_model", "BACKGROUND_REMOVAL")],
               "RemoveBackground": [("mask", "MASK")], "InvertMask": [("MASK", "MASK")], "JoinImageWithAlpha": [("IMAGE", "IMAGE")],
               "LoadImage": [("IMAGE", "IMAGE"), ("MASK", "MASK")], "ControlNetLoader": [("CONTROL_NET", "CONTROL_NET")],
               "ControlNetApplyAdvanced": [("positive", "CONDITIONING"), ("negative", "CONDITIONING")]}
    widgets = {"CheckpointLoaderSimple": ["ckpt_name"], "CLIPTextEncode": ["text"],
               "EmptyLatentImage": ["width", "height", "batch_size"],
               "KSampler": ["seed", "control_after_generate", "steps", "cfg", "sampler_name", "scheduler", "denoise"],
               "VAEDecode": [], "SaveImage": ["filename_prefix"], "ImageScale": ["upscale_method", "width", "height", "crop"], "PrimitiveString": ["value"],
               "StringConcatenate": ["string_a", "string_b", "delimiter"], "LoadBackgroundRemovalModel": ["bg_removal_name"],
               "RemoveBackground": [], "InvertMask": [], "JoinImageWithAlpha": [], "LoadImage": ["image"],
               "ControlNetLoader": ["control_net_name"], "ControlNetApplyAdvanced": ["strength", "start_percent", "end_percent"]}
    positions = {"10": [-900, 0], "11": [-900, 170], "12": [-900, 340], "13": [-900, 510],
                 "14": [-500, 0], "15": [-500, 340], "16": [-80, 0], "17": [330, 0], "18": [330, 300],
                 "1": [750, 620], "2": [750, 0], "3": [750, 310], "4": [1120, 620],
                 "5": [1320, 0], "6": [1690, 0], "20": [1690, 350], "21": [2070, 350],
                 "22": [2450, 350], "23": [2830, 0], "8": [3210, 0], "7": [3590, 0],
                 "24": [750, 950], "25": [1120, 950], "26": [1120, 310]}
    ui_nodes, by_id, links = [], {}, []
    for order, (key, value) in enumerate(api.items()):
        kind, data = value["class_type"], value["inputs"]
        widget_values = ["fixed" if name == "control_after_generate" else
                         ("" if isinstance(data[name], list) else data[name]) for name in widgets[kind]]
        ui_node = {"id": int(key), "type": kind, "title": value["_meta"]["title"],
                   "pos": positions[key], "size": [470 if kind == "CLIPTextEncode" else 340,
                                                       240 if kind == "CLIPTextEncode" else 420 if kind == "SaveImage" else 290 if kind == "KSampler" else 230 if kind == "StringConcatenate" else 220 if kind == "ImageScale" else 135],
                   "flags": {}, "order": order, "mode": 0, "inputs": [],
                   "outputs": [{"name": name, "type": data_type, "links": []} for name, data_type in outputs[kind]],
                   "properties": {"Node name for S&R": kind}, "widgets_values": widget_values}
        ui_nodes.append(ui_node)
        by_id[key] = ui_node
    for key, value in api.items():
        for name, source in value["inputs"].items():
            if not isinstance(source, list):
                continue
            origin, slot = source
            data_type = by_id[origin]["outputs"][slot]["type"]
            link_id = len(links) + 1
            target_slot = len(by_id[key]["inputs"])
            input_slot = {"name": name, "type": data_type, "link": link_id}
            if name in widgets[value["class_type"]]:
                input_slot["widget"] = {"name": name}
            by_id[key]["inputs"].append(input_slot)
            by_id[origin]["outputs"][slot]["links"].append(link_id)
            links.append([link_id, int(origin), slot, int(key), target_slot, data_type])
    return {"last_node_id": max(map(int, api)), "last_link_id": len(links), "nodes": ui_nodes, "links": links,
            "groups": [], "config": {}, "extra": {"project": "Thai_02_Big_Data_in_Our_Lives", "selection": selection}, "version": 0.4}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--group", default="student")
    parser.add_argument("--product", default="bags")
    parser.add_argument("--color1", default="blue")
    parser.add_argument("--color2", default="white")
    parser.add_argument("--profile", default="quality")
    parser.add_argument("--seed", type=int, default=7042026)
    parser.add_argument("--output-dir", type=Path, default=ROOT / "generated")
    args = parser.parse_args()
    api, ui, selection = build(args.group, args.product, args.color1, args.color2, args.profile, args.seed)
    args.output_dir.mkdir(parents=True, exist_ok=True)
    for suffix, value in (("api", api), ("ui", ui), ("inputs", selection)):
        target = args.output_dir / f"product_480.{suffix}.json"
        target.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        print(f"Written: {target}")


if __name__ == "__main__":
    main()
