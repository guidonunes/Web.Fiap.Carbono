# Web.Fiap.Carbono — CI/CD Baseline

## Status

Phase 1 of the [CI/CD roadmap](ci-cd-roadmap.md) is in progress. This document records the available evidence before repository reorganization. It does not mark the full baseline or Phase 1 complete.

The user confirmed that restore and the Release build passed, and reported that the tests passed after disabling the VPN. On October 7, 2026, Codex inspected the existing TRX report to verify the test counts below. No restore, build, tests, database scripts, or deployments were executed by Codex as part of these documentation updates.

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

The recorded HEAD identifies the repository at documentation inspection; the TRX itself does not embed the source revision used to build the test assembly. Restore and Release build success are confirmed by the user's report. The local TRX is currently untracked; its link depends on retaining that artifact. Review raw reports for sensitive content before sharing them.

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

### Remaining baseline verification

- [ ] Record the SDK/Docker versions and the intended source revision used for the baseline.
- [x] Record restore and Release build success based on the user's confirmation.
- [ ] Retain restore/build logs or record warning counts and exit codes when available.
- [ ] Confirm tests work without depending on ignored developer configuration and record the test process exit code on the next required run.
- [ ] Retain the reviewed test evidence with the baseline artifacts.

## After reorganization

Not executed or verified in this record. Moving the API and migration utility under `src/`, moving the Dockerfile to the root, updating references, and repeating solution/image verification remain tasks in Phase 1. Record their actual commands and results here when performed.
