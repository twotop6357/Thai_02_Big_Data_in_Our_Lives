"""Adopt a ComfyRuntime moved from an older build, then point this build's Comfy settings at it.

Deployed as Launcher/prepare_player.py next to NSM_DNA_02.exe. Standard library only.
"""
import json
import os
from pathlib import Path
import sys
import traceback

PLAYER = "NSM_DNA_02.exe"


def main():
    root = Path(sys.argv[1]).resolve()
    runtime = root / "ComfyRuntime"
    manifest_path = runtime / "package_manifest.json"
    # utf-8-sig also reads a manifest that an editor saved with a BOM; it is always rewritten without one.
    manifest = json.loads(manifest_path.read_text(encoding="utf-8-sig"))
    if manifest.get("player") != PLAYER or manifest_path.read_bytes().startswith(b"\xef\xbb\xbf"):
        manifest["player"] = PLAYER
        temporary = manifest_path.with_suffix(".json.tmp")
        temporary.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
        os.replace(temporary, manifest_path)
    sys.path.insert(0, str(runtime))
    import portable_boot
    player = portable_boot.prepare(root)
    print("Prepared " + player.name)


if __name__ == "__main__":
    log = Path(__file__).resolve().parent / "startup_error.txt"
    try:
        main()
        if log.exists():
            log.unlink()
    except Exception:
        log.write_text(traceback.format_exc(), encoding="utf-8")
        traceback.print_exc()
        sys.exit(1)
