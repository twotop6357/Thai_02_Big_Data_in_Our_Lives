"""Explicit integration check: inject missing palette alpha and execute the real native Comfy graph."""
import io
import json
from pathlib import Path
import sys
import time
import urllib.parse
import urllib.request
import uuid
import numpy as np
from PIL import Image
sys.path.insert(0, str(Path(__file__).resolve().parent))
from alpha_mask_nodes import alpha_nodes
from build_workflow import read_json
from verify_alpha_workflow import http

folder = Path("docs/validation/alpha_opaque/native_synthetic")
folder.mkdir(parents=True, exist_ok=True)
palette = read_json("presets.json")["color_options"]
colors = ["black", "white", "blue", "red", "green", "yellow"]
pixels = np.full((128, 256, 4), [128, 128, 128, 0], dtype=np.uint8)
for index, color in enumerate(colors):
    x = 16 + index*36
    value = palette[color]["hex"].lstrip("#")
    rgb = [int(value[i:i+2], 16) for i in (0, 2, 4)]
    pixels[20:108, x:x+24, :3] = rgb
    pixels[20:108, x:x+24, 3] = 255
    pixels[20:108, x, 3] = 128
    pixels[40:60, x+6:x+18, 3] = 0  # Erroneous removal of a colored/white material patch.
    pixels[78:94, x+8:x+18] = [128, 128, 128, 0]  # Actual opening showing the gray backdrop.
buffer = io.BytesIO()
Image.fromarray(pixels).save(buffer, format="PNG")
(folder/"injected.png").write_bytes(buffer.getvalue())
name = "injected_"+uuid.uuid4().hex+".png"
boundary = uuid.uuid4().hex
body = f'--{boundary}\r\nContent-Disposition: form-data; name="subfolder"\r\n\r\nThai02Alpha\r\n'.encode()
body += f'--{boundary}\r\nContent-Disposition: form-data; name="image"; filename="{name}"\r\nContent-Type: image/png\r\n\r\n'.encode()+buffer.getvalue()+f'\r\n--{boundary}--\r\n'.encode()
request = urllib.request.Request("http://127.0.0.1:8188/upload/image", data=body, headers={"Content-Type": "multipart/form-data; boundary="+boundary})
with urllib.request.urlopen(request, timeout=20) as response:
    uploaded = json.load(response)
source = uploaded["subfolder"]+"/"+uploaded["name"]
report = {"task": "ALPHA-OPAQUE-001", "checks": []}
for bicycle in [False, True]:
    def node(kind, **inputs):
        return {"class_type": kind, "inputs": inputs}
    graph = {"6": node("LoadImage", image=source), "11": node("PrimitiveString", value="bicycle" if bicycle else "bag"),
             "21": node("InvertMask", mask=["6", 1]), "22": node("InvertMask", mask=["60", 0]),
             "23": node("JoinImageWithAlpha", image=["6", 0], alpha=["22", 0]),
             "7": node("SaveImage", images=["23", 0], filename_prefix="Thai02/AlphaSynthetic/output")}
    graph.update(alpha_nodes())
    queued = http("/prompt", {"prompt": graph, "client_id": str(uuid.uuid4())})
    start = time.monotonic()
    while time.monotonic()-start < 30:
        history = http("/history/"+queued["prompt_id"]).get(queued["prompt_id"])
        if history:
            if history["status"]["status_str"] != "success":
                raise RuntimeError(history["status"])
            descriptor = history["outputs"]["7"]["images"][0]
            png = http("/view?"+urllib.parse.urlencode(descriptor), binary=True)
            (folder/("bicycle.png" if bicycle else "repaired.png")).write_bytes(png)
            actual = np.asarray(Image.open(io.BytesIO(png)))
            assert np.array_equal(actual[:, :, :3], pixels[:, :, :3]), "RGB changed"
            for index, color in enumerate(colors):
                x = 16+index*36
                assert np.all(actual[40:60, x+6:x+18, 3] == (0 if bicycle else 255)), color+" wrong material alpha"
                assert np.all(actual[78:94, x+8:x+18, 3] == 0), color+" real gap filled"
                assert np.all(actual[25:100, x, 3] == 128), color+" antialiased boundary changed"
                report["checks"].append({"color": color, "bicycle": bicycle, "missing_alpha_repaired_or_intentionally_preserved": True, "real_opening_preserved": True, "edge_alpha_128_preserved": True})
            break
        time.sleep(.1)
    else:
        raise TimeoutError(queued)
report["passed"] = True
(folder/"report.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
print("Actual native graph: all six palette colors repaired; real gaps/edge alpha and bicycle openings preserved.")
