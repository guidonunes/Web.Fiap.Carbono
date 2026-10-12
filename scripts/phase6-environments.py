#!/usr/bin/env python3
"""Inspect isolated EC2 Compose environments without printing secret values."""

import argparse
from datetime import datetime, timezone
import json
import re
import subprocess
import sys


def require(condition, message):
    if not condition:
        raise ValueError(message)


def docker(*args):
    result = subprocess.run(["docker", *args], capture_output=True, text=True, timeout=30)
    require(result.returncode == 0, "Docker command failed; verify Docker access and the expected containers")
    return result.stdout


def inspect_environments(expected_image=None):
    names = [f"carbono-{environment}-{service}-1"
             for environment in ("staging", "production") for service in ("api", "mongodb")]
    containers = {item["Name"].lstrip("/"): item
                  for item in json.loads(docker("inspect", "--type", "container", *names))}
    report = []
    keys = []
    volumes = []
    images = []
    for environment, port in (("staging", "8081"), ("production", "8082")):
        api_name = f"carbono-{environment}-api-1"
        mongo_name = f"carbono-{environment}-mongodb-1"
        api, mongo = containers[api_name], containers[mongo_name]
        for item in (api, mongo):
            require(item["State"]["Running"] and item["State"]["Health"]["Status"] == "healthy", f"{item['Name']}: expected healthy container")
        image = api["Config"]["Image"]
        require(re.fullmatch(r"gui2506/web-fiap-carbono@sha256:[0-9a-f]{64}", image), "API must use a published digest")
        if expected_image:
            require(image == expected_image, f"{environment}: runtime image differs from the selected release")
        images.append(image)
        bindings = api["NetworkSettings"]["Ports"].get("8080/tcp") or []
        require(bindings and {item["HostPort"] for item in bindings} == {port}, f"{environment}: incorrect API port")
        require(not any(mongo["NetworkSettings"]["Ports"].values()), f"{environment}: MongoDB has a published host port")
        mounts = [item for item in mongo["Mounts"] if item["Destination"] == "/data/db"]
        expected_volume = f"carbono-{environment}_fiap_carbono_mongodb_data"
        require(len(mounts) == 1 and mounts[0]["Type"] == "volume" and mounts[0]["Name"] == expected_volume, f"{environment}: unexpected database volume")
        volumes.append(expected_volume)
        network = f"carbono-{environment}_default"
        for item in (api, mongo):
            require(set(item["NetworkSettings"]["Networks"]) == {network}, f"{environment}: container connected to an unexpected network")
        members = json.loads(docker("network", "inspect", network))[0]["Containers"]
        require({item["Name"] for item in members.values()} == {api_name, mongo_name}, f"{environment}: unexpected network membership")

        # Inspect environment values only in memory. Do not print Docker's raw JSON.
        settings = dict(item.split("=", 1) for item in api["Config"]["Env"] if "=" in item)
        require(settings.get("MongoDb__ConnectionString") == "mongodb://mongodb:27017", f"{environment}: unexpected MongoDB service address")
        require(settings.get("MongoDb__DatabaseName") == "fiap_carbono", f"{environment}: unexpected database name")
        key = settings.get("Jwt__SecretKey", "")
        require(len(key) >= 32, f"{environment}: missing/short runtime signing key")
        keys.append(key)
        require(settings.get("Jwt__Issuer") and settings.get("Jwt__Audience"), f"{environment}: missing JWT configuration")
        docker("exec", api_name, "jq", "-e", '.Jwt.SecretKey == ""', "/app/appsettings.json")
        report.append({"environment": environment, "api_image": image, "api_port": port,
                       "api_health": "healthy", "mongodb_health": "healthy",
                       "mongodb_container_id": mongo["Id"], "volume": expected_volume,
                       "network": network, "mongodb_host_ports": [],
                       "runtime_configuration_valid": True, "packaged_jwt_key_empty": True})
    require(keys[0] != keys[1], "Staging and production must use different signing keys")
    require(len(set(volumes)) == 2, "Database volumes must be independent")
    require(len(set(images)) == 1, "Staging and production image digests differ")
    return {"verified_at_utc": datetime.now(timezone.utc).isoformat(),
            "environments": report, "signing_keys_independent": True}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--image", help="Optional expected repository@sha256 digest from the selected workflow")
    args = parser.parse_args()
    print(json.dumps(inspect_environments(args.image), indent=2))
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except (ValueError, KeyError, TypeError, OSError, subprocess.TimeoutExpired) as error:
        print(f"Environment verification failed: {error}", file=sys.stderr)
        sys.exit(1)
