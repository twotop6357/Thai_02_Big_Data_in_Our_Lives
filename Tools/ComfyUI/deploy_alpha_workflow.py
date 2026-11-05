"""Patch only alpha nodes/backdrop in an existing build; preserve its binary, RFID and all selection data."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import zipfile

ROOT = Path(__file__).resolve().parents[2]
parser = argparse.ArgumentParser()
parser.add_argument("build", type=Path)
args = parser.parse_args()
build = args.build.resolve(strict=True)
data_dirs = [path for path in build.glob("*_Data") if (path/"StreamingAssets/BlockGeneration/BlockCatalog.json").exists()]
if len(data_dirs) != 1:
    raise ValueError("Expected one Unity Data directory.")
data = data_dirs[0]
assets = data/"StreamingAssets/BlockGeneration"
source = ROOT/"Assets/StreamingAssets/BlockGeneration"
current_api = json.loads((source/"product_480.api.json").read_text(encoding="utf-8"))
source_catalog = json.loads((source/"BlockCatalog.json").read_text(encoding="utf-8"))
backup = build/"Workflow_Backup_ALPHA_OPAQUE_001"
backup.mkdir(exist_ok=False)
protected = [build/(data.name[:-5]+".exe"), build/"UnityPlayer.dll", data/"Managed/Assembly-CSharp.dll",
             data/"StreamingAssets/Settings.json", data/"StreamingAssets/RFID/RfidSettings.json"]
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
before = {str(path.relative_to(build)): sha(path) for path in protected if path.is_file()}
for filename in ["BlockCatalog.json", "product_480.api.json"]:
    shutil.copy2(assets/filename, backup/filename)
catalog_path = assets/"BlockCatalog.json"
catalog_text = catalog_path.read_bytes().decode("utf-8-sig")
catalog = json.loads(catalog_text)
old_suffix = catalog["styleSuffix"]
new_suffix = old_suffix.replace("Plain white background", "Flat uniform medium-gray background (#808080), opaque object surfaces")
if new_suffix == old_suffix and "medium-gray background (#808080)" not in old_suffix:
    raise ValueError("Unrecognized backdrop prompt; the custom build was not overwritten.")
new_catalog = json.loads(catalog_text.replace(json.dumps(old_suffix, ensure_ascii=False), json.dumps(new_suffix, ensure_ascii=False), 1))
assert {key: value for key, value in catalog.items() if key != "styleSuffix"} == {key: value for key, value in new_catalog.items() if key != "styleSuffix"}
graph_path = assets/"product_480.api.json"
graph = json.loads(graph_path.read_text(encoding="utf-8-sig"))
mask_nodes = {key: value for key, value in current_api.items() if int(key) >= 40}
if any(key in graph and graph[key] != value for key, value in mask_nodes.items()):
    raise ValueError("Alpha node IDs overlap unknown build changes; inspect before applying.")
graph.update(mask_nodes)
graph["22"]["inputs"]["mask"] = current_api["22"]["inputs"]["mask"]
graph["18"]["inputs"]["string_b"] = new_suffix
def replace_file(path, text):
    temporary = path.with_suffix(path.suffix+".tmp")
    temporary.write_text(text, encoding="utf-8")
    os.replace(temporary, path)
replace_file(catalog_path, catalog_text.replace(json.dumps(old_suffix, ensure_ascii=False), json.dumps(new_suffix, ensure_ascii=False), 1))
replace_file(graph_path, json.dumps(graph, ensure_ascii=False, indent=2)+"\n")
assert all(sha(build/file) == expected for file, expected in before.items())
report = {"build": str(build), "protected_files_unchanged": before, "catalog_only_style_suffix_changed": True,
          "updated_files": {filename: sha(assets/filename) for filename in ["BlockCatalog.json", "product_480.api.json"]}}
evidence = ROOT/"docs/validation/alpha_opaque"
(evidence/"deployment.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
archive = ROOT/"Builds/Alpha_Workflow_Update.zip"
with zipfile.ZipFile(archive, "x", zipfile.ZIP_DEFLATED) as output:
    for filename in ["BlockCatalog.json", "product_480.api.json"]:
        output.write(assets/filename, str(Path(data.name)/"StreamingAssets/BlockGeneration"/filename))
    output.writestr("README.txt", "프로그램 종료 후 압축 안의 *_Data 폴더를 기존 빌드 폴더에 덮어쓰세요.\n변경 파일은 BlockGeneration의 JSON 2개뿐입니다. Comfy/모델 재설치나 Player 재빌드가 필요 없습니다.\n")
print("Patched two JSON files, preserved existing build/settings, and created", archive)
