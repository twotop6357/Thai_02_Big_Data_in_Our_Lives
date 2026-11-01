"""Local-only delayed queue fixture for cancellation/timeout verification. Never submits GPU jobs."""
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
import time


class Handler(BaseHTTPRequestHandler):
    def do_GET(self):
        time.sleep(3)
        data = b'{"queue_running":[],"queue_pending":[]}'
        try:
            self.send_response(200)
            self.send_header("Content-Type", "application/json")
            self.send_header("Content-Length", str(len(data)))
            self.end_headers()
            self.wfile.write(data)
        except (BrokenPipeError, ConnectionResetError, ConnectionAbortedError):
            pass

    def log_message(self, *_):
        pass


if __name__ == "__main__":
    ThreadingHTTPServer(("127.0.0.1", 18189), Handler).serve_forever()
