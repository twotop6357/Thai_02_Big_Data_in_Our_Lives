"""Local deterministic Comfy responses for spinner lifecycle checks; never submits a GPU job."""
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
import json, time, urllib.parse, uuid

ROOT = Path(__file__).resolve().parents[2]
state = {"mode": "delay", "jobs": {}}
reference = next((ROOT / "Assets/StreamingAssets/BlockGeneration/StyleReferences").glob("style_user_*.png")).read_bytes()
product = (ROOT / "docs/validation/comfy_style3/runtime_0_shoes.png").read_bytes()

class Handler(BaseHTTPRequestHandler):
    def send(self, body, status=200, kind="application/json"):
        if not isinstance(body, bytes): body = json.dumps(body).encode()
        try:
            self.send_response(status); self.send_header("Content-Type", kind); self.send_header("Content-Length", str(len(body)))
            self.end_headers(); self.wfile.write(body)
        except (BrokenPipeError, ConnectionResetError, ConnectionAbortedError): pass
    def do_GET(self):
        route = urllib.parse.urlparse(self.path); query = urllib.parse.parse_qs(route.query)
        if route.path == "/queue": return self.send({"queue_running": [], "queue_pending": []})
        if route.path.startswith("/history/"):
            pid = route.path.rsplit("/", 1)[-1]; job = state["jobs"].get(pid)
            if not job or job["mode"] == "delay" or time.monotonic() - job["start"] < 1: return self.send({})
            return self.send({pid: {"status": {"status_str": "success", "completed": True}, "outputs": {"7": {"images": [{"filename": "fixture.png", "subfolder": "fixture", "type": "output"}]}}}})
        if route.path == "/view": return self.send(reference if query.get("type") == ["input"] else product, kind="image/png")
        return self.send({}, 404)
    def do_POST(self):
        body = json.loads(self.rfile.read(int(self.headers.get("Content-Length", 0))) or "{}")
        if self.path == "/mode": state["mode"] = body["mode"]; return self.send({"mode": state["mode"]})
        if self.path == "/queue": return self.send(b"")
        if self.path == "/prompt":
            if state["mode"] == "error": return self.send({"error": "Intentional spinner verification error"}, 400)
            pid = uuid.uuid4().hex; state["jobs"][pid] = {"mode": state["mode"], "start": time.monotonic()}; return self.send({"prompt_id": pid})
        return self.send({}, 404)
    def log_message(self, *_): pass

if __name__ == "__main__": ThreadingHTTPServer(("127.0.0.1", 18191), Handler).serve_forever()
