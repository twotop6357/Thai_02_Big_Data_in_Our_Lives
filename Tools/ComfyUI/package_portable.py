"""Copy the validated local Comfy environment beside an existing Player; no host installs or updates."""
import argparse
import concurrent.futures
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess

ROOT = Path(__file__).resolve().parents[2]


def copy_tree(source, destination, excludes=(), exclude_files=()):
    command = ["robocopy", str(source), str(destination), "/E", "/R:1", "/W:1", "/MT:8",
               "/NFL", "/NDL", "/NJH", "/NJS", "/NP", "/XJ"]
    if excludes:
        command += ["/XD", *map(str, excludes)]
    if exclude_files:
        command += ["/XF", *exclude_files]
    result = subprocess.run(command, capture_output=True)
    if result.returncode >= 8:
        raise RuntimeError(f"Copy failed ({result.returncode}): {source}\n{result.stdout.decode(errors='replace')}")
    print(f"Copied: {destination.name}", flush=True)


def sha(path):
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(8 * 1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def package(destination):
    destination = destination.resolve(strict=True)
    players = [path for path in destination.glob("*.exe") if (destination / (path.stem + "_Data")).is_dir()]
    if len(players) != 1:
        raise ValueError("Expected exactly one Unity Player and matching Data folder.")
    player = players[0]
    source = Path(os.environ["LOCALAPPDATA"]) / "Comfy-Desktop/ComfyUI-Installs/ComfyUI"
    shared = Path(os.environ["LOCALAPPDATA"]) / "Comfy-Desktop/ComfyUI-Shared/models"
    runtime = destination / "ComfyRuntime"
    if runtime.exists():
        raise FileExistsError("ComfyRuntime already exists; inspect it before replacing any packaged runtime.")
    runtime.mkdir()
    baseline = {str(path.relative_to(destination)): sha(path) for path in [
        player, destination / "UnityPlayer.dll", destination / (player.stem + "_Data/Managed/Assembly-CSharp.dll"),
        destination / (player.stem + "_Data/StreamingAssets/Settings.json"),
        destination / (player.stem + "_Data/StreamingAssets/RFID/RfidSettings.json")
    ] if path.exists()}
    with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:
        jobs = [
            pool.submit(copy_tree, source / "standalone-env", runtime / "python",
                        [source / "standalone-env/Lib/site-packages", "__pycache__"], ["*.pyc", "*.pdb", "uv.exe"]),
            pool.submit(copy_tree, source / "ComfyUI/.venv/Lib/site-packages", runtime / "python/Lib/site-packages",
                        ["__pycache__"], ["*.pyc", "_virtualenv.pth", "_virtualenv.py"]),
            pool.submit(copy_tree, source / "ComfyUI", runtime / "ComfyUI",
                        ["__pycache__", *[source / "ComfyUI" / name for name in
                         [".git", ".venv", "custom_nodes", "input", "output", "temp", "user", "models",
                          "tests", "tests-unit", ".github", ".ci"]]],
                        ["*.pyc", ".env", ".comfy_environment", "extra_model_paths.yaml"])
        ]
        for job in jobs:
            job.result()
    # A base interpreter plus the exact tested site-packages, isolated from installed Python/user packages.
    (runtime / "python/python313._pth").write_text(".\nLib\nDLLs\nLib\\site-packages\n..\\ComfyUI\nimport site\n", encoding="ascii")
    licenses = runtime / "licenses"
    licenses.mkdir()
    for name in ["flux2-klein-LICENSE.md", "NOTICE.txt"]:
        shutil.copy2(ROOT / "Tools/ComfyUI/licenses" / name, licenses / name)
    shutil.copy2(source / "ComfyUI/LICENSE", licenses / "ComfyUI-GPL-3.0.txt")
    provenance = {"task": "COMFY-PORTABLE-001", "player": player.name,
                  "python": "3.13.12", "torch": "2.11.0+cu128", "comfy": "0.42.0",
                  "models": [], "preserved_player_files": baseline}
    for manifest_name, category in [("model.json", "diffusion_models"), ("text_encoder.json", "text_encoders"),
                                    ("vae.json", "vae"), ("background_model.json", "background_removal")]:
        manifest = json.loads((ROOT / "Tools/ComfyUI" / manifest_name).read_text(encoding="utf-8"))
        target = runtime / "models" / category / manifest["filename"]
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(shared / category / manifest["filename"], target)
        actual = sha(target)
        if target.stat().st_size != manifest["bytes"] or actual != manifest["sha256"]:
            raise RuntimeError(f"Model size/hash mismatch: {target.name}")
        shutil.copy2(ROOT / "Tools/ComfyUI" / manifest_name, licenses / manifest_name)
        provenance["models"].append({"path": str(target.relative_to(runtime)), "bytes": target.stat().st_size, "sha256": actual})
        print(f"Verified model: {target.name}", flush=True)
    for name in ["input", "output", "user", "temp", "logs", "prerequisites"]:
        (runtime / name).mkdir()
    style = ROOT / "Assets/StreamingAssets/BlockGeneration/StyleReferences/style_user_fa1ec41434dabe24.png"
    (runtime / "input/Thai02Style3").mkdir()
    shutil.copy2(style, runtime / "input/Thai02Style3" / style.name)
    shutil.copy2(ROOT / "Tools/ComfyUI/portable_boot.py", runtime / "portable_boot.py")
    shutil.copy2(ROOT / "Tools/ComfyUI/portable_start.ps1", runtime / "portable_start.ps1")
    shutil.copy2(ROOT / "Tools/ComfyUI/portable_server.py", runtime / "portable_server.py")
    for name in ["Run_Thai02.cmd", "Check_Runtime.cmd"]:
        shutil.copy2(ROOT / "Tools/ComfyUI" / name, destination / name)
    shutil.copy2(ROOT / "Tools/ComfyUI/PORTABLE_README_KO.txt", destination / "README_먼저읽기.txt")
    for file, expected in baseline.items():
        if sha(destination / file) != expected:
            raise RuntimeError(f"Existing Player file changed unexpectedly: {file}")
    (runtime / "package_manifest.json").write_text(json.dumps(provenance, ensure_ascii=False, indent=2), encoding="utf-8")
    print("Portable environment copied and all four model hashes verified.", flush=True)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("destination", type=Path)
    package(parser.parse_args().destination)
