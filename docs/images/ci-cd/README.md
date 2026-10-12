# CI/CD evidence index

Only real captures belong here. No screenshot files have been supplied for Phase 6 yet. The [verification guide](../../ci-cd-verification.md) provides commands and the [roadmap](../../ci-cd-roadmap.md) defines acceptance. Creating this index does not complete the evidence task.

| Capture to collect | Actual evidence required | Status |
| --- | --- | --- |
| `01-pr-ci.png` | Successful PR/run identity and restore/build/tests; delivery jobs skipped | Pending |
| `01b-pr-ci-failure.png` | Controlled PR test failure, retained summary/TRX, delivery jobs skipped | Pending |
| `02-master-pipeline.png` | Successful run, ordered CI/publication/staging/production | Pending; execution records exist |
| `03-docker-hub-image.png` | `gui2506/web-fiap-carbono`, full commit-SHA tag, matching registry digest | Pending; earlier registry evidence exists |
| `04-staging.png` | Healthy staging containers, port `8081`, actual database-backed HTTP `200`/`EMP-001` | Pending |
| `05-production.png` | Healthy production containers, port `8082`, same promoted digest and actual smoke result | Pending |
| `06-failure-blocking.png` | Real staging database outage, recovery, skipped production, unchanged production metadata | Pending; real outage still to execute |
| `07-persistence-isolation.png` | Already-recorded persistence comparison/volume, production marker absence, distinct networks/configuration | Pending; prior persistence result passed |
| `08-deployment-concurrency.png` | Actual A/B run links and UTC timing proving non-overlap | Pending; timing independently inspected |
| `09-api-auth-analytics.png` | Sanitized API checker results for login/roles, persisted calculation, and analytics | Pending; EC2 check still to execute |

Use additional images where required for legibility; do not cram different failed/recovery runs into an ambiguous image. Capture application/terminal/GitHub results, not source-code screenshots standing in for execution.

For each file actually added, append its caption using these fields:

```text
Filename:
Captured at (date/time/timezone):
Environment and command or GitHub job:
PR/run link and full commit SHA:
Published/runtime digest, where applicable:
Observed result and what it proves:
Evidence source (user capture / independently inspected run):
Limitations or associated recovery run:
```

Attach the real image below its completed caption. Retain full run URLs and digests rather than mixing identifiers from different releases. For old persistence evidence, say it is the October 12 observation and use the existing saved comparison only if it is still available; never reconstruct it as though it were a historical capture. After cleanup the temporary record should remain absent.

Redact credentials, JWTs, signing keys, passwords, private keys, credential-bearing connection strings, and personal data. Keep environment/run/time identity visible. Review the final captions and redactions before marking a capture collected. The README and presentation/report must later include captions and interpretations as required by Phase 7; no final delivery is claimed here.
