"""Loopback-only log collector for contract/Player tests; never forwards to production."""
import argparse
import json
import threading
import time
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.parse import parse_qs, urlparse

parser = argparse.ArgumentParser()
parser.add_argument("--port", type=int, default=18196)
parser.add_argument("--output", type=Path, default=Path("Temp/ContentLog/requests.jsonl"))
args = parser.parse_args()
args.output.parent.mkdir(parents=True, exist_ok=True)
lock = threading.Lock()

class Handler(BaseHTTPRequestHandler):
    def do_GET(self):
        parsed = urlparse(self.path)
        if parsed.path == "/health":
            self.send_response(200); self.end_headers(); self.wfile.write(b"ok"); return
        message = parse_qs(parsed.query, keep_blank_values=True).get("message", [""])[0]
        with lock, args.output.open("a", encoding="utf-8") as stream:
            stream.write(json.dumps({"time": time.time(), "path": parsed.path, "message": message}, ensure_ascii=False) + "\n")
        self.send_response(503 if parsed.path == "/fail" else 200)
        self.end_headers()
        self.wfile.write(b"ok")
    def log_message(self, *_):
        pass

ThreadingHTTPServer(("127.0.0.1", args.port), Handler).serve_forever()
