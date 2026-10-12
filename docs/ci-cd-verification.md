# Carbon API — Phase 6 verification and evidence

Use this guide with the [active roadmap](ci-cd-roadmap.md) and [release/recovery record](ci-cd-deployment.md). Helpers were implemented and locally verified on **October 12, 2026**. Their existence does not establish EC2 execution or complete Phase 6. Keep actual run results and screenshots separate from local regression results.

## Start with the EC2 checks

The files are authored once in the local repository. The manual EC2 checkout at `/home/ubuntu/carbono/repository` needs copies before these commands will work. The automated runner uses its own checkout of the published commit, not that manual directory.

If the changes have not been pushed, copy the four host scripts from **your computer's repository terminal** using the same key and current public IPv4 address used for SSH. Replace the two assignments with your actual values; keep the quotes around paths. Do not paste the private-key contents:

```bash
CARBONO_KEY='/absolute/path/to/your-existing-key.pem'
CARBONO_HOST='CURRENT_EC2_PUBLIC_IPV4'
scp -i "$CARBONO_KEY" \
  scripts/phase6-environments.py scripts/phase6-api.py \
  scripts/phase6-staging-failure.sh scripts/smoke.sh \
  "ubuntu@${CARBONO_HOST}:/home/ubuntu/carbono/repository/scripts/"
```

Alternatively, after you publish the changes through your normal Git workflow, update the clean manual EC2 checkout to that revision. A `git pull` cannot retrieve uncommitted local changes. Copying these scripts alone does not update GitHub's workflow; the two optional failure steps require the updated workflow to be published too.

Run the following in **EC2's SSH terminal**, with both environments running and no release changing them:

```bash
cd /home/ubuntu/carbono/repository
mkdir -p artifacts/phase6

python3 scripts/phase6-environments.py \
  > artifacts/phase6/environments.json
echo $?
cat artifacts/phase6/environments.json

bash scripts/smoke.sh http://127.0.0.1:8081/api/empresas
echo $?
bash scripts/smoke.sh http://127.0.0.1:8082/api/empresas
echo $?

python3 scripts/phase6-api.py --environment both
echo $?
```

Expected: each command exits `0`; the two smoke commands print `Smoke check passed: HTTP 200 and EMP-001`; the API checker finishes with `API verification passed`. Stop and inspect a failing check before treating it as evidence. These are live deployment checks; `dotnet test` remains on the GitHub-hosted CI runner and is unnecessary on EC2.

The environment report checks all four containers are healthy, both APIs use the same published digest, ports are `8081`/`8082`, MongoDB has no published port, volumes/networks are independent, runtime configuration exists, signing keys differ, and packaged JWT signing keys are empty. It prints only selected metadata and Boolean results, not raw container environment values. An optional `--image 'gui2506/web-fiap-carbono@sha256:FULL_DIGEST'` checks against the exact digest copied from the chosen successful workflow; an internally matching pair alone does not establish the association with that run.

### User-supplied environment report — October 12, 2026

The user supplied the report below after the manual EC2 checkout fast-forwarded from `230a6eb` to `fa133df`. `python3 scripts/phase6-environments.py` exited **`0`** at **`2026-10-12T19:16:15.810318+00:00`**. Codex did not execute this host command.

| Check | Staging | Production |
| --- | --- | --- |
| API image, both environments | `gui2506/web-fiap-carbono@sha256:028cd12fc13f1bd56fbe2e5182bea24259cae0f8ec6bb5867ec9e006be02a279` | Same exact reference |
| API / MongoDB health | `healthy` / `healthy` | `healthy` / `healthy` |
| API host port | `8081` | `8082` |
| MongoDB container ID | `fba828125d29c72578c95bc91facee975d9e0caf1343f856cf35b54f75b800c6` | `615eb74fb0ed312f6a5dee7509c54d7f62d4c21df5a82d691e9995b7bf3d2fe7` |
| MongoDB volume | `carbono-staging_fiap_carbono_mongodb_data` | `carbono-production_fiap_carbono_mongodb_data` |
| Network | `carbono-staging_default` | `carbono-production_default` |
| MongoDB published host ports | None | None |
| Runtime configuration valid | `true` | `true` |
| Packaged JWT key empty | `true` | `true` |

The report also returned `signing_keys_independent: true`. This completes task 6's resource/configuration checks together with the earlier production marker absence and post-recreation API responses. No current smoke/API output or screenshot was supplied with this report. The manual checkout revision is not proof that the running image was built from that same commit; no corresponding publication run/SHA was supplied for this digest. Capture this actual output for isolation evidence while retaining older release identifiers separately.

### Next: smoke and API contract checks

The API checker prompts for the **existing application account passwords**, hidden while typing: `admin@carbono.com` and `analista@carbono.com`. These are application passwords, not the SSH key or AWS password. It retains JWTs only in memory. It checks login, invalid credentials, `401`/`403` role restrictions, malformed request validation, every persisted emission's decimal quantity × applied factor, and footprint/ranking/dashboard totals. In `both` mode it also checks production rejects staging's token. It does not create domain records: its authenticated write-route probes contain invalid IDs or empty payloads. Its calculation assertion concerns persisted records; it does not demonstrate creation of a new calculated emission. It assumes the existing seeded academic dataset, at most 1,000 emissions, and the existing two demo identities.

Capture the sanitized report and smoke output for `04-staging.png`, `05-production.png`, and `07-persistence-isolation.png`; capture API `PASS` lines and the final result in an additional `09-api-auth-analytics.png`. Keep UTC date, environment, and the matching release identity visible. Do not capture password input, login response bodies, tokens, or raw `docker inspect` JSON.

### User-supplied API results — October 12, 2026

The user ran `python3 scripts/phase6-api.py --environment both` on EC2 at **`2026-10-12T19:29:29.533439+00:00`**. Both environments completed every assertion; the command printed `API verification passed; passwords/tokens omitted; no MongoDB records changed` and exited **`0`**. This completes Phase 6 task 7's execution checks. Codex reviewed the supplied output without executing the checker on EC2.

| Contract | Staging and production result |
| --- | --- |
| Company/product reads, required seeds present | HTTP `200`, passed |
| ADMIN and ANALISTA_ESG login/role identity | HTTP `200`, both expected roles passed; tokens omitted |
| Invalid account login | HTTP `401` |
| Protected delete without token / with invalid token | HTTP `401` / `401` |
| Analyst delete / factor-create restrictions | HTTP `403` / `403` |
| Admin malformed delete ID | HTTP `400` |
| Empty calculation payload with each permitted role | HTTP `400` for both roles |
| Decimal quantity × applied factor | All fifteen persisted emissions in each environment passed |
| Product footprint / supplier ranking / company dashboard totals | Passed in each environment |
| Staging token used on production | HTTP `401`; production rejected it |

The output includes no passwords or JWT values. Malformed write probes do not change domain records; the calculation checks validate existing emissions, not creation of a new one. No image digest or workflow/SHA association is captured by this command. Retain the earlier environment report's separate timestamp/digest without inventing a new release association. The standalone Bash smoke outputs were not supplied with these API results; keep that distinction in the happy-path record. Capture the actual `PASS` lines, timestamp, final message, and exit `0` in `09-api-auth-analytics.png`; no screenshot file has been supplied yet.

### User-supplied standalone smoke results — October 12, 2026

The user subsequently ran both Bash smoke commands in EC2's `/home/ubuntu/carbono/repository` terminal and supplied these results:

| Environment | Command | Output | Exit code |
| --- | --- | --- | --- |
| Staging | `bash scripts/smoke.sh http://127.0.0.1:8081/api/empresas` | `Smoke check passed: HTTP 200 and EMP-001` | `0` |
| Production | `bash scripts/smoke.sh http://127.0.0.1:8082/api/empresas` | `Smoke check passed: HTTP 200 and EMP-001` | `0` |

This completes current host-local smoke evidence: HTTP `200`, a single JSON array, and seeded `EMP-001` in each environment. Codex reviewed the user's output without executing host commands. No exact smoke execution timestamp or new runtime image/publication-run comparison was supplied. The earlier environment and API timestamps identify those checks only; do not present them as the timestamps of these smoke commands. Capture the actual terminal results for `04-staging.png`/`05-production.png`, with a caption preserving this attribution. Screenshot files and the complete happy-path release/PR association remain pending.

## 1. Complete the happy-path record

- [ ] Save an actual successful PR/run URL, SHA, successful restore/build/tests, and downloaded `ci-results` summary/TRX. Publication and both deployment jobs must be skipped on the PR.
- [ ] Save an accepted `master` run URL, full SHA, Docker Hub full-SHA tag and registry digest, staging/production order, and each environment's real smoke output.
- [ ] Run the EC2 checks above against that release, using `--image` to link runtime state to the publication digest.

The independently inspected October 11 [run 38184996037](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38184996037), SHA `57bf5021356a8796b33df175ddde056e13559df6`, already records 107 passing tests and successful publication/staging/production using digest `sha256:dc309ed5084582eaa51a8f0143370a26e12dd41a69d811c9e85ba7bebb478b8e`. Reuse its dated execution record; do not label it the currently deployed release without a current comparison. No independently inspected successful PR link or actual screenshots are recorded yet.

## 2. Verify a real PR test failure

After publishing the updated workflow, use **GitHub repository Settings → Secrets and variables → Actions → Variables**. These switches are ordinary configuration variables, not credentials. The workflow uses GitHub's [repository variable context](https://docs.github.com/en/actions/reference/workflows-and-actions/contexts).

1. During one verification window, set `PHASE6_VERIFY_CI_FAILURE` to the exact lowercase value `true`. Leave `PHASE6_VERIFY_STAGING_FAILURE` unset or `false`.
2. Open/update a temporary PR containing the updated workflow. The optional step creates one real failing xUnit test only inside the runner checkout on a PR; it never injects the test on a `master` push. Do not merge a deliberately failing PR.
3. Confirm restore/build succeed, tests fail with `Phase6CiFailureVerificationTest.ControlledFailureRetainsReportsAndBlocksDelivery`, and `ci-results` still uploads. With the current 107-test baseline, expect 107 passed and one controlled failure.
4. Download `ci-results`. Verify the summary says `Tests: failure` and the TRX names the controlled failure. Save the actual PR/run URL, SHA, UTC time, failure log, and skipped publication/deployment jobs. Capture `01b-pr-ci-failure.png`.
5. Delete the variable or set it to `false`, then rerun the PR's jobs. Download the successful report with zero failures and capture `01-pr-ci.png`. Close the exercise PR or continue through the normal passing review process.

PR gating alone skips delivery even on a passing PR. The earlier independently inspected failing `master` [run 38145711002](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38145711002) separately proves `needs: ci` blocks publication after a test failure. Keep that historical gate evidence alongside this PR/report exercise.

## 3. Verify an actual staging database outage blocks production

Run this through the updated workflow to prove the **production job is skipped**. Running the shell helper manually tests outage/recovery only and cannot prove GitHub job blocking.

1. Start with both environments healthy and no other manual operations. Save the current production container ID, image, start time, and HTTP result on **EC2**:

   ```bash
   docker inspect --type container \
     --format '{{.Id}} {{.Config.Image}} {{.State.StartedAt}}' \
     carbono-production-api-1
   bash /home/ubuntu/carbono/repository/scripts/smoke.sh \
     http://127.0.0.1:8082/api/empresas
   ```

2. Set GitHub variable `PHASE6_VERIFY_STAGING_FAILURE=true` for one verification release, with the CI-failure switch unset/`false`. Publish the updated workflow via the normal passing PR/`master` process. CI/publication and normal staging deployment must pass before the optional outage step runs.
3. The helper stops only `carbono-staging-mongodb-1`, using a bounded [Docker stop timeout](https://docs.docker.com/reference/cli/docker/container/stop/). The real database-backed smoke check must fail. Its exit handler restarts the same database container, waits for health, checks staging recovery, compares production's ID/image/start time, and checks production's API. It does not delete records or volumes or change production containers.
4. **Expected workflow result: staging fails intentionally with exit `1`, production is `Skipped`.** Require all these messages in the retained `phase6-staging-failure` artifact:

   ```text
   Staging smoke check rejected the database outage
   Staging database and API recovered
   Production unchanged and available
   Controlled database failure verified; staging recovered; production must be skipped
   ```

   An arbitrary red job is not proof: a preflight/recovery error, missing final marker, or canceled job does not pass this exercise. Capture the run URL/SHA/published digest, before/after production metadata, recovery output, and skipped production job in `06-failure-blocking.png` and additional captures as needed. Availability is sampled before/after the outage, not continuously measured.
5. Delete the switch or set it to `false` immediately after the exercise. Allow cleanup to finish; do not cancel the outage job while restoration is running. Both normal delivery gates stay unchanged. The verification run publishes/deploys a new staging digest while preserving production's previous digest; a temporary digest difference here is expected.
6. Publish a subsequent normal release with both switches off. Require all jobs green and repeat the EC2 checks against its digest. Record the recovery run and both smoke results.

If restoration reports an error, on EC2 run `docker start carbono-staging-mongodb-1`, inspect its health/logs, and rerun staging's smoke check before continuing. Do not remove the volume or reset the database. Catchable interrupts invoke cleanup; forced process termination or host loss cannot guarantee cleanup.

For an explicitly chosen manual outage window, use the [existing idle-runner suspension procedure](ci-cd-deployment.md#manual-recovery-to-a-previously-verified-release) before:

```bash
cd /home/ubuntu/carbono/repository
bash scripts/phase6-staging-failure.sh --verify
echo $?
```

Expected `1` **with the final recovery marker**. Restore the runner afterward. Prefer the workflow exercise above for Phase 6 acceptance; no separate manual outage is necessary.

## 4. Reuse verified concurrency evidence

Task 4 is complete from the user's earlier A/B confirmation and Codex's October 12 read-only inspection of these actual GitHub job timestamps. All times below are **October 11, 2026 UTC**:

| Run | Created | CI started | Staging started / ended | Production started / ended |
| --- | --- | --- | --- | --- |
| [A: 38182523444](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38182523444) | `22:58:44` | `22:58:48` | `23:00:35` / `23:00:47` | `23:00:48` / `23:01:00` |
| [B: 38182543656](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38182543656) | `22:59:07` | `23:01:03` | `23:02:43` / `23:02:55` | `23:02:57` / `23:03:08` |

A's SHA is `107d6b29661a5cf8704ff8c45f0cad2ca57bf35f`; B's SHA is `44704e096442fd385a404ed196d83d222dc0934c`. Both completed successfully. B was created while A ran, but its first job started three seconds after A's final production job completed. Together with the user's observed waiting status and the unchanged workflow-wide concurrency group, this demonstrates non-overlapping complete release sequences. No superseded pending run was identified in this pair; this is not a guarantee that every queued revision deploys.

Capture these real run identities/timings in `08-deployment-concurrency.png`; a new concurrency exercise is unnecessary.

## 5–7. Persistence, isolation, and API contracts

- [x] Persistence already passed in the user's October 12 EC2 recreation, unchanged-record comparison, exact cleanup, and runner restart. Reuse the commands/results in the [storage-preservation record](ci-cd-deployment.md#storage-preservation-verification--october-12-2026-utc). The temporary record is already deleted; do not claim it still exists or recreate it solely to manufacture historical evidence.
- [x] Complete the remaining isolation evidence with the environment checker. The user's October 12 report passed with exit `0`, supplementing the earlier production marker counts of zero before/after staging recreation and HTTP `200` from both APIs afterward. Task 6's execution checks are complete; screenshots remain pending. The separate outage helper will additionally compare unchanged production metadata and check availability; continuous availability is not claimed.
- [x] Run the API checker in both environments and retain its sanitized results for login, roles, decimal calculation, and analytics. The user's October 12 execution passed with exit `0`, including fifteen persisted emission calculations per environment and rejection of staging's JWT by production. The actual results are recorded above; screenshot capture remains pending.

## 8. Gather actual evidence and close the phase

The [screenshot index](images/ci-cd/README.md) lists each required capture and its context. Add real files and captions there, and retain downloaded reports before their fourteen-day retention expires. Keep PR and failure/recovery runs distinct. Add date/time, full SHA, digest where applicable, environment, command, result, and whether evidence was user-supplied or independently inspected. Do not add signing keys, passwords, tokens, private keys, or environment-file contents.

Both Phase 6 exit gates remain unchecked until the missing happy-path release/PR records, controlled PR/outage results, and readable screenshots are recorded. Isolation, API, and standalone smoke execution evidence are now recorded above; their screenshots are still pending. Phase 7 packaging, README/report updates, and the source ZIP are separate later work.

## Local validation of this implementation

Codex ran `python3 scripts/verify-phase6.py`: **17 regression cases passed** using mocked Docker/HTTP responses and the actual shell helpers; no EC2 operation occurred. They cover successful outage/recovery, catchable interruption, failed restoration, changed/unavailable production, baseline rejection, decimal/analytics contracts, and unsafe resource/configuration rejection with secret-output checks. Run this verifier locally, not as deployment evidence.

All workflow shell blocks and deployment/failure scripts passed Bash syntax checks. Workflow review preserved triggers, CI/publication/deployment gates, same-digest promotion, prior steps, and concurrency. A temporary local copy of the generated Fact produced **one expected failure and a TRX**; it was removed, then `dotnet test Web.Fiap.Carbono.sln --configuration Release --no-restore --logger 'trx;LogFileName=phase6.trx' --results-directory ./artifacts/phase6/local-tests` rebuilt and passed **107 tests, zero failed/skipped**. These are local results, not a real PR exercise. `artifacts/` is ignored to keep generated reports out of tracked source. Codex did not publish this revision, change GitHub variables, trigger workflows, deploy, or access EC2.

<!-- Temporary Phase 6 CI failure exercise. -->
