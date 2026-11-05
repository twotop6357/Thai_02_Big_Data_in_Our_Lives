"""Verify a copied interpreter/settings in a second Unicode/spaced path without duplicating model bytes."""
import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess

parser = argparse.ArgumentParser()
parser.add_argument("package", type=Path)
parser.add_argument("--report", type=Path, required=True)
args = parser.parse_args()
root = args.package.resolve(strict=True)
stage = root / "_RelocationCheck 이동 테스트"
if stage.exists():
    raise FileExistsError("Inspect the existing relocation check folder before reusing it.")
stage.mkdir()
runtime = root / "ComfyRuntime"
manifest = json.loads((runtime / "package_manifest.json").read_text(encoding="utf-8"))
shutil.copytree(runtime, stage / "ComfyRuntime", copy_function=os.link,
                ignore=shutil.ignore_patterns("__pycache__", "cache", "logs", "output", "user", "temp"))
os.link(root / manifest["player"], stage / manifest["player"])
settings = Path(manifest["player"]).stem + "_Data/StreamingAssets/BlockGeneration/ComfyUISettings.json"
(stage / settings).parent.mkdir(parents=True)
shutil.copy2(root / settings, stage / settings)
python = stage / "ComfyRuntime/python/python.exe"
completed = subprocess.run([str(python), "-I", str(stage / "ComfyRuntime/portable_boot.py"), "--root", str(stage), "--diagnose"],
                           cwd=os.environ["SystemRoot"], capture_output=True, text=True, encoding="utf-8", timeout=120)
if completed.returncode:
    raise RuntimeError(completed.stdout + completed.stderr)
diagnostic = json.loads((stage / "ComfyRuntime/logs/diagnostics.json").read_text(encoding="utf-8"))
prepared = json.loads((stage / settings).read_text(encoding="utf-8"))
assert diagnostic["prefix"] == str(stage / "ComfyRuntime/python")
assert prepared["serverExecutable"] == str(python)
assert prepared["serverWorkingDirectory"] == str(stage / "ComfyRuntime")
assert str(stage / "ComfyRuntime/portable_server.py") in prepared["serverArguments"]
args.report.parent.mkdir(parents=True, exist_ok=True)
args.report.write_text(json.dumps({"passed": True, "method": "Separate directory tree; file contents hard-linked for storage only; no junctions",
                                  "unicode_and_spaces": True, "working_directory": os.environ["SystemRoot"],
                                  "diagnostic": diagnostic, "server_arguments": prepared["serverArguments"]}, indent=2), encoding="utf-8")
print("Relocated interpreter, CUDA calculation and regenerated paths passed. Remove only the explicit test folder after review.")
