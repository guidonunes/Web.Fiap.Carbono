# Web.Fiap.Carbono — CI/CD Baseline

## Status

Phase 1 of the [CI/CD roadmap](ci-cd-roadmap.md) is complete as verified on October 7, 2026. The pre-move baseline passed, the reorganized Release solution builds, all 107 tests pass from a clean source snapshot without developer settings, and the root Dockerfile builds successfully. Phase 2 has not started.

The user confirmed the pre-move restore and Release build and reported that tests passed after disabling the VPN. Codex initially inspected the existing TRX without rerunning commands. The later authorized Phase 1 implementation executed the post-move verification recorded separately below.

## Before reorganization — October 7, 2026

### Context and evidence

| Item | Recorded observation |
| --- | --- |
| Local test report | [artifacts/test-results/before/baseline.trx](../artifacts/test-results/before/baseline.trx) |
| Test assembly | `Web.Fiap.Carbono.Tests/bin/Release/net8.0/Web.Fiap.Carbono.Tests.dll`, as recorded in the TRX |
| Test start | `2026-10-07T19:26:11.4077201-03:00` |
| Test finish | `2026-10-07T19:26:51.6208844-03:00` |
| Elapsed time | Approximately 40.21 seconds between the TRX start and finish timestamps |
| Git HEAD at documentation inspection | `03a2c8a157833d56348276a00168ad51f42b6a45` |
| Working tree before documentation edits | No tracked changes; existing untracked `artifacts/` directory |
| SDK and Docker versions at test execution | Not recorded in the available evidence |
| Network condition | User reports that the successful rerun followed disabling the VPN |

The recorded HEAD identifies the repository at initial documentation inspection; the TRX itself does not embed the source revision used to build the test assembly. Restore and Release build success are confirmed by the user's report. The pre-move TRX was subsequently tracked and is preserved unchanged by the reorganization.

### Restore and build results

| Command | Result | Evidence |
| --- | --- | --- |
| `dotnet restore Web.Fiap.Carbono.sln` | Passed | Confirmed by the user on October 7, 2026 |
| `dotnet build Web.Fiap.Carbono.sln --configuration Release --no-restore` | Passed | Confirmed by the user on October 7, 2026 |

Console logs, warning counts, durations, and numeric exit codes were not supplied. Do not interpret these reported successes as a claim of zero warnings or independently captured build output.

### Test command supplied by the user

```bash
dotnet test Web.Fiap.Carbono.sln \
  --configuration Release \
  --no-build \
  --logger "trx;LogFileName=baseline.trx" \
  --results-directory ./artifacts/test-results/before
```

This is the command supplied during troubleshooting. The TRX confirms a Release test assembly and the results, but does not preserve the complete shell invocation or its process exit code.

`--no-build` intentionally skips compilation and tests existing Release binaries. Restore and Release build success are recorded separately above rather than inferred from the test results.

### Verified test results

| TRX counter | Value |
| --- | ---: |
| Total | 106 |
| Executed | 106 |
| Passed | 106 |
| Failed | 0 |
| Errors | 0 |
| Timeouts | 0 |
| Aborted | 0 |
| Not executed | 0 |

The report's overall outcome is `Completed`; all 106 individual test results have outcome `Passed`. The shell exit code and restore/build output were not captured in the available evidence and are not inferred from these counters.

### Timeout troubleshooting

The initial test attempt appeared to time out. The user reported that the tests passed after turning the VPN off. VPN interference with Docker/test-container connectivity is a possible explanation, but the exact affected connection and root cause were not established. The available successful TRX contains no timeouts and does not document the earlier attempt's error.

If this recurs, capture the exact error and console output, inspect Docker/container readiness, and compare connectivity with and without the VPN where permitted. Avoid overlapping test runs: the API fixture uses host networking and fixed port `27019`. Add `--logger "console;verbosity=detailed"` to the test command when more visible progress is needed. Disabling the VPN is a reported local workaround, not a general application or CI requirement.

### Baseline evidence status

- [x] Record the SDK/Docker versions and starting revision for post-move verification below; versions from the earlier user run remain uncaptured.
- [x] Record restore and Release build success based on the user's confirmation.
- [x] Record post-move restore/build results, warning counts, and exit codes; do not invent missing pre-move output.
- [x] Confirm tests work without ignored developer configuration and record their exit code.
- [x] Preserve the pre-move TRX and retain the reviewed post-move TRX in the baseline artifacts.

## After reorganization — October 7, 2026

### Implemented changes

- API and migration utility moved to `src/`; test project, solution, database scripts, and Oracle archive remain at the root.
- Dockerfile moved to the root with repository-root context and updated project paths. The API image still builds only the API, not the migration utility.
- Solution/project references and linked `.dockerignore`/Oracle context paths repaired. Migration-to-API sibling reference retained.
- Current README and migration-tool commands/links updated; `roadmap.md` and historical execution records preserved. The migration report received only a link correction for the relocated tool.
- Test factory supplies synthetic JWT settings before the application entry point executes. Configuration-failure tests reuse that factory. One new regression test checks that signing and validation use the same synthetic key, issuer, and audience.
- Docker context excludes developer settings, environment files, test artifacts, and ZIPs. Local ignored settings moved with their project and were not copied into the clean verification snapshot.

### Environment and source

| Item | Observed value |
| --- | --- |
| Starting Git revision | `6edb163034a2064972cf1ef2f18258e1787696ba` plus the uncommitted Phase 1 changes |
| Initial working tree | Clean |
| OS | Ubuntu 22.04, Linux x64 |
| .NET SDK / MSBuild | `8.0.422` / `17.11.48` |
| Test runtime / VSTest | .NET `8.0.28` / `17.11.1` |
| Docker Engine client/server | `29.8.1` |
| Docker Compose | `v5.1.3` |
| Test MongoDB image | `mongo:8.0.29-noble`, disposable fixture containers |
| Clean verification directory | `/tmp/fiap-carbono-phase1-eypnl1sj` |

The clean snapshot copied tracked source with the current path/code changes, including `archive/oracle/` and `database/mongodb/`. It excluded developer appsettings, `.env`, previous binaries/intermediates, test artifacts, and ZIPs. It is a verification copy, not a committed revision or deployment. SDK and package caches remain host prerequisites.

### Executed verification

| Check | Result | Exit code |
| --- | --- | ---: |
| Solution restore in the working repository | Passed; all three projects restored | 0 |
| Release build in the working repository | Passed; 0 warnings, 0 errors | 0 |
| Focused `MongoDbConfigurationTest` run after the factory correction | 7 passed, 0 failed, 0 skipped | 0 |
| Restore in the clean snapshot | Passed; all three projects restored | 0 |
| Release build in the clean snapshot | Passed; 0 warnings, 0 errors | 0 |
| Full solution tests in the clean snapshot | 107 passed, 0 failed, 0 skipped | 0 |
| Root Dockerfile image build | Passed; API build reported 0 warnings, 0 errors | 0 |
| Solution listing, linked-path checks, documentation links, and diff whitespace | Passed | 0 |

Repository commands:

```bash
dotnet --info
docker version
docker compose version
dotnet restore Web.Fiap.Carbono.sln
dotnet build Web.Fiap.Carbono.sln --configuration Release --no-restore
dotnet test Web.Fiap.Carbono.sln --configuration Release --no-build \
  --filter FullyQualifiedName~MongoDbConfigurationTest
dotnet sln Web.Fiap.Carbono.sln list
docker build --file Dockerfile --tag web-fiap-carbono:baseline .
git diff --check
```

The clean snapshot repeated restore and Release build, then ran:

```bash
dotnet test Web.Fiap.Carbono.sln --configuration Release --no-build \
  --logger "trx;LogFileName=baseline.trx" \
  --results-directory /home/guilherme/Documents/code/Web.Fiap.Carbono/artifacts/test-results/after
```

The absolute results directory retained evidence in the working repository rather than the temporary snapshot. For a normal repository run, use `./artifacts/test-results/after` as shown in the README.

### Post-move test and image evidence

The [post-move TRX](../artifacts/test-results/after/baseline.trx) records 107 total/executed/passed tests, zero failures, errors, timeouts, aborted tests, or unexecuted tests. The added JWT regression test accounts for the increase from 106. Test start was `2026-10-07T20:05:18.3450866-03:00`, finish was `2026-10-07T20:05:49.1751686-03:00`, approximately 30.83 seconds elapsed.

The full run exercises API content-root discovery under `src/`, migration fixtures locating the root database scripts, CRUD, decimal calculation, snapshots, analytics, and authorization against disposable MongoDB. No application or shared database was used for this verification. Report inspection found no JWT-shaped tokens or credential-bearing MongoDB URIs.

Local image `web-fiap-carbono:baseline` was built successfully with image ID `sha256:81d11c95014fef1e51465e70a7a3bc6ed2fac719cf57b3216e64dc942d948296`. This is a local image ID, not a published registry digest. No image was pushed and no deployment occurred.

### Issues resolved during verification

- Sandboxed MSBuild restore failed without a project diagnostic, and Docker socket access was denied. Approved execution outside the sandbox succeeded; these initial attempts are not counted as passing checks.
- The first JWT regression run failed because `ConfigureAppConfiguration` ran after `Program` had already read token-validation settings. Supplying synthetic values through `ConfigureHostConfiguration` in the factory's `CreateHost` resolved this; the focused and complete suites then passed.
- The earlier VPN workaround remains a user-reported pre-move observation. No VPN change was needed or performed during this successful post-move run, and the original timeout's exact cause remains unconfirmed.

### Exit gate

- [x] Pre-move restore/build/tests are recorded with their actual evidence sources.
- [x] Reorganized Release build and full tests pass without ignored developer configuration.
- [x] Root Dockerfile builds, references resolve, and current instructions use the new paths.
- [x] Application business code, five-collection design, .NET 8, migration artifacts, and historical records are preserved.

Next task: Phase 2 Compose implementation. Compose configuration and CI/CD workflows were not changed in Phase 1.
