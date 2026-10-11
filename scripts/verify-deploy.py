#!/usr/bin/env python3
"""Check deploy.sh with mock Docker/curl/sleep and real Bash/grep/jq."""

import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile


ROOT = Path(__file__).resolve().parent.parent
DIGEST = "sha256:" + "a" * 64
IMAGE = "gui2506/web-fiap-carbono@" + DIGEST
CASES = (
    ("api_first", "staging", 0),
    ("api_last", "staging", 0),
    ("api_first", "production", 0),
    ("invalid_digest", "staging", 1),
    ("missing_environment", "staging", 1),
    ("config_failure", "staging", 23),
    ("images_failure", "staging", 1),
    ("images_failure_after_match", "staging", 1),
    ("empty_images", "staging", 1),
    ("wrong_image", "staging", 1),
    ("partial_match", "staging", 1),
    ("pull_failure", "staging", 23),
    ("wait_failure", "staging", 23),
    ("http_failure", "staging", 1),
    ("missing_company", "staging", 1),
)

MOCK_DOCKER = r'''
import json, os, signal, sys, time
from pathlib import Path

signal.signal(signal.SIGPIPE, signal.SIG_DFL)
args = sys.argv[1:]
with open(os.environ["CARBONO_CHECK_LOG"], "a") as log:
    log.write(json.dumps(["docker", *args]) + "\n")
assert args[0] == "compose", args
assert args[args.index("--project-name") + 1] == os.environ["CARBONO_CHECK_PROJECT"]
assert args[args.index("--env-file") + 1] == os.environ["CARBONO_CHECK_ENV"]
assert Path(args[args.index("--file") + 1]).is_file()
assert os.environ["APP_IMAGE"] == os.environ["CARBONO_CHECK_IMAGE"]
command = args[7:]
mode = os.environ["CARBONO_CHECK_MODE"]
if command == ["config", "--quiet"]:
    sys.exit(23 if mode == "config_failure" else 0)
elif command == ["config", "--images"]:
    if mode == "images_failure":
        sys.exit(23)
    if mode == "empty_images":
        sys.exit(0)
    image = os.environ["APP_IMAGE"]
    if mode == "wrong_image":
        image = "web-fiap-carbono:local"
    elif mode == "partial_match":
        image += "extra"
    if mode == "api_last":
        os.write(1, b"mongo:8.0.29-noble\n")
        time.sleep(0.1)
        os.write(1, (image + "\n").encode())
    else:
        os.write(1, (image + "\n").encode())
        # The old grep -q pipeline closes before this second write: SIGPIPE.
        time.sleep(0.1)
        if mode == "images_failure_after_match":
            sys.exit(23)
        os.write(1, b"mongo:8.0.29-noble\n")
elif command == ["pull", "api"]:
    sys.exit(23 if mode == "pull_failure" else 0)
elif command == ["up", "-d", "--no-build", "--pull", "missing",
                 "--wait", "--wait-timeout", "120", "api"]:
    sys.exit(23 if mode == "wait_failure" else 0)
else:
    raise AssertionError(command)
'''

MOCK_CURL = r'''
import json, os, sys
from pathlib import Path

args = sys.argv[1:]
with open(os.environ["CARBONO_CHECK_LOG"], "a") as log:
    log.write(json.dumps(["curl", *args]) + "\n")
assert args[-1] == os.environ["CARBONO_CHECK_URL"], args
mode = os.environ["CARBONO_CHECK_MODE"]
company = "EMP-002" if mode == "missing_company" else "EMP-001"
Path(args[args.index("--output") + 1]).write_text(json.dumps([{"codigo": company}]))
print("503" if mode == "http_failure" else "200", end="")
'''


def verify_case(deploy_script, case, environment, expected_exit):
    with tempfile.TemporaryDirectory(prefix="carbono-deploy-check-") as temporary:
        directory = Path(temporary)
        checkout = directory / "checkout"
        scripts = checkout / "scripts"
        scripts.mkdir(parents=True)
        shutil.copyfile(deploy_script, scripts / "deploy.sh")
        shutil.copyfile(ROOT / "scripts/smoke.sh", scripts / "smoke.sh")
        shutil.copyfile(ROOT / "docker-compose.yml", checkout / "docker-compose.yml")
        env_file = directory / (environment + ".env")
        if case != "missing_environment":
            env_file.write_text("API_PORT=" + ("8081" if environment == "staging" else "8082") + "\n")
        mock_bin = directory / "bin"
        mock_bin.mkdir()
        for name, source in (("docker", MOCK_DOCKER), ("curl", MOCK_CURL)):
            command = mock_bin / name
            command.write_text(f"#!{sys.executable}\n" + source)
            command.chmod(0o755)
        sleeper = mock_bin / "sleep"
        sleeper.write_text("#!/usr/bin/env bash\nexit 0\n")
        sleeper.chmod(0o755)
        log_file = directory / "calls.jsonl"
        project = "carbono-" + environment
        url = "http://127.0.0.1:" + ("8081" if environment == "staging" else "8082") + "/api/empresas"
        process_environment = dict(os.environ,
            PATH=str(mock_bin) + os.pathsep + os.environ["PATH"],
            CARBONO_CHECK_LOG=str(log_file), CARBONO_CHECK_PROJECT=project,
            CARBONO_CHECK_ENV=str(env_file), CARBONO_CHECK_IMAGE=IMAGE,
            CARBONO_CHECK_MODE=case, CARBONO_CHECK_URL=url, TMPDIR=str(directory))
        selected_image = "gui2506/web-fiap-carbono@invalid" if case == "invalid_digest" else IMAGE
        result = subprocess.run(
            ["bash", str(scripts / "deploy.sh"), project, str(env_file), selected_image, url],
            cwd=checkout, env=process_environment, capture_output=True, text=True, timeout=15)
        assert result.returncode == expected_exit, (
            f"expected exit {expected_exit}, got {result.returncode}; " + result.stderr.strip())
        calls = [json.loads(line) for line in log_file.read_text().splitlines()] if log_file.exists() else []
        docker_calls = [call for call in calls if call[0] == "docker"]
        curl_calls = [call for call in calls if call[0] == "curl"]
        expected_docker = {
            "invalid_digest": 0, "missing_environment": 0, "config_failure": 1,
            "images_failure": 2, "images_failure_after_match": 2,
            "empty_images": 2, "wrong_image": 2, "partial_match": 2,
            "pull_failure": 3,
        }.get(case, 4)
        assert len(docker_calls) == expected_docker, docker_calls
        expected_curl = 1 if expected_exit == 0 else 10 if case in ("http_failure", "missing_company") else 0
        assert len(curl_calls) == expected_curl, curl_calls
        if case in ("images_failure", "images_failure_after_match"):
            assert "Could not read Compose image configuration" in result.stderr
        if case in ("empty_images", "wrong_image", "partial_match"):
            assert "Compose did not select the supplied API image" in result.stderr
        if expected_exit == 0:
            assert f"Deploying {project} with {IMAGE}" in result.stdout
            assert "Smoke check passed: HTTP 200 and EMP-001" in result.stdout
            assert f"Deployment verified: {project}" in result.stdout
        else:
            assert "Deployment verified:" not in result.stdout
        print(f"PASS {environment}/{case}: exit {result.returncode}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--deploy-script", type=Path, default=ROOT / "scripts/deploy.sh")
    deploy_script = parser.parse_args().deploy_script.resolve()
    for tool in ("bash", "grep", "jq"):
        if shutil.which(tool) is None:
            print(f"Missing command: {tool}", file=sys.stderr)
            return 1
    failures = 0
    for case, environment, expected_exit in CASES:
        try:
            verify_case(deploy_script, case, environment, expected_exit)
        except (AssertionError, OSError, subprocess.TimeoutExpired) as error:
            failures += 1
            print(f"FAIL {environment}/{case}: {error}", file=sys.stderr)
    if failures:
        return 1
    print(f"All {len(CASES)} deployment scenarios passed; no real Docker/network operations")
    return 0


if __name__ == "__main__":
    sys.exit(main())
