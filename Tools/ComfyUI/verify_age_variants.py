"""Generate fixed-seed comparisons for all four age design conditions, after the product batch."""
import json
from pathlib import Path
import time
from benchmark import run

ROOT = Path(__file__).resolve().parents[2] / "docs/validation/comfy_shoepose"


def main():
    deadline = time.monotonic() + 600
    print("Waiting for the current product batch before age comparisons.", flush=True)
    while True:
        current = ROOT / "other_products/report.json"
        if current.exists() and json.loads(current.read_text()).get("completed"):
            break
        if time.monotonic() > deadline:
            raise TimeoutError("Product batch did not finish; no overlapping jobs were submitted.")
        time.sleep(1)
    out = ROOT / "ages"
    out.mkdir(exist_ok=True)
    report = {"seed": 1106531, "colors": ["red", "blue"], "results": [], "failed": []}
    for product in ("bags", "shoes"):
        for age in ("child", "student", "working_adult", "senior"):
            try:
                result = run("http://127.0.0.1:8188", age, product, "red", "blue", "quality", report["seed"],
                             180, out / f"{product}_{age}.png")
                report["results"].append(result)
                print("READY " + product + "/" + age, flush=True)
            except Exception as ex:
                report["failed"].append({"product": product, "age": age, "error": str(ex)})
                print("FAILED " + product + "/" + age + ": " + str(ex), flush=True)
            (out / "report.json").write_text(json.dumps(report, indent=2))
    report["completed"] = True
    (out / "report.json").write_text(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
