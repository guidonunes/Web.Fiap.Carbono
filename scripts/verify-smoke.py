#!/usr/bin/env python3
"""Verify smoke.sh against temporary loopback HTTP fixtures, without Docker."""

from concurrent.futures import ThreadPoolExecutor, as_completed
from contextlib import contextmanager
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
import os
import shutil
import socket
import subprocess
import sys
import tempfile
import threading
import time


SMOKE_SCRIPT = Path(__file__).resolve().with_name("smoke.sh")
BODY_MARKER = "SMOKE-FIXTURE-BODY-MUST-NOT-BE-LOGGED"
READY_BODY = ('[{"codigo":"EMP-001","fixture":"' + BODY_MARKER + '"}]').encode()
CASES = {
    "ready": (200, READY_BODY),
    "recovers_after_two_failures": (200, READY_BODY),
    "http_503": (503, READY_BODY),
    "http_201": (201, READY_BODY),
    "redirect": (302, READY_BODY),
    "html": (200, b"<html>" + BODY_MARKER.encode() + b"</html>"),
    "malformed_json": (200, READY_BODY[:-1]),
    "multiple_json_values": (200, READY_BODY + b"\n[]"),
    "object_instead_of_array": (200, READY_BODY[1:-1]),
    "empty_array": (200, b"[]"),
    "missing_company": (200, READY_BODY.replace(b"EMP-001", b"EMP-002")),
    "request_timeout": (200, READY_BODY),
    "connection_refused": (0, b""),
}


@contextmanager
def fixture(case):
    if case == "connection_refused":
        # Reserve an unused port without listening, so another process cannot take it.
        with socket.socket() as reservation:
            reservation.bind(("127.0.0.1", 0))
            port = reservation.getsockname()[1]
            yield f"http://127.0.0.1:{port}/api/empresas", None
        return

    class Handler(BaseHTTPRequestHandler):
        def log_message(self, *args):
            pass

        def do_GET(self):
            with self.server.request_lock:
                self.server.request_count += 1
                attempt = self.server.request_count

            status, body = CASES[case]
            if case == "recovers_after_two_failures" and attempt <= 2:
                status = 503
            if case == "request_timeout":
                # Exceed smoke.sh's real five-second curl timeout.
                time.sleep(6)
            if case == "redirect" and self.path == "/ready":
                status = 200

            try:
                self.send_response(status)
                if status == 302:
                    self.send_header("Location", "/ready")
                self.send_header("Content-Length", str(len(body)))
                self.end_headers()
                self.wfile.write(body)
            except OSError:
                # Timed-out curl requests close their connections before we reply.
                pass

    server = ThreadingHTTPServer(("127.0.0.1", 0), Handler)
    server.daemon_threads = True
    server.request_count = 0
    server.request_lock = threading.Lock()
    listener = threading.Thread(target=server.serve_forever, daemon=True)
    listener.start()
    try:
        yield f"http://127.0.0.1:{server.server_port}/api/empresas", server
    finally:
        server.shutdown()
        server.server_close()
        listener.join()


def verify_case(case):
    expected_exit = 0 if case in ("ready", "recovers_after_two_failures") else 1
    expected_attempts = {"ready": 1, "recovers_after_two_failures": 3}.get(case, 10)

    with fixture(case) as (url, server), tempfile.TemporaryDirectory(prefix="carbono-smoke-") as temporary:
        environment = dict(os.environ, TMPDIR=temporary)
        started = time.monotonic()
        result = subprocess.run(
            ["bash", str(SMOKE_SCRIPT), url],
            env=environment,
            capture_output=True,
            text=True,
            timeout=80,
        )
        elapsed = time.monotonic() - started
        if result.returncode != expected_exit:
            raise AssertionError(f"{case}: expected exit {expected_exit}, got {result.returncode}")
        if server is not None and server.request_count != expected_attempts:
            raise AssertionError(f"{case}: expected {expected_attempts} requests, got {server.request_count}")
        if expected_exit == 0:
            if result.stdout.strip() != "Smoke check passed: HTTP 200 and EMP-001":
                raise AssertionError(f"{case}: success message missing")
        else:
            if result.stderr.count("Attempt ") != 10:
                raise AssertionError(f"{case}: expected ten failed attempts")
            if "Smoke check failed after 10 attempts" not in result.stderr:
                raise AssertionError(f"{case}: retry exhaustion message missing")
            if "Smoke check passed" in result.stdout:
                raise AssertionError(f"{case}: failure reported as success")
        if BODY_MARKER in result.stdout + result.stderr:
            raise AssertionError(f"{case}: response body leaked to logs")
        if list(Path(temporary).iterdir()):
            raise AssertionError(f"{case}: temporary response file was not cleaned up")
        return f"PASS {case}: exit {result.returncode}, {expected_attempts} attempts, {elapsed:.1f}s"


def main():
    for tool in ("bash", "curl", "jq"):
        if shutil.which(tool) is None:
            print(f"Missing command: {tool}", file=sys.stderr)
            return 1
    if not SMOKE_SCRIPT.is_file():
        print(f"Missing smoke script: {SMOKE_SCRIPT}", file=sys.stderr)
        return 1

    failures = []
    # Separate ephemeral ports keep cases independent; real retry/timeout bounds stay intact.
    with ThreadPoolExecutor(max_workers=len(CASES)) as executor:
        pending = {executor.submit(verify_case, case): case for case in CASES}
        for completed in as_completed(pending):
            try:
                print(completed.result(), flush=True)
            except Exception as error:
                failures.append(pending[completed])
                print(f"FAIL {pending[completed]}: {error}", file=sys.stderr, flush=True)

    if failures:
        return 1
    print(f"All {len(CASES)} smoke-check scenarios passed; fixture servers cleaned up.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
