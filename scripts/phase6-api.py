#!/usr/bin/env python3
"""Check deployed API contracts without changing MongoDB records or printing tokens."""

import argparse
from datetime import datetime, timezone
from decimal import Decimal
import getpass
import json
import os
import sys
import urllib.error
import urllib.request


class VerificationError(Exception):
    pass


class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, request, fp, code, message, headers, new_url):
        return None


def require(condition, message):
    if not condition:
        raise VerificationError(message)


class Api:
    def __init__(self, port):
        self.base = f"http://127.0.0.1:{port}"
        self.opener = urllib.request.build_opener(urllib.request.ProxyHandler({}), NoRedirect())

    def request(self, method, path, expected=200, data=None, token=None):
        headers = {"Content-Type": "application/json"}
        if token:
            headers["Authorization"] = "Bearer " + token
        request = urllib.request.Request(
            self.base + path,
            data=json.dumps(data).encode() if data is not None else None,
            headers=headers, method=method)
        try:
            response = self.opener.open(request, timeout=10)
        except urllib.error.HTTPError as error:
            response = error
        except (urllib.error.URLError, TimeoutError, OSError):
            raise VerificationError(f"{method} {path}: request failed") from None
        with response:
            status = response.status
            raw = response.read()
        require(status == expected, f"{method} {path}: expected HTTP {expected}, got {status}")
        print(f"PASS {self.base} {method} {path}: HTTP {status}")
        if expected != 200:
            return None
        try:
            return json.loads(raw, parse_float=Decimal)
        except (ValueError, UnicodeError):
            raise VerificationError(f"{method} {path}: invalid JSON") from None


def check_api(api, passwords):
    companies = api.request("GET", "/api/empresas")
    products = api.request("GET", "/api/produtos")
    company = next((item for item in companies if item["codigo"] == "EMP-001"), None)
    product = next((item for item in products if item["codigo"] == "PRO-001"), None)
    require(company and product, "Required EMP-001/PRO-001 seed records missing")

    tokens = {}
    for role, email in (("ADMIN", "admin@carbono.com"), ("ANALISTA_ESG", "analista@carbono.com")):
        login = api.request("POST", "/api/auth/login", data={"email": email, "senha": passwords[role]})
        require(login.get("role") == role and login.get("token"), f"Login did not return the expected {role} identity")
        tokens[role] = login["token"]
        print(f"PASS authenticated role: {role} (token omitted)")

    api.request("POST", "/api/auth/login", expected=401,
                data={"email": "phase6-invalid@example.invalid", "senha": "invalid"})
    # These requests cannot delete or create a domain record: authentication,
    # role checks or malformed IDs/payloads reject them before persistence.
    path = "/api/emissoes-carbono/mongodb/not-an-object-id"
    api.request("DELETE", path, expected=401)
    api.request("DELETE", path, expected=401, token="not-a-valid-token")
    api.request("DELETE", path, expected=403, token=tokens["ANALISTA_ESG"])
    api.request("DELETE", path, expected=400, token=tokens["ADMIN"])
    api.request("POST", "/api/fatores-emissao", expected=403, data={}, token=tokens["ANALISTA_ESG"])
    for token in tokens.values():
        api.request("POST", "/api/emissoes-carbono/calcular", expected=400, data={}, token=token)

    emissions = []
    page = api.request("GET", "/api/emissoes-carbono?pageNumber=1&pageSize=50")
    total_pages = page["totalPages"]
    require(1 <= total_pages <= 20, "Expected a nonempty academic dataset of at most 1000 emissions")
    emissions.extend(page["items"])
    for number in range(2, total_pages + 1):
        emissions.extend(api.request("GET", f"/api/emissoes-carbono?pageNumber={number}&pageSize=50")["items"])
    require(len(emissions) == page["totalItems"], "Emission pagination count mismatch")
    for emission in emissions:
        expected = Decimal(str(emission["quantidadeAtividade"])) * Decimal(str(emission["fatorAplicado"]["valor"]))
        require(Decimal(str(emission["quantidadeEmitidaKgCO2e"])) == expected, "Persisted decimal emission formula mismatch")
    print(f"PASS decimal calculation and applied-factor snapshots: {len(emissions)} persisted emissions")

    footprint = api.request("GET", f'/api/produtos-carbono/{product["id"]}/pegada')
    product_total = sum((item["quantidadeEmitidaKgCO2e"] for item in emissions if item["produtoId"] == product["id"]), Decimal(0))
    require(footprint["totalCo2e"] == product_total, "Product-footprint total mismatch")
    require(sum((item["totalCo2e"] for item in footprint["emissoesPorEtapa"]), Decimal(0)) == product_total, "Product category totals mismatch")

    ranking = api.request("GET", "/api/fornecedores-carbono/ranking?pageNumber=1&pageSize=50")
    totals = [item["totalCo2e"] for item in ranking["items"]]
    require(totals == sorted(totals, reverse=True), "Supplier ranking is not descending")
    for supplier in ranking["items"]:
        expected = sum((item["quantidadeEmitidaKgCO2e"] for item in emissions if item["fornecedorId"] == supplier["idFornecedor"]), Decimal(0))
        require(supplier["totalCo2e"] == expected, "Supplier ranking total mismatch")

    dashboard = api.request("GET", f'/api/dashboard-carbono/empresas/{company["id"]}/resumo')
    company_emissions = [item for item in emissions if item["empresaId"] == company["id"]]
    company_total = sum((item["quantidadeEmitidaKgCO2e"] for item in company_emissions), Decimal(0))
    require(dashboard["totalCo2e"] == company_total, "Company dashboard total mismatch")
    require(dashboard["quantidadeEmissoes"] == len(company_emissions), "Company dashboard count mismatch")
    for field in ("emissoesPorMes", "emissoesPorEscopo"):
        require(sum((item["totalCo2e"] for item in dashboard[field]), Decimal(0)) == company_total, f"Company dashboard {field} total mismatch")
    print("PASS product footprint, supplier ranking, and company dashboard totals")
    return tokens["ADMIN"]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--environment", choices=("staging", "production", "both"), default="both")
    args = parser.parse_args()
    passwords = {}
    for role, variable in (("ADMIN", "CARBONO_ADMIN_PASSWORD"), ("ANALISTA_ESG", "CARBONO_ANALYST_PASSWORD")):
        password = os.environ.get(variable)
        require(password or sys.stdin.isatty(), f"Set {variable} securely or use an interactive terminal")
        passwords[role] = password or getpass.getpass(f"Existing {role} account password (hidden): ")
        require(passwords[role], f"Missing {role} password")
    print("Verification UTC:", datetime.now(timezone.utc).isoformat())
    tokens = {}
    for environment, port in (("staging", 8081), ("production", 8082)):
        if args.environment in ("both", environment):
            tokens[environment] = check_api(Api(port), passwords)
    if args.environment == "both":
        Api(8082).request("POST", "/api/emissoes-carbono/calcular", expected=401,
                          data={}, token=tokens["staging"])
        print("PASS production rejects staging's signing key")
    print("API verification passed; passwords/tokens omitted; no MongoDB records changed")
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except VerificationError as error:
        print(f"API verification failed: {error}", file=sys.stderr)
        sys.exit(1)
    except (KeyError, TypeError, ValueError):
        print("API verification failed; check the last PASS line and the documented response contract", file=sys.stderr)
        sys.exit(1)
