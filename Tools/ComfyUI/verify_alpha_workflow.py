"""Generate all products/colors and verify mask invariants against the same RGB before repair."""
import argparse
import json
from pathlib import Path
import sys
import time
import urllib.parse
import urllib.request
import uuid
import numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage as ndi
sys.path.insert(0, str(Path(__file__).resolve().parent))
from build_workflow import build, read_json
from prepare_style_reference import ensure

SERVER = "http://127.0.0.1:8188"


def http(path, data=None, binary=False):
    payload = None if data is None else json.dumps(data).encode()
    request = urllib.request.Request(SERVER+path, data=payload,
                                     headers={"Content-Type": "application/json"} if data is not None else {})
    with urllib.request.urlopen(request, timeout=20) as response:
        return response.read() if binary else json.load(response)


def graph(selection, replay_descriptor=None):
    product, group, first, second, seed = selection
    api, _, meta = build(group, product, first, second, seed=seed)
    def node(kind, **inputs):
        return {"class_type": kind, "inputs": inputs}
    if replay_descriptor:
        source = replay_descriptor["subfolder"]+"/"+replay_descriptor["filename"]+" [output]"
        api["6"] = node("LoadImage", image=source)
    api["7"]["inputs"]["filename_prefix"] = "Thai02/AlphaOpaque/result"
    api["90"] = node("InvertMask", mask=["21", 0])
    api["91"] = node("JoinImageWithAlpha", image=["6", 0], alpha=["90", 0])
    api["92"] = node("ImageScale", image=["91", 0], upscale_method="area", width=480, height=480, crop="disabled")
    for key, image in [("93", ["92", 0]), ("94", ["23", 0]), ("95", ["91", 0])]:
        api[key] = node("SaveImage", images=image, filename_prefix="Thai02/AlphaOpaque/check"+key)
    api["97"] = node("MaskToImage", mask=["59", 0])
    api["98"] = node("SaveImage", images=["97", 0], filename_prefix="Thai02/AlphaOpaque/core")
    api["100"] = node("MaskToImage", mask=["63", 0])
    api["99"] = node("SaveImage", images=["100", 0], filename_prefix="Thai02/AlphaOpaque/material")
    return api, meta


def render(api, folder, tag):
    queue = http("/queue")
    if queue["queue_running"] or queue["queue_pending"]:
        raise RuntimeError("Shared ComfyUI server is busy; no job submitted.")
    start = time.perf_counter()
    queued = http("/prompt", {"prompt": api, "client_id": str(uuid.uuid4())})
    if "prompt_id" not in queued:
        raise RuntimeError(queued)
    while time.perf_counter() - start < 180:
        history = http("/history/"+queued["prompt_id"]).get(queued["prompt_id"])
        if history:
            if history["status"]["status_str"] != "success":
                raise RuntimeError(history["status"])
            files = {}
            for key in ["7", "93", "94", "95", "98", "99"]:
                descriptor = history["outputs"][key]["images"][0]
                name = tag + "_" + key + ".png"
                (folder/name).write_bytes(http("/view?"+urllib.parse.urlencode(descriptor), binary=True))
                files[key] = name
            return files, round(time.perf_counter()-start, 3), queued["prompt_id"]
        time.sleep(.15)
    raise TimeoutError(queued)


def inspect(folder, files, bicycle=False):
    final, before, native, raw, core, material = [np.asarray(Image.open(folder/files[key]).convert("RGBA"))
                                                for key in ["7", "93", "94", "95", "98", "99"]]
    core_mask = core[:, :, 0] == 255
    material_core = ndi.binary_erosion(material[:, :, 0] == 255)
    rgb = final[:, :, :3]
    alpha = final[:, :, 3]
    white = ndi.binary_erosion((rgb.min(axis=2) > 225) & (np.ptp(rgb, axis=2) < 25), iterations=2)
    colorful = ndi.binary_erosion(np.ptp(rgb, axis=2) > 40, iterations=2)
    white_loss = white & (alpha < 250)
    color_loss = colorful & (alpha < 250)
    white_regions, white_count = ndi.label(white_loss)
    color_regions, color_count = ndi.label(color_loss)
    largest = lambda labels, count: max([int((labels == i).sum()) for i in range(1, count+1)], default=0)
    return {"rgb_unchanged_480": bool(np.array_equal(final[:, :, :3], before[:, :, :3])),
            "rgb_unchanged_native": bool(np.array_equal(native[:, :, :3], raw[:, :, :3])),
            "native_background_zero_preserved": bool(np.all(native[:, :, 3][(raw[:, :, 3] == 0) & ((material[:, :, 0] == 0) | bicycle)] == 0)),
            "native_material_all_opaque": bicycle or bool(np.all(native[:, :, 3][material_core] == 255)),
            "native_outside_core_unchanged": bool(np.array_equal(native[:, :, 3][~core_mask], raw[:, :, 3][~core_mask])),
            "native_alpha_never_reduced": bool(np.all(native[:, :, 3] >= raw[:, :, 3])),
            "native_core_all_opaque": bool(np.all(native[:, :, 3][core_mask] == 255)),
            "native_core_pixels": int(core_mask.sum()), "white_core_pixels": int(white.sum()),
            "lost_white_core_pixels": int(white_loss.sum()), "largest_lost_white_region": largest(white_regions, white_count),
            "lost_colored_core_pixels": int(color_loss.sum()), "largest_lost_color_region": largest(color_regions, color_count),
            "corners_transparent": bool(np.all(alpha[[0, 0, -1, -1], [0, -1, 0, -1]] == 0))}


def sheets(folder, rows):
    for start in range(0, len(rows), 30):
        selected = rows[start:start+30]
        cell = 230
        canvas = Image.new("RGB", (cell*5, (len(selected)+4)//5*(cell+42)), "#252b39")
        draw = ImageDraw.Draw(canvas)
        for index, row in enumerate(selected):
            x, y = index%5*cell, index//5*(cell+42)
            bg = Image.new("RGBA", (cell, cell), "#65617f")
            bgd = ImageDraw.Draw(bg)
            for yy in range(0, cell, 20):
                for xx in range(0, cell, 20):
                    if (xx//20+yy//20)%2:
                        bgd.rectangle((xx, yy, xx+19, yy+19), fill="#424a5a")
            bg.alpha_composite(Image.open(folder/row["files"]["7"]).convert("RGBA").resize((cell, cell)))
            canvas.paste(bg.convert("RGB"), (x, y))
            draw.text((x+4, y+cell), row["tag"][:32], fill="white")
            draw.text((x+4, y+cell+17), f'white/color lost max: {row["checks"]["largest_lost_white_region"]}/{row["checks"]["largest_lost_color_region"]}', fill="white")
        canvas.save(folder/f"sheet_{start//30+1}.png")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--limit", type=int)
    parser.add_argument("--replay", type=Path)
    parser.add_argument("--smoke-white", action="store_true")
    parser.add_argument("--end-to-end", action="store_true")
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=True)
    ensure(SERVER)
    catalog = read_json("presets.json")
    products, groups = list(catalog["products"]), list(catalog["groups"])
    colors = ["black", "white", "blue", "red", "green", "yellow"]
    cases = []
    for index, product in enumerate(products):
        for color_index, color in enumerate(colors):
            cases.append((product, groups[(index+color_index)%4], color, "white", 2026110200+index*6+color_index))
    for index, product in enumerate(products):
        for second_index, second in enumerate(["blue", "black"]):
            cases.append((product, groups[(index+second_index+1)%4], "white", second, 2026111200+index*2+second_index))
        cases.append((product, groups[index%4], "red", "blue", 2026112200+index))
    if args.limit:
        cases = cases[:args.limit]
    replay = {}
    if args.smoke_white:
        cases = [(product, groups[i%4], "white", "white", 2026113200+i) for i, product in enumerate(products)]
    if args.end_to_end:
        cases = [("bags", "working_adult", "white", "blue", 2026114200),
                 ("wristwatches", "working_adult", "white", "black", 2026114201),
                 ("sunglasses", "senior", "red", "blue", 2026114202),
                 ("umbrellas", "student", "white", "white", 2026114203)]
    if args.replay:
        old_rows = json.loads(args.replay.read_text(encoding="utf-8"))["results"]
        cases = [(r["selection"]["product"], r["selection"]["group"], r["selection"]["color1"], r["selection"]["color2"], r["selection"]["seed"]) for r in old_rows]
        replay = {case: row for case, row in zip(cases, old_rows)}
    report = {"task": "ALPHA-OPAQUE-001", "device": http("/system_stats")["devices"], "results": []}
    for selection in cases:
        product, group, first, second, seed = selection
        tag = f"{product}_{group}_{first}_{second}_{seed}"
        if any(row["tag"] == tag for row in report["results"]):
            continue
        descriptor = None
        if selection in replay:
            pid = replay[selection]["prompt_id"]
            descriptor = http("/history/"+pid)[pid]["outputs"]["95"]["images"][0]
        api, meta = graph(selection, descriptor)
        files, seconds, prompt = render(api, args.output, tag)
        checks = inspect(args.output, files, bicycle=product=="bicycles")
        row = {"tag": tag, "selection": meta, "files": files, "seconds": seconds, "prompt_id": prompt, "checks": checks}
        report["results"].append(row)
        print(f'{len(report["results"])}/{len(cases)} {tag} {seconds}s white/color:{checks["largest_lost_white_region"]}/{checks["largest_lost_color_region"]}', flush=True)
        (args.output/"report.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
        if len(report["results"])%10 == 0:
            sheets(args.output, report["results"])
    invariant_keys = ["rgb_unchanged_480", "rgb_unchanged_native", "native_background_zero_preserved", "native_material_all_opaque", "native_outside_core_unchanged", "native_alpha_never_reduced", "native_core_all_opaque", "corners_transparent"]
    report["invariants_passed"] = all(all(row["checks"][key] for key in invariant_keys) for row in report["results"])
    report["review_flags"] = [row["tag"] for row in report["results"] if row["checks"]["largest_lost_white_region"] > 16 or row["checks"]["largest_lost_color_region"] > 16]
    (args.output/"report.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    sheets(args.output, report["results"])
    print("Invariants:", report["invariants_passed"], "review flags:", report["review_flags"], flush=True)
