# CI/CD evidence index

Only real captures belong here. Six user-supplied PNGs are now present and were visually reviewed by Codex on October 12, 2026, including the subsequently supplied Docker Hub capture. Their original filenames are preserved below. The [verification guide](../../ci-cd-verification.md) provides commands and the [roadmap](../../ci-cd-roadmap.md) defines acceptance. The remaining captures and Phase 6 exit gates are still pending. The images are currently untracked in Git; their presence in the workspace does not establish that they have been committed or published.

| Capture to collect | Actual evidence required | Status |
| --- | --- | --- |
| `01-pr-ci.png` | Successful PR/run identity and restore/build/tests; delivery jobs skipped | Overview collected as [11-build-before-merge.png](11-build-before-merge.png); report contents require their own retained record |
| `01b-pr-ci-failure.png` | Controlled PR test failure, retained summary/TRX, delivery jobs skipped | Overview collected as [10-failure-evidence.png](10-failure-evidence.png); screenshot shows one artifact, not its contents |
| `02-master-pipeline.png` | Successful run, ordered CI/publication/staging/production | Collected as [12-successful-workflow.png](12-successful-workflow.png), the original successful master overview |
| `03-docker-hub-image.png` | `gui2506/web-fiap-carbono`, full commit-SHA tag, matching registry digest | Collected as [docker-hub-image.png](docker-hub-image.png); full SHA/digest agree with the user's registry command |
| `04-staging.png` | Healthy staging containers, port `8081`, actual database-backed HTTP `200`/`EMP-001` | Capture pending; user-supplied environment and standalone smoke checks passed |
| `05-production.png` | Healthy production containers, port `8082`, same promoted digest and actual smoke result | Capture pending; environment/smoke checks passed, current publication-run association still pending |
| `06-failure-blocking.png` | Real staging database outage, recovery, skipped production, unchanged production metadata | Overview collected as [13-failed-staging-test.png](13-failed-staging-test.png); recovery marker and production comparison still need retained log evidence |
| `07-persistence-isolation.png` | Already-recorded persistence comparison/volume, production marker absence, distinct networks/configuration | Capture pending; persistence and October 12 environment/isolation results passed |
| `08-deployment-concurrency.png` | Actual A/B run links and UTC timing proving non-overlap | Pending; timing independently inspected |
| `09-api-auth-analytics.png` | Sanitized API checker results for login/roles, persisted calculation, and analytics | Collected and reviewed: [09-api-auth-analytics.png](09-api-auth-analytics.png) |

## Reviewed captures

The capture times of the PNG files were not supplied. Dates/times below identify the actual execution shown or the associated independently inspected GitHub jobs; file timestamps are not treated as capture timestamps. All six images are readable and contain no visible passwords, signing keys, private keys, or tokens. This is a visual review of these captures, not an exhaustive secret audit. No images were generated, edited, renamed, or staged by Codex.

### Docker Hub SHA tag and registry digest

![Docker Hub full-SHA tag and manifest digest](docker-hub-image.png)

The user's Docker Hub tag-details capture shows `gui2506/web-fiap-carbono:b1f2bdb4783e9cdc97f1b9c34c6279a772c03b22`, manifest digest `sha256:12e855e8d4552b394e41507cb300fbd33ff99170a016306e72745dadd47ef619`, and `linux/amd64`. It was reviewed on October 12, 2026; exact capture time was not supplied. The user's `docker buildx imagetools inspect` output for the same full-SHA tag names that same digest and media type `application/vnd.docker.distribution.manifest.v2+json`. This satisfies the registry capture requirement and links the observed tag to its current registry content. It does not establish the deployed container digest or which publication attempt produced it; the post-recovery runtime comparison is still pending. Keep it separate from the earlier pre-outage production digest `97355d...` and original-success capture.

### API authentication, calculation, and analytics

![EC2 API verification in staging and production](09-api-auth-analytics.png)

The user's EC2 command `python3 scripts/phase6-api.py --environment both` displays execution timestamp `2026-10-12T19:29:29.533439+00:00`, both ports, expected HTTP statuses and roles, fifteen persisted emission calculations per environment, analytics totals, production rejection of staging's token, the final success message, and exit `0`. Password prompts are hidden and tokens omitted. This is actual user-supplied execution and satisfies the API screenshot evidence. No workflow SHA or image digest is visible, and the checker did not create a new calculated emission.

### Controlled PR failure

![Failed PR CI and skipped delivery jobs](10-failure-evidence.png)

[PR #1](https://github.com/guidonunes/Web.Fiap.Carbono/pull/1), [run 38302777058, attempt 1](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38302777058/attempts/1), commit `70743919e4a209ddf5e46c12d3d8fcf53e5cdcdb`, October 12, 2026. The image shows the pull-request trigger, failed CI, one artifact, and skipped publication/staging/production. Read-only job metadata separately confirms the controlled-test preparation succeeded, restore/build succeeded, tests failed, and diagnostic summary/artifact upload succeeded. CI ran `20:13:18`–`20:14:17 UTC`. The screenshot does not display the failing test name, test counters, or downloaded report contents; do not infer those from the overview.

### Successful PR recovery, delivery still skipped

![Successful PR recovery and explicit skipped image job](11-build-before-merge.png)

[Run 38302777058, attempt 2](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38302777058/attempts/2), the same PR/commit as above, October 12, 2026. The image shows green CI, `Latest #2`, and the explicit skipped image job; both deployment jobs also show skipped status. Read-only metadata confirms the optional failure step was skipped and restore/build/tests/summary/upload succeeded. CI ran `20:27:25`–`20:28:32 UTC`. This is a successful PR check before merging, with delivery excluded; it does not demonstrate image publication or deployment.

### Original successful master pipeline

![Successful master checks, publication, staging, and production](12-successful-workflow.png)

[Run 38304927773, original attempt](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38304927773/attempts/1), commit `b1f2bdb4783e9cdc97f1b9c34c6279a772c03b22`, October 12, 2026. The screenshot shows the `push` to `master`, short SHA `b1f2bdb`, green CI/publication/staging/production in order, and the `ci-results` artifact. It does not show a registry image digest or host container metadata. Keep this original-success capture separate from the later controlled failure (attempt 2) and recovery (attempt 3); it is not a screenshot of the post-outage recovery.

### Staging failure blocks production

![Successful CI and publication, failed staging, skipped production](13-failed-staging-test.png)

[Run 38304927773, attempt 2](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38304927773/attempts/2), the same master commit as above, October 12, 2026. The screenshot shows `Latest #2`, successful CI/publication, failed staging with exit `1`, two artifacts, and skipped production. Read-only metadata confirms normal staging deployment succeeded before the controlled database-failure step failed, and its evidence artifact uploaded. Staging ran `20:45:43`–`20:47:15 UTC`. The image does not display the database stop/recovery messages or unchanged production ID/image/start time; retain those from the `phase6-staging-failure` artifact or an additional actual log capture before closing the outage acceptance.

The separately inspected [attempt 3](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38304927773/attempts/3) has successful staging (`20:50:06`–`20:50:12 UTC`) and production (`20:50:14`–`20:50:26 UTC`) with the controlled failure step skipped. Its successful CI/publication entries retain attempt 2's earlier timestamps; do not describe this recovery as a freshly rebuilt or republished image. No capture of attempt 3 or current post-recovery environment report has been supplied yet.

## Remaining capture requirements

The pending table rows still require host health/current runtime image and smoke results, persistence/isolation resources, and A/B concurrency evidence, plus detailed outage restoration/production-comparison proof. The registry capture is now reviewed. The API capture above already covers database-backed responses in both environments; supplement it with the host resource report rather than recreating that API exercise. Reuse the already-recorded persistence and concurrency results without repeating destructive or deployment exercises merely to obtain pictures.

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

Place the explanatory caption directly below its real image. Retain full run URLs and digests rather than mixing identifiers from different releases. For old persistence evidence, say it is the October 12 observation and use the existing saved comparison only if it is still available; never reconstruct it as though it were a historical capture. After cleanup the temporary record should remain absent.

Redact credentials, JWTs, signing keys, passwords, private keys, credential-bearing connection strings, and personal data. Keep environment/run/time identity visible. Review the final captions and redactions before marking a capture collected. The README and presentation/report must later include captions and interpretations as required by Phase 7; no final delivery is claimed here.
