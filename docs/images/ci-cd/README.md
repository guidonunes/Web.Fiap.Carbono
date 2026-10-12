# CI/CD evidence index

Nine real user-supplied PNGs are now tracked in the checkout and visually reviewed. Their original filenames are preserved below. On October 12, 2026, Codex also downloaded and verified the existing PR recovery report and outage artifact, preserving their exact contents here. Together with the earlier dated persistence results, this closes Phase 6's evidence checks. The [verification guide](../../ci-cd-verification.md) records acceptance and attribution; [Phase 7](../../ci-cd-roadmap.md#phase-7--october-12-documentation-and-submission-package) still covers the final README, presentation/report, checklist, and source ZIP. New captions/records are uncommitted; no image editing or Git publication was performed by Codex.

| Capture to collect | Actual evidence required | Status |
| --- | --- | --- |
| `01-pr-ci.png` | Successful PR/run identity and restore/build/tests; delivery jobs skipped | [11-build-before-merge.png](11-build-before-merge.png), verified [summary](pr-recovery-summary.txt) and [107-passing-test TRX](pr-recovery-ci.trx) |
| `01b-pr-ci-failure.png` | Controlled PR test failure, retained summary/TRX, delivery jobs skipped | Overview collected as [10-failure-evidence.png](10-failure-evidence.png); screenshot shows one artifact, not its contents |
| `02-master-pipeline.png` | Successful run, ordered CI/publication/staging/production | Collected as [12-successful-workflow.png](12-successful-workflow.png), the original successful master overview |
| `03-docker-hub-image.png` | `gui2506/web-fiap-carbono`, full commit-SHA tag, matching registry digest | Collected as [docker-hub-image.png](docker-hub-image.png); full SHA/digest agree with the user's registry command |
| `04-staging.png` | Healthy staging containers, port `8081`, actual database-backed HTTP `200`/`EMP-001` | Combined [environment capture](14-post-recovery-environments.png), [API capture](09-api-auth-analytics.png), and dated smoke results |
| `05-production.png` | Healthy production containers, port `8082`, same promoted digest and actual smoke result | Same combined captures; both runtime images match the registry digest; recovery job succeeded |
| `06-failure-blocking.png` | Real staging database outage, recovery, skipped production, unchanged production metadata | [Overview](13-failed-staging-test.png), [detailed log capture](15-staging-outage-recovery.png), and verified [retained log](staging-failure.txt) |
| `07-persistence-isolation.png` | Preserved temporary record, production absence, and separate resources | [Earlier actual persistence commands/results](../../ci-cd-deployment.md#storage-preservation-verification--october-12-2026-utc) plus [current isolation capture](14-post-recovery-environments.png); no separate historical snapshot PNG was supplied or invented |
| `08-deployment-concurrency.png` | Actual A/B run links and UTC timing proving non-overlap | Collected and reviewed: [08-deployment-concurrency.png](08-deployment-concurrency.png) |
| `09-api-auth-analytics.png` | Sanitized API checker results for login/roles, persisted calculation, and analytics | Collected and reviewed: [09-api-auth-analytics.png](09-api-auth-analytics.png) |

## Reviewed captures

The capture times of the PNG files were not supplied. Dates/times below identify the actual execution shown or the associated independently inspected GitHub jobs; file timestamps are not treated as capture timestamps. All nine images are readable and contain no visible passwords, signing keys, private keys, or tokens. This is a visual review of these captures, not an exhaustive secret audit. No images were generated, edited, renamed, or staged by Codex.

### Docker Hub SHA tag and registry digest

![Docker Hub full-SHA tag and manifest digest](docker-hub-image.png)

The user's Docker Hub tag-details capture shows `gui2506/web-fiap-carbono:b1f2bdb4783e9cdc97f1b9c34c6279a772c03b22`, manifest digest `sha256:12e855e8d4552b394e41507cb300fbd33ff99170a016306e72745dadd47ef619`, and `linux/amd64`. The user's registry command names the same digest and media type `application/vnd.docker.distribution.manifest.v2+json`. The post-recovery capture below now proves both runtime images match this observed SHA-tag/digest mapping. Keep it separate from the earlier pre-outage production digest `97355d...` and original-success capture; no digest is retroactively assigned to the original publication attempt.

### API authentication, calculation, and analytics

![EC2 API verification in staging and production](09-api-auth-analytics.png)

The user's EC2 command `python3 scripts/phase6-api.py --environment both` displays execution timestamp `2026-10-12T19:29:29.533439+00:00`, both ports, expected HTTP statuses and roles, fifteen persisted emission calculations per environment, analytics totals, production rejection of staging's token, the final success message, and exit `0`. Password prompts are hidden and tokens omitted. This is actual user-supplied execution and satisfies the API screenshot evidence. No workflow SHA or image digest is visible, and the checker did not create a new calculated emission.

### Controlled PR failure

![Failed PR CI and skipped delivery jobs](10-failure-evidence.png)

[PR #1](https://github.com/guidonunes/Web.Fiap.Carbono/pull/1), [run 38302777058, attempt 1](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38302777058/attempts/1), commit `70743919e4a209ddf5e46c12d3d8fcf53e5cdcdb`, October 12, 2026. The image shows the pull-request trigger, failed CI, one artifact, and skipped publication/staging/production. Read-only job metadata separately confirms the controlled-test preparation succeeded, restore/build succeeded, tests failed, and diagnostic summary/artifact upload succeeded. CI ran `20:13:18`–`20:14:17 UTC`. The screenshot does not display the failing test name, test counters, or downloaded report contents; do not infer those from the overview.

### Successful PR recovery, delivery still skipped

![Successful PR recovery and explicit skipped image job](11-build-before-merge.png)

[Run 38302777058, attempt 2](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38302777058/attempts/2), the same PR/commit as above, October 12, 2026. The image shows green CI, `Latest #2`, and the explicit skipped image job; both deployment jobs also show skipped status. CI ran `20:27:25`–`20:28:32 UTC`. Codex subsequently downloaded artifact `11771429292`, created at `20:28:29 UTC`: its [summary](pr-recovery-summary.txt) reports restore/build/tests success, and its [TRX](pr-recovery-ci.trx) contains 107 passed, 0 failed, 0 not executed. The diagnostic commit is `744e332c996e07749ea4a18da64f76aa0ab57401`; preserve it separately from the PR head commit `70743919...` and master release `b1f2bdb...`. This is a successful PR check with delivery excluded.

### Original successful master pipeline

![Successful master checks, publication, staging, and production](12-successful-workflow.png)

[Run 38304927773, original attempt](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38304927773/attempts/1), commit `b1f2bdb4783e9cdc97f1b9c34c6279a772c03b22`, October 12, 2026. The screenshot shows the `push` to `master`, short SHA `b1f2bdb`, green CI/publication/staging/production in order, and the `ci-results` artifact. It does not show a registry image digest or host container metadata. Keep this original-success capture separate from the later controlled failure (attempt 2) and recovery (attempt 3); it is not a screenshot of the post-outage recovery.

### Staging failure blocks production

![Successful CI and publication, failed staging, skipped production](13-failed-staging-test.png)

[Run 38304927773, attempt 2](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38304927773/attempts/2), the same master commit as above, October 12, 2026. The screenshot shows successful CI/publication, failed staging with exit `1`, two artifacts, and skipped production. Normal staging deployment succeeded before the controlled database-failure step failed. Staging ran `20:45:43`–`20:47:15 UTC`. The detailed capture and downloaded artifact below now provide its database restoration and unchanged-production proof.

The separately inspected [attempt 3](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38304927773/attempts/3) has successful staging (`20:50:06`–`20:50:12 UTC`) and production (`20:50:14`–`20:50:26 UTC`) with the controlled failure step skipped. Its CI/publication entries retain attempt 2's earlier timestamps; recovery reused those results. The post-recovery environment report below supplies actual runtime acceptance; a new overview screenshot of attempt 3 is unnecessary.

### Post-recovery images, health, and environment isolation

![Recovered staging and production match the registry digest](14-post-recovery-environments.png)

The user's EC2 checker displays timestamp `2026-10-12T22:42:11.243395+00:00`, the exact expected `--image` reference, both matching API references ending in `12e855e8...`, four healthy containers, ports `8081`/`8082`, separate named volumes/networks, no MongoDB host ports, valid runtime configuration, different signing keys, empty packaged signing keys, and exit `0`. It links the observed registry SHA tag to both recovered runtime images. This is user-supplied host execution reviewed visually by Codex, not direct Codex EC2 access. It also supplies the current isolation screenshot; record persistence is credited to the earlier actual Phase 5 recreation/comparison results, not inferred from volume names alone.

### Actual staging outage, restoration, and unchanged production

![Database outage rejected, staging restored, production unchanged](15-staging-outage-recovery.png)

This user's capture shows the real controlled-failure step from run `38304927773`, attempt 2, staging job `115001048408`. Verification started at `2026-10-12T20:45:54Z`; staging MongoDB stopped, ten database-backed requests timed out, smoke rejected the outage, and staging recovered with HTTP `200`/`EMP-001`. Production's before/after container ID is exactly `a1f9a27648e3673969ad0143eb5b1c04cca5c1ae140ba2f1deab60dd284d72a0`, image ends in `97355d07831b80b2fff9ed6f886e2d71a7e6ca61009adb545851e9aed4babe1f`, and start time is `2026-10-12T20:34:47.553875021Z` in both lines. Production's smoke passed; the final recovery marker precedes intentional exit `1` and the skipped production job.

Codex downloaded artifact `11772867181`, created at `20:47:14 UTC`, and preserved its exact [staging-failure.txt](staging-failure.txt). The archive/log verification confirms identical production metadata, all stop/rejection/restoration markers, and four successful smoke messages across baseline and recovery. Availability was sampled before/after the outage, not continuously measured. The preserved `97355d...` image is the outage baseline; the later recovered deployment uses `12e855e8...`.

### Complete release sequences do not overlap

![Real GitHub A/B run timestamps demonstrate serialized releases](08-deployment-concurrency.png)

The user's terminal capture names the two original run/attempt URLs and reads actual GitHub job metadata: A (`38182523444`) ended production at `2026-10-11T23:01:00Z`; B (`38182543656`) was created at `22:59:07Z` while A ran and started CI at `23:01:03Z`. These match Codex's independent read-only inspection. A's SHA is `107d6b29661a5cf8704ff8c45f0cad2ca57bf35f`; B's is `44704e096442fd385a404ed196d83d222dc0934c`. B's first job started three seconds after A's final job completed. No superseded pending run is claimed for this pair.

## Coverage and handoff

The combined captures cover the proposed evidence slots without duplicating existing images. Persistence uses the dated original Phase 5 commands/results and current isolation capture; no standalone historical persistence PNG was supplied or invented. The PR's successful report and actual outage log are preserved as additional raw evidence. Exact failed-PR report contents were not independently downloaded; its test failure/retention and blocked delivery are verified by job metadata and the original overview. Earlier Phase 4 records preserve the user-confirmed downloaded failure-report check. Phase 6 is complete on this attributed evidence; finish the README/report and final source package in Phase 7.

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
