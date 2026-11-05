"""Run the bundled ComfyUI with local cache paths and persistent startup diagnostics."""
import os
from pathlib import Path
import runpy
import sys
import traceback

runtime = Path(__file__).resolve().parent
os.environ["HF_HOME"] = str(runtime / "cache/huggingface")
os.environ["HF_HUB_OFFLINE"] = "1"
os.environ["TRANSFORMERS_OFFLINE"] = "1"
os.environ["TORCH_HOME"] = str(runtime / "cache/torch")
os.environ["PYTHONPYCACHEPREFIX"] = str(runtime / "cache/pycache")
(runtime / "logs").mkdir(exist_ok=True)
log_path = runtime / "logs/comfy_server.log"
if log_path.exists() and log_path.stat().st_size > 8 * 1024 * 1024:
    os.replace(log_path, log_path.with_suffix(".previous.log"))
original_stdout, original_stderr = sys.stdout, sys.stderr
with log_path.open("a", encoding="utf-8", buffering=1) as log:
    sys.stdout = log
    sys.stderr = log
    main = runtime / "ComfyUI/main.py"
    sys.argv[0] = str(main)
    sys.path.insert(0, str(main.parent))
    try:
        runpy.run_path(str(main), run_name="__main__")
    except BaseException:
        traceback.print_exc()
        raise
    finally:
        sys.stdout, sys.stderr = original_stdout, original_stderr
