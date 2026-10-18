"""Build ComfyUI UI/API workflows from audience, product and two named colors. No custom nodes."""
import argparse
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parent


def read_json(name):
    return json.loads((ROOT / name).read_text(encoding="utf-8"))


def build(group="student", product="bag", color1="blue", color2="white", profile="balanced", seed=7042026):
    presets, model = read_json("presets.json"), read_json("model.json")
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

    def node(kind, title, **inputs):
        return {"class_type": kind, "inputs": inputs, "_meta": {"title": title}}

    api = {
        "1": node("CheckpointLoaderSimple", "SDXL Base 1.0 / no refiner", ckpt_name=model["filename"]),
        "2": node("CLIPTextEncode", "Positive product conditioning", text=["18", 0], clip=["1", 1]),
        "3": node("CLIPTextEncode", "Negative / exclude people and text", text=presets["negative"], clip=["1", 1]),
        "4": node("EmptyLatentImage", "768 x 768 internal / batch 1", width=768, height=768, batch_size=1),
        "5": node("KSampler", f"{profile} / {sampling['steps']} steps", model=["1", 0], positive=["2", 0],
                  negative=["3", 0], latent_image=["4", 0], seed=seed, steps=sampling["steps"],
                  cfg=sampling["cfg"], sampler_name=sampling["sampler"], scheduler=sampling["scheduler"], denoise=1.0),
        "6": node("VAEDecode", "Decode final image", samples=["5", 0], vae=["1", 2]),
        "7": node("SaveImage", "Final 480 x 480 PNG", images=["8", 0], filename_prefix="Thai02/product"),
        "8": node("ImageScale", "Downsample to 480 x 480", image=["6", 0], upscale_method="area", width=480, height=480, crop="disabled"),
        "10": node("PrimitiveString", f"INPUT 1 / audience: {group}", value=audience),
        "11": node("PrimitiveString", f"INPUT 2 / product: {product}", value=item["prompt"]),
        "12": node("PrimitiveString", f"INPUT 3 / primary: {color1}", value=primary),
        "13": node("PrimitiveString", f"INPUT 4 / accent: {color2}", value=secondary),
        "14": node("StringConcatenate", "Product + colors", string_a=["11", 0], string_b=["15", 0], delimiter=", "),
        "15": node("StringConcatenate", "Product-specific color parts", string_a=["12", 0], string_b=["13", 0], delimiter=", "),
        "16": node("StringConcatenate", "Combine four choices", string_a=["14", 0], string_b=["10", 0], delimiter=", "),
        "17": node("StringConcatenate", "Fixed mobile RPG item style", string_a=presets["style_prefix"], string_b=["16", 0], delimiter=", "),
        "18": node("StringConcatenate", "Fixed product composition", string_a=["17", 0], string_b=presets["style"], delimiter=", ")
    }
    inputs = {"group": group, "product": product, "color1": color1, "color2": color2,
              "profile": profile, "seed": seed, "width": 480, "height": 480,
              "positive_prompt": ", ".join([presets["style_prefix"], item["prompt"], primary, secondary, audience, presets["style"]])}
    return api, to_ui(api, inputs), inputs


def to_ui(api, selection):
    """Version 0.4 workflow format, supported by the installed ComfyUI frontend."""
    outputs = {"CheckpointLoaderSimple": [("MODEL", "MODEL"), ("CLIP", "CLIP"), ("VAE", "VAE")],
               "CLIPTextEncode": [("CONDITIONING", "CONDITIONING")],
               "EmptyLatentImage": [("LATENT", "LATENT")], "KSampler": [("LATENT", "LATENT")],
               "VAEDecode": [("IMAGE", "IMAGE")], "SaveImage": [("IMAGE", "IMAGE")], "ImageScale": [("IMAGE", "IMAGE")],
               "PrimitiveString": [("STRING", "STRING")], "StringConcatenate": [("STRING", "STRING")]}
    widgets = {"CheckpointLoaderSimple": ["ckpt_name"], "CLIPTextEncode": ["text"],
               "EmptyLatentImage": ["width", "height", "batch_size"],
               "KSampler": ["seed", "control_after_generate", "steps", "cfg", "sampler_name", "scheduler", "denoise"],
               "VAEDecode": [], "SaveImage": ["filename_prefix"], "ImageScale": ["upscale_method", "width", "height", "crop"], "PrimitiveString": ["value"],
               "StringConcatenate": ["string_a", "string_b", "delimiter"]}
    positions = {"10": [-900, 0], "11": [-900, 170], "12": [-900, 340], "13": [-900, 510],
                 "14": [-500, 0], "15": [-500, 340], "16": [-80, 0], "17": [330, 0], "18": [330, 300],
                 "1": [750, 620], "2": [750, 0], "3": [750, 310], "4": [1120, 620],
                 "5": [1320, 0], "6": [1690, 0], "8": [2070, 0], "7": [2450, 0]}
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
    return {"last_node_id": 18, "last_link_id": len(links), "nodes": ui_nodes, "links": links,
            "groups": [], "config": {}, "extra": {"project": "Thai_02_Big_Data_in_Our_Lives", "selection": selection}, "version": 0.4}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--group", default="student")
    parser.add_argument("--product", default="bag")
    parser.add_argument("--color1", default="blue")
    parser.add_argument("--color2", default="white")
    parser.add_argument("--profile", default="balanced")
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
