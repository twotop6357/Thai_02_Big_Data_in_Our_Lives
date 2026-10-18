"""Download the pinned checkpoint into an explicitly selected ComfyUI model folder."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import urllib.request


def sha256(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--models-dir", type=Path, required=True, help="ComfyUI checkpoints directory")
    args = parser.parse_args()
    model = json.loads(Path(__file__).with_name("model.json").read_text(encoding="utf-8"))
    folder = args.models_dir.resolve()
    folder.mkdir(parents=True, exist_ok=True)
    target = folder / model["filename"]
    if target.exists():
        if sha256(target) != model["sha256"]:
            raise RuntimeError("Existing checkpoint has an unexpected hash; refusing to overwrite it.")
        print("Checkpoint already exists and SHA-256 matches.")
        return
    partial = target.with_suffix(target.suffix + ".partial")
    print(f"Downloading {model['filename']} ({model['bytes'] / 1e9:.2f} GB).", flush=True)
    total = 0
    digest = hashlib.sha256()
    # Exclusive creation preserves any pre-existing partial download for inspection.
    with partial.open("xb") as output:
        with urllib.request.urlopen(model["url"], timeout=60) as response:
            while block := response.read(4 * 1024 * 1024):
                output.write(block)
                digest.update(block)
                previous = total
                total += len(block)
                if total // (256 * 1024 * 1024) != previous // (256 * 1024 * 1024):
                    print(f"Downloaded {total / 1e9:.2f} GB.", flush=True)
        output.flush()
        os.fsync(output.fileno())
    if total != model["bytes"] or digest.hexdigest() != model["sha256"]:
        raise RuntimeError("Checkpoint size/hash verification failed. Partial download was preserved.")
    # Recheck before rename; never replace an existing checkpoint.
    if target.exists():
        raise RuntimeError("Checkpoint appeared during download; partial download was preserved.")
    partial.rename(target)
    print(f"Checkpoint installed and SHA-256 verified: {target}", flush=True)


if __name__ == "__main__":
    main()
