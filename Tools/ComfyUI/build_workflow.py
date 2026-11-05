"""Build ComfyUI UI/API workflows from audience, product and two named colors. No custom nodes."""
import argparse
import json
from pathlib import Path
from alpha_mask_nodes import alpha_nodes

ROOT = Path(__file__).resolve().parent


def read_json(name):
    data = json.loads((ROOT / name).read_text(encoding="utf-8"))
    if name == "presets.json":
        catalog = json.loads((ROOT / data["catalog"]).read_text(encoding="utf-8"))
        data.update(style_prefix=catalog["stylePrefix"], style=catalog["styleSuffix"], negative=catalog["negativePrompt"],
                    prompt_separator=catalog.get("promptSeparator", ", "), numeric_color_prompt=catalog.get("numericColorPrompt", False),
                    groups={item["id"]: item for item in catalog["groups"]}, colors=[item["id"] for item in catalog["colors"]],
                    products={item["id"]: dict(item, primary="{color1} " + item["primaryPart"],
                        secondary="{color2} " + item["secondaryPart"]) for item in catalog["products"]},
                    equipment_variants=catalog["equipmentVariants"], color_options={item["id"]: item for item in catalog["colors"]})
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
    variants = item.get("ageVariants", [])
    if variants:
        if (len(variants) != len(presets["groups"]) or
                {v["groupId"] for v in variants} != set(presets["groups"]) or
                any(not v.get(key, "").strip() for v in variants
                    for key in ("label", "prompt", "primaryPart", "secondaryPart"))):
            raise ValueError("Invalid or incomplete product age designs: " + product)
        design = next(v for v in variants if v["groupId"] == group)
    else:
        design = item
    audience = presets["groups"][group]["prompt"]
    primary = presets["color_options"][color1]["prompt"] + " " + design["primaryPart"]
    secondary = presets["color_options"][color2]["prompt"] + " " + design["secondaryPart"]
    if presets["numeric_color_prompt"]:
        primary = "Main color " + presets["color_options"][color1]["hex"].upper() + " (" + presets["color_options"][color1]["prompt"] + ") on " + design["primaryPart"]
        secondary = "Accent color " + presets["color_options"][color2]["hex"].upper() + " (" + presets["color_options"][color2]["prompt"] + ") on " + design["secondaryPart"]
    def reference(color):
        code = presets["color_options"][color]["hex"].upper()
        rgb = ", ".join(str(int(code[i:i+2], 16)) for i in (1, 3, 5))
        return f"{code} RGB({rgb})"
    sampling = presets["profiles"][profile]
    negative = presets["negative"] + (", " + item["negativePrompt"] if item.get("negativePrompt") else "")
    subject = design["prompt"]
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
        "5": node("SamplerCustom", f"Turbo / {sampling['steps']} steps", model=["1", 0], add_noise=True,
                  noise_seed=seed, cfg=1.0, positive=["2", 0], negative=["2", 0],
                  sampler=["28", 0], sigmas=["29", 0], latent_image=["4", 0]),
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
        "17": node("StringConcatenate", "Product first, then illustration style", string_a=["16", 0], string_b=presets["style_prefix"], delimiter=", "),
        "18": node("StringConcatenate", "Fixed product composition", string_a=["17", 0], string_b=presets["style"], delimiter=", "),
        "20": node("LoadBackgroundRemovalModel", "BiRefNet foreground segmentation", bg_removal_name=read_json("background_model.json")["filename"]),
        "21": node("RemoveBackground", "Foreground mask at native resolution", bg_removal_model=["20", 0], image=["6", 0]),
        "22": node("InvertMask", "Foreground to Comfy alpha convention", mask=["60", 0]),
        "23": node("JoinImageWithAlpha", "Apply repaired alpha to unchanged decoded RGB", image=["6", 0], alpha=["22", 0])
    }
    api.update(alpha_nodes())
    api["12"]["_meta"]["color_reference"] = reference(color1)
    api["13"]["_meta"]["color_reference"] = reference(color2)
    api["28"] = node("KSamplerSelect", "Turbo sampler", sampler_name=sampling["sampler"])
    api["29"] = node("SDTurboScheduler", "Native Turbo timestep schedule", model=["1", 0], steps=sampling["steps"], denoise=1.0)
    if model.get("active_mode") == "reference_illustration":
        ref = model["style_reference"]
        api["1"] = node("UNETLoader", model["name"], unet_name=model["filename"], weight_dtype="default")
        api["30"] = node("CLIPLoader", "Qwen3 4B language encoder", clip_name=read_json("text_encoder.json")["filename"], type="flux2", device="default")
        api["31"] = node("VAELoader", "Klein VAE", vae_name=read_json("vae.json")["filename"])
        api["2"]["inputs"]["clip"] = ["30", 0]
        api["3"] = node("ConditioningZeroOut", "Unused negative conditioning at CFG1", conditioning=["2", 0])
        api["4"] = node("EmptyFlux2LatentImage", f"{width} x {height} canvas", width=width, height=height, batch_size=1)
        api["29"] = node("Flux2Scheduler", "Distilled Klein / 4 steps", steps=sampling["steps"], width=width, height=height)
        api["32"] = node("LoadImage", "Original user art style only", image=ref["subfolder"] + "/" + ref["filename"])
        api["33"] = node("ImageScale", "Small art style reference", image=["32", 0], upscale_method="lanczos", width=ref["width"], height=ref["height"], crop="disabled")
        if ref.get("grayscale"):
            api["38"] = node("ImageToMask", "Remove reference colors, retain drawing style", image=["32", 0], channel="green")
            api["39"] = node("MaskToImage", "Neutral art style reference", mask=["38", 0])
            api["33"]["inputs"]["image"] = ["39", 0]
        api["34"] = node("VAEEncode", "Encode art style reference", pixels=["33", 0], vae=["31", 0])
        api["35"] = node("ReferenceLatent", "Reference illustration conditioning", conditioning=["2", 0], latent=["34", 0])
        api["36"] = node("RandomNoise", "Session seed", noise_seed=seed)
        api["37"] = node("BasicGuider", "Reference art guidance", model=["1", 0], conditioning=["35", 0])
        api["5"] = node("SamplerCustomAdvanced", "Single illustration generation / 4 steps", noise=["36", 0], guider=["37", 0], sampler=["28", 0], sigmas=["29", 0], latent_image=["4", 0])
        api["6"]["inputs"]["vae"] = ["31", 0]
    for key in ("14", "15", "16", "17", "18"):
        api[key]["inputs"]["delimiter"] = presets["prompt_separator"]
    if item.get("poseGuide"):
        api["24"] = node("LoadImage", "Limited natural pose reference", image="Thai02Quality/" + item["poseGuide"])
        api["25"] = node("ControlNetLoader", "Lightweight product pose control", control_net_name=read_json("control_model.json")["filename"])
        api["27"] = node("Canny", "Product reference edges", image=["24", 0], low_threshold=item["poseLowThreshold"], high_threshold=item["poseHighThreshold"])
        api["26"] = node("ControlNetApplyAdvanced", "Loose pose during early sampling only", positive=["2", 0], negative=["3", 0],
            control_net=["25", 0], image=["27", 0], vae=["1", 2], strength=item["poseStrength"], start_percent=0, end_percent=item["poseEndPercent"])
        api["5"]["inputs"].update(positive=["26", 0], negative=["26", 1])
    inputs = {"group": group, "product": product, "color1": color1, "color2": color2,
              "primary_hex": presets["color_options"][color1]["hex"], "secondary_hex": presets["color_options"][color2]["hex"],
              "profile": profile, "seed": seed, "width": 480, "height": 480,
              "resolved_product": resolved, "alpha": True,
              "positive_prompt": presets["prompt_separator"].join([subject, primary, secondary, audience, presets["style_prefix"], presets["style"]])}
    return api, to_ui(api, inputs), inputs


def to_ui(api, selection):
    """Version 0.4 workflow format, supported by the installed ComfyUI frontend."""
    outputs = {"CheckpointLoaderSimple": [("MODEL", "MODEL"), ("CLIP", "CLIP"), ("VAE", "VAE")],
               "CLIPTextEncode": [("CONDITIONING", "CONDITIONING")],
               "EmptyLatentImage": [("LATENT", "LATENT")], "KSampler": [("LATENT", "LATENT")],
               "SamplerCustom": [("output", "LATENT"), ("denoised_output", "LATENT")],
               "SamplerCustomAdvanced": [("output", "LATENT"), ("denoised_output", "LATENT")],
               "UNETLoader": [("MODEL", "MODEL")], "CLIPLoader": [("CLIP", "CLIP")], "VAELoader": [("VAE", "VAE")],
               "EmptyFlux2LatentImage": [("LATENT", "LATENT")], "Flux2Scheduler": [("SIGMAS", "SIGMAS")],
               "ConditioningZeroOut": [("CONDITIONING", "CONDITIONING")], "VAEEncode": [("LATENT", "LATENT")],
               "ReferenceLatent": [("CONDITIONING", "CONDITIONING")], "BasicGuider": [("GUIDER", "GUIDER")], "RandomNoise": [("NOISE", "NOISE")],
               "ImageToMask": [("MASK", "MASK")], "MaskToImage": [("IMAGE", "IMAGE")],
               "KSamplerSelect": [("SAMPLER", "SAMPLER")], "SDTurboScheduler": [("SIGMAS", "SIGMAS")],
               "VAEDecode": [("IMAGE", "IMAGE")], "SaveImage": [("IMAGE", "IMAGE")], "ImageScale": [("IMAGE", "IMAGE")],
               "PrimitiveString": [("STRING", "STRING")], "StringConcatenate": [("STRING", "STRING")],
               "LoadBackgroundRemovalModel": [("bg_model", "BACKGROUND_REMOVAL")],
               "RemoveBackground": [("mask", "MASK")], "InvertMask": [("MASK", "MASK")], "JoinImageWithAlpha": [("IMAGE", "IMAGE")],
               "ThresholdMask": [("MASK", "MASK")], "GrowMask": [("MASK", "MASK")], "MaskComposite": [("MASK", "MASK")],
               "StringContains": [("BOOLEAN", "BOOLEAN")], "ComfySwitchNode": [("output", "MASK")],
               "LoadImage": [("IMAGE", "IMAGE"), ("MASK", "MASK")], "ControlNetLoader": [("CONTROL_NET", "CONTROL_NET")],
               "ControlNetApplyAdvanced": [("positive", "CONDITIONING"), ("negative", "CONDITIONING")], "Canny": [("IMAGE", "IMAGE")]}
    widgets = {"CheckpointLoaderSimple": ["ckpt_name"], "CLIPTextEncode": ["text"],
               "EmptyLatentImage": ["width", "height", "batch_size"],
               "KSampler": ["seed", "control_after_generate", "steps", "cfg", "sampler_name", "scheduler", "denoise"],
               "SamplerCustom": ["add_noise", "noise_seed", "control_after_generate", "cfg"],
               "SamplerCustomAdvanced": [], "UNETLoader": ["unet_name", "weight_dtype"], "CLIPLoader": ["clip_name", "type", "device"], "VAELoader": ["vae_name"],
               "EmptyFlux2LatentImage": ["width", "height", "batch_size"], "Flux2Scheduler": ["steps", "width", "height"],
               "ConditioningZeroOut": [], "VAEEncode": [], "ReferenceLatent": [], "BasicGuider": [], "RandomNoise": ["noise_seed", "control_after_generate"],
               "ImageToMask": ["channel"], "MaskToImage": [],
               "KSamplerSelect": ["sampler_name"], "SDTurboScheduler": ["steps", "denoise"],
               "VAEDecode": [], "SaveImage": ["filename_prefix"], "ImageScale": ["upscale_method", "width", "height", "crop"], "PrimitiveString": ["value"],
               "StringConcatenate": ["string_a", "string_b", "delimiter"], "LoadBackgroundRemovalModel": ["bg_removal_name"],
               "RemoveBackground": [], "InvertMask": [], "JoinImageWithAlpha": [], "LoadImage": ["image"],
               "ThresholdMask": ["value"], "GrowMask": ["expand", "tapered_corners"], "MaskComposite": ["x", "y", "operation"],
               "StringContains": ["string", "substring", "case_sensitive"], "ComfySwitchNode": [],
               "ControlNetLoader": ["control_net_name"], "ControlNetApplyAdvanced": ["strength", "start_percent", "end_percent"], "Canny": ["low_threshold", "high_threshold"]}
    positions = {"10": [-900, 0], "11": [-900, 170], "12": [-900, 340], "13": [-900, 510],
                 "14": [-500, 0], "15": [-500, 340], "16": [-80, 0], "17": [330, 0], "18": [330, 300],
                 "1": [750, 620], "2": [750, 0], "3": [750, 310], "4": [1120, 620],
                 "5": [1320, 0], "6": [1690, 0], "20": [1690, 350], "21": [2070, 350],
                 "22": [2450, 350], "23": [2830, 0], "8": [3210, 0], "7": [3590, 0],
                 "24": [750, 950], "25": [1120, 950], "26": [1120, 310], "27": [750, 1200],
                 "28": [750, 920], "29": [1120, 920]}
    positions.update({"30": [750, 1180], "31": [1120, 1180], "32": [-900, 800], "33": [-500, 800],
                      "34": [-80, 800], "35": [330, 800], "36": [1120, 1420], "37": [750, 1420]})
    positions.update({"38": [-900, 1090], "39": [-500, 1090]})
    positions.update({str(key): [1690+(key-40)%4*380, 970+(key-40)//4*240] for key in range(40, 64)})
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
