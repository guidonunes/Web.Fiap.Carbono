#!/usr/bin/env python3
"""Regression checks for Phase 6 helpers using mocked Docker/HTTP; no deployment."""

from contextlib import redirect_stdout
from copy import deepcopy
import importlib.util
import io
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch


ROOT = Path(__file__).resolve().parent.parent
sys.dont_write_bytecode = True
IMAGE = "gui2506/web-fiap-carbono@sha256:" + "a" * 64
CANARY = "PHASE6_SECRET_CANARY_DO_NOT_PRINT_"


def module(name):
    spec = importlib.util.spec_from_file_location(name, ROOT / "scripts" / f"{name}.py")
    result = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result


API = module("phase6-api")
ENVIRONMENTS = module("phase6-environments")


MOCK_DOCKER = r'''
import json, os, signal, sys
from pathlib import Path
file = Path(os.environ["PHASE6_STATE"])
state = json.loads(file.read_text())
args = sys.argv[1:]
state["calls"].append(args)
mode = os.environ["PHASE6_MODE"]
exit_code = 0
if args[0] == "inspect":
    target = args[-1]
    if target == "carbono-production-api-1":
        name = "changed" if mode == "production_changed" and state["stopped"] else "original"
        print(name + " gui2506/web-fiap-carbono@sha256:" + "a" * 64 + " 2026-10-12T00:00:00Z")
    elif target == "carbono-staging-mongodb-1":
        running = "true" if state["running"] else "false"
        if args[-2] == "{{.State.Running}}":
            print(running)
        else:
            unhealthy = mode == "recovery_unhealthy" and state["stopped"]
            print(running + (" unhealthy" if unhealthy else " healthy"))
    else:
        raise AssertionError(args)
elif args == ["stop", "--timeout", "15", "carbono-staging-mongodb-1"]:
    state["running"] = False
    state["stopped"] = True
    if mode == "stop_error":
        exit_code = 23
    file.write_text(json.dumps(state))
    if mode == "interrupted":
        os.kill(os.getppid(), signal.SIGTERM)
        sys.exit(0)
elif args == ["start", "carbono-staging-mongodb-1"]:
    if mode == "recovery_error":
        exit_code = 23
    else:
        state["running"] = True
else:
    raise AssertionError(args)
file.write_text(json.dumps(state))
sys.exit(exit_code)
'''

MOCK_CURL = r'''
import json, os, sys
from pathlib import Path
args = sys.argv[1:]
state = json.loads(Path(os.environ["PHASE6_STATE"]).read_text())
mode = os.environ["PHASE6_MODE"]
staging = args[-1] == "http://127.0.0.1:8081/api/empresas"
healthy = True
if staging:
    healthy = mode != "baseline_error" and (state["running"] or mode == "unexpected_smoke")
elif mode == "production_unavailable" and state["stopped"]:
    healthy = False
Path(args[args.index("--output") + 1]).write_text('[{"codigo":"EMP-001"}]')
print("200" if healthy else "503", end="")
'''


class FailureRecoveryTest(unittest.TestCase):
    def run_case(self, mode, expected_exit, recovered=True):
        with tempfile.TemporaryDirectory(prefix="carbono-phase6-") as temporary:
            directory = Path(temporary)
            scripts = directory / "scripts"
            scripts.mkdir()
            for name in ("phase6-staging-failure.sh", "smoke.sh"):
                shutil.copyfile(ROOT / "scripts" / name, scripts / name)
            binaries = directory / "bin"
            binaries.mkdir()
            for name, source in (("docker", MOCK_DOCKER), ("curl", MOCK_CURL)):
                command = binaries / name
                command.write_text(f"#!{sys.executable}\n" + source)
                command.chmod(0o755)
            sleeper = binaries / "sleep"
            sleeper.write_text("#!/bin/sh\nexit 0\n")
            sleeper.chmod(0o755)
            state_file = directory / "state.json"
            state_file.write_text(json.dumps({"running": True, "stopped": False, "calls": []}))
            environment = dict(os.environ, PATH=str(binaries) + os.pathsep + os.environ["PATH"],
                               PHASE6_STATE=str(state_file), PHASE6_MODE=mode, TMPDIR=str(directory))
            result = subprocess.run(["bash", str(scripts / "phase6-staging-failure.sh"), "--verify"],
                                    capture_output=True, text=True, env=environment, timeout=20)
            state = json.loads(state_file.read_text())
            self.assertEqual(expected_exit, result.returncode, result.stdout + result.stderr)
            self.assertEqual(recovered, state["running"])
            self.assertFalse(any(call[0] in ("stop", "start") and call[-1] != "carbono-staging-mongodb-1" for call in state["calls"]))
            if mode == "success":
                self.assertIn("Controlled database failure verified; staging recovered; production must be skipped", result.stdout)
                self.assertIn("Production unchanged and available", result.stdout)
            else:
                self.assertNotIn("Controlled database failure verified;", result.stdout)
            if mode == "baseline_error":
                self.assertFalse(any(call[0] == "stop" for call in state["calls"]))
            else:
                self.assertTrue(any(call[0] == "start" for call in state["calls"]))

    def test_verified_outage_recovers_and_returns_failure(self): self.run_case("success", 1)
    def test_broken_baseline_does_not_stop_database(self): self.run_case("baseline_error", 2)
    def test_stop_error_still_restores_database(self): self.run_case("stop_error", 23)
    def test_interrupt_restores_database(self): self.run_case("interrupted", 143)
    def test_unexpected_smoke_pass_restores_database(self): self.run_case("unexpected_smoke", 2)
    def test_changed_production_is_rejected(self): self.run_case("production_changed", 2)
    def test_unavailable_production_is_rejected(self): self.run_case("production_unavailable", 2)
    def test_recovery_failure_is_explicit(self): self.run_case("recovery_error", 2, recovered=False)
    def test_recovery_health_timeout_is_rejected(self): self.run_case("recovery_unhealthy", 2)


class Response:
    def __init__(self, value, status=200):
        self.status = status
        self.raw = value.encode() if isinstance(value, str) else json.dumps(value).encode()
    def read(self): return self.raw
    def __enter__(self): return self
    def __exit__(self, *args): pass


def response_for(request):
    path = request.full_url.split(":8081", 1)[1]
    token = request.headers.get("Authorization", "")
    if path == "/api/auth/login":
        email = json.loads(request.data)["email"]
        if email == "phase6-invalid@example.invalid": return Response({}, 401)
        role = "ADMIN" if email == "admin@carbono.com" else "ANALISTA_ESG"
        return Response({"role": role, "token": CANARY + role})
    if request.method == "DELETE":
        status = 403 if "ANALISTA_ESG" in token else 400 if "ADMIN" in token else 401
        return Response({}, status)
    if request.method == "POST":
        return Response({}, 403 if path == "/api/fatores-emissao" else 400)
    if path == "/api/empresas": return Response([{"id": "company", "codigo": "EMP-001"}])
    if path == "/api/produtos": return Response([{"id": "product", "codigo": "PRO-001"}])
    if path.startswith("/api/emissoes-carbono?"):
        return Response({"totalPages": 1, "totalItems": 1, "items": [{"produtoId": "product", "empresaId": "company", "fornecedorId": "supplier", "quantidadeAtividade": 3, "fatorAplicado": {"valor": 0.1}, "quantidadeEmitidaKgCO2e": 0.3}]})
    if path.startswith("/api/produtos-carbono/"):
        return Response({"totalCo2e": 0.3, "emissoesPorEtapa": [{"totalCo2e": 0.3}]})
    if path.startswith("/api/fornecedores-carbono/"):
        return Response({"items": [{"idFornecedor": "supplier", "totalCo2e": 0.3}]})
    if path.startswith("/api/dashboard-carbono/"):
        return Response({"totalCo2e": 0.3, "quantidadeEmissoes": 1, "emissoesPorMes": [{"totalCo2e": 0.3}], "emissoesPorEscopo": [{"totalCo2e": 0.3}]})
    raise AssertionError(path)


class ApiCheckTest(unittest.TestCase):
    def run_api(self, change=None):
        api = API.Api(8081)
        output = io.StringIO()
        def respond(request, timeout):
            response = response_for(request)
            return change(request, response) if change else response
        with patch.object(api.opener, "open", side_effect=respond), redirect_stdout(output):
            token = API.check_api(api, {"ADMIN": CANARY, "ANALISTA_ESG": CANARY})
        self.assertTrue(token.startswith(CANARY))
        self.assertNotIn(CANARY, output.getvalue())

    def test_decimal_and_analytics_contracts_and_secret_omission(self): self.run_api()
    def test_formula_mismatch_fails(self):
        def change(request, response):
            if "/api/emissoes-carbono?" in request.full_url:
                value = json.loads(response.raw)
                value["items"][0]["quantidadeEmitidaKgCO2e"] = 0.4
                return Response(value)
            return response
        with self.assertRaises(API.VerificationError): self.run_api(change)
    def test_authorization_mismatch_fails(self):
        with self.assertRaises(API.VerificationError):
            self.run_api(lambda request, response: Response({}, 200) if request.method == "DELETE" else response)


def environment_fixture():
    containers, networks = [], {}
    for environment, port in (("staging", "8081"), ("production", "8082")):
        network = f"carbono-{environment}_default"
        members = {}
        for service in ("api", "mongodb"):
            name = f"carbono-{environment}-{service}-1"
            members[name] = {"Name": name}
            containers.append({"Name": "/" + name, "Id": name + "-id",
                "State": {"Running": True, "Health": {"Status": "healthy"}},
                "Config": {"Image": IMAGE, "Env": ["MongoDb__ConnectionString=mongodb://mongodb:27017", "MongoDb__DatabaseName=fiap_carbono", "Jwt__SecretKey=" + CANARY + environment, "Jwt__Issuer=issuer", "Jwt__Audience=audience"]},
                "Mounts": [{"Destination": "/data/db", "Type": "volume", "Name": f"carbono-{environment}_fiap_carbono_mongodb_data"}],
                "NetworkSettings": {"Networks": {network: {}}, "Ports": {"8080/tcp": [{"HostPort": port}]} if service == "api" else {"27017/tcp": None}}})
        networks[network] = [{"Containers": members}]
    return containers, networks


class EnvironmentCheckTest(unittest.TestCase):
    def run_environment(self, mutate=None):
        containers, networks = deepcopy(environment_fixture())
        if mutate: mutate(containers, networks)
        def fake_docker(*args):
            if args[0] == "inspect": return json.dumps(containers)
            if args[:2] == ("network", "inspect"): return json.dumps(networks[args[-1]])
            if args[0] == "exec": return "true\n"
            raise AssertionError(args)
        with patch.object(ENVIRONMENTS, "docker", side_effect=fake_docker):
            report = ENVIRONMENTS.inspect_environments(IMAGE)
        self.assertNotIn(CANARY, json.dumps(report))

    def test_isolation_and_secret_omission(self): self.run_environment()
    def test_published_mongo_port_rejected(self):
        with self.assertRaises(ValueError):
            self.run_environment(lambda containers, networks: containers[1]["NetworkSettings"].update(Ports={"27017/tcp": [{"HostPort": "27017"}]}))
    def test_unexpected_network_member_rejected(self):
        with self.assertRaises(ValueError):
            self.run_environment(lambda containers, networks: networks["carbono-staging_default"][0]["Containers"].update(other={"Name": "other-container"}))
    def test_duplicate_signing_key_rejected(self):
        def mutate(containers, networks): containers[2]["Config"]["Env"] = containers[0]["Config"]["Env"][:]
        with self.assertRaises(ValueError): self.run_environment(mutate)
    def test_mismatching_digest_rejected(self):
        with self.assertRaises(ValueError):
            self.run_environment(lambda containers, networks: containers[2]["Config"].update(Image="gui2506/web-fiap-carbono@sha256:" + "b" * 64))


if __name__ == "__main__":
    unittest.main(verbosity=2)
