"""Portable settings preparation and diagnostics; launched by the included PowerShell wrapper."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import sys


def prepare(root):
    runtime = root / "ComfyRuntime"
    manifest = json.loads((runtime / "package_manifest.json").read_text(encoding="utf-8"))
    player = root / manifest["player"]
    if not player.is_file():
        raise FileNotFoundError(player)
    settings_path = root / (player.stem + "_Data/StreamingAssets/BlockGeneration/ComfyUISettings.json")
    settings = json.loads(settings_path.read_text(encoding="utf-8-sig"))
    backup = runtime / "ComfyUISettings.original.json"
    if not backup.exists():
        backup.write_bytes(settings_path.read_bytes())
    for folder in ["input", "output", "user", "temp", "logs"]:
        (runtime / folder).mkdir(exist_ok=True)
    for model in manifest["models"]:
        path = runtime / model["path"]
        if not path.is_file() or path.stat().st_size != model["bytes"]:
            raise RuntimeError(f"Required model is missing or incomplete: {path.name}")
    arguments = ["-s", str(runtime / "portable_server.py"), "--listen", "127.0.0.1", "--port", "8188",
                 "--disable-auto-launch", "--disable-all-custom-nodes", "--offline",
                 "--models-directory", str(runtime / "models"), "--input-directory", str(runtime / "input"),
                 "--output-directory", str(runtime / "output"), "--user-directory", str(runtime / "user"),
                 "--temp-directory", str(runtime / "temp")]
    settings.update(autoLaunchServer=True, launchServerInEditor=False,
                    serverUrl="http://127.0.0.1:8188", serverExecutable=str(runtime / "python/python.exe"),
                    serverWorkingDirectory=str(runtime), serverArguments=subprocess.list2cmdline(arguments),
                    comfyOutputDirectory=str(runtime / "output"), serverStartupTimeoutSeconds=300)
    data = json.dumps(settings, ensure_ascii=False, indent=2) + "\n"
    if settings_path.read_text(encoding="utf-8-sig") != data:
        temporary = settings_path.with_suffix(".json.tmp")
        temporary.write_text(data, encoding="utf-8")
        os.replace(temporary, settings_path)
    return player


def diagnose(root):
    import torch
    import PIL
    import aiohttp
    import safetensors
    from importlib.metadata import version
    available = torch.cuda.is_available()
    report = {"python": sys.version.split()[0], "python_executable": sys.executable,
              "prefix": sys.prefix, "torch": torch.__version__, "cuda_runtime": torch.version.cuda,
              "cuda_available": available, "gpu": torch.cuda.get_device_name(0) if available else None,
              "torch_arches": torch.cuda.get_arch_list() if available else [], "frontend": version("comfyui-frontend-package")}
    if available:
        left = torch.tensor([1.0, 2.0], device="cuda")
        report["cuda_calculation"] = (left * 2).cpu().tolist() == [2.0, 4.0]
    else:
        report["error"] = "Install the bundled NVIDIA driver and restart this PC."
    (root / "ComfyRuntime/logs/diagnostics.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(report, indent=2))
    return available and report.get("cuda_calculation", False)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parent.parent)
    parser.add_argument("--diagnose", action="store_true")
    args = parser.parse_args()
    if args.diagnose:
        # Diagnostics only: the Player resolves the runtime itself, so no settings are rewritten.
        (args.root.resolve() / "ComfyRuntime/logs").mkdir(exist_ok=True)
        raise SystemExit(0 if diagnose(args.root.resolve()) else 2)
    player = prepare(args.root.resolve())
    print(f"Prepared portable runtime for {player.name}")
