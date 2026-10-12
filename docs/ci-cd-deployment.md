# Carbon API — release record and manual recovery

For the current EC2 verification commands, controlled failure exercises, and screenshot requirements, use the [Phase 6 verification guide](ci-cd-verification.md). Historical release/persistence identifiers below retain their original dates and attribution.

This document records Phase 5's release, recovery procedure, and completed storage-preservation check from the [active CI/CD roadmap](ci-cd-roadmap.md). GitHub evidence was inspected read-only on October 11, 2026; the later EC2 persistence results below were supplied by the user. Recovery commands remain a procedure for use when needed; Codex has not executed a rollback or host verification.

## Verified release — October 11, 2026

| Field | Recorded value |
| --- | --- |
| Repository | `guidonunes/Web.Fiap.Carbono` |
| Workflow | [CI run 38184996037](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38184996037) |
| Trigger and branch | `push` to `master` |
| Commit | `57bf5021356a8796b33df175ddde056e13559df6` (`fix deploy`) |
| Run created / completed, UTC | `2026-10-11T23:37:46Z` / `2026-10-11T23:39:57Z` |
| Result | Successful CI, publication, staging, and production |
| CI tests | 107 passed, 0 failed, 0 skipped, as reported by the actual run log |
| Docker Hub SHA tag | `gui2506/web-fiap-carbono:57bf5021356a8796b33df175ddde056e13559df6` |
| Published registry digest | `sha256:dc309ed5084582eaa51a8f0143370a26e12dd41a69d811c9e85ba7bebb478b8e` |
| Application image for both environments | `gui2506/web-fiap-carbono@sha256:dc309ed5084582eaa51a8f0143370a26e12dd41a69d811c9e85ba7bebb478b8e` |

The digest comes from the actual publication log. Both deployment logs selected that exact image reference, reported healthy API/MongoDB containers, and printed `Smoke check passed: HTTP 200 and EMP-001` followed by their environment's `Deployment verified` message. The smoke script requires HTTP `200`, one JSON array, and a seeded company with `codigo` equal to `EMP-001`.

| Job | Started, UTC | Completed, UTC | Result |
| --- | --- | --- | --- |
| Restore, build and test | `23:37:50` | `23:38:54` | Success |
| Build and publish application image | `23:38:57` | `23:39:32` | Success |
| Deploy and verify staging | `23:39:33` | `23:39:45` | Success; smoke passed at `23:39:43` |
| Deploy and verify production | `23:39:46` | `23:39:57` | Success; smoke passed at `23:39:56` |

These timestamps are from October 11, 2026. Production started after staging completed. CI/publication ran on GitHub-hosted Ubuntu; deployment ran on the existing Ubuntu EC2 host's self-hosted runner selected by `self-hosted`, `linux`, `x64`, and `carbono-deploy`.

| Environment | Compose project | Untracked host configuration | Host-local API check | Persistent database volume |
| --- | --- | --- | --- | --- |
| Staging | `carbono-staging` | `/home/ubuntu/carbono/staging/staging.env` | `http://127.0.0.1:8081/api/empresas` | `carbono-staging_fiap_carbono_mongodb_data` |
| Production | `carbono-production` | `/home/ubuntu/carbono/production/production.env` | `http://127.0.0.1:8082/api/empresas` | `carbono-production_fiap_carbono_mongodb_data` |

Environment arguments were checked against the workflow source. Volume names were supplied by the user during Phase 3; they were not re-inspected on EC2 for this documentation task. GitHub logs establish the release's execution results, not continuous availability afterward. No independent current EC2 or Docker Hub inspection, downloaded reports, or screenshots were produced here.

The user subsequently supplied `true` from `.Jwt.SecretKey == ""` in each deployed API's `/app/appsettings.json` and confirmed reviewing workflow logs without finding signing keys, tokens, passwords, or environment-file contents. The GitHub repository is private and the Docker Hub repository is public, as confirmed by the user. Docker Hub publication uses GitHub Secrets; runtime settings remain in the two protected host files. The public image does not require deployment-host registry login.

This successful run follows [failed run 38183852655](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38183852655), whose staging image-selection check failed before updating containers and blocked production. Commit `57bf502...` fixes that check. Keep the failed run and older successful digests as separate historical evidence.

## Accepted Phase 6 release and outage recovery — October 12, 2026

This later acceptance supplements the October 11 release above without replacing its identifiers. Host/registry observations come from the user's actual outputs/images; Codex inspected existing GitHub metadata, downloaded retained reports/log, and reviewed the captures, without running an EC2 deployment.

| Field | Accepted evidence |
| --- | --- |
| Master source commit | `b1f2bdb4783e9cdc97f1b9c34c6279a772c03b22` |
| Master run | [38304927773](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38304927773) |
| Versioned registry tag | `gui2506/web-fiap-carbono:b1f2bdb4783e9cdc97f1b9c34c6279a772c03b22` |
| Registry and accepted recovery runtime digest | `sha256:12e855e8d4552b394e41507cb300fbd33ff99170a016306e72745dadd47ef619` |
| Runtime acceptance | User capture `14-post-recovery-environments.png`, `2026-10-12T22:42:11.243395+00:00`, exact `--image` match in both APIs, four healthy containers, isolated resources/configuration, exit `0` |
| PR check | [PR #1/run 38302777058](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38302777058), head `70743919e4a209ddf5e46c12d3d8fcf53e5cdcdb`; original controlled failure then successful recovery, delivery skipped in both |
| Independently inspected successful PR report | Retained artifact `11771429292`: 107 passed, zero failed/not executed; diagnostic commit `744e332c996e07749ea4a18da64f76aa0ab57401` |
| Controlled outage | Attempt 2, staging job `115001048408`, verified at `2026-10-12T20:45:54Z`; database stopped, smoke rejected outage, staging restored, production unchanged/available, intentional exit `1`; production job skipped |
| Deployment recovery | Attempt 3: staging `20:50:06`–`20:50:12 UTC`, production `20:50:14`–`20:50:26 UTC`, both successful; failure step skipped; prior successful CI/image jobs reused |

During the outage, production retained container `a1f9a27648e3673969ad0143eb5b1c04cca5c1ae140ba2f1deab60dd284d72a0`, image `gui2506/web-fiap-carbono@sha256:97355d07831b80b2fff9ed6f886e2d71a7e6ca61009adb545851e9aed4babe1f`, and start time `2026-10-12T20:34:47.553875021Z`. The same three fields appear before and after in the actual [retained outage log](images/ci-cd/staging-failure.txt), whose stop/recovery markers and four successful baseline/recovery smoke checks were verified. This old image is the preserved failed-release baseline; the later successful deployment uses `12e855e8...`.

The [evidence index](images/ci-cd/README.md) captions all nine original images and links the preserved [PR summary](images/ci-cd/pr-recovery-summary.txt)/[TRX](images/ci-cd/pr-recovery-ci.trx). Current storage isolation supplements the already executed persistence check below; no repeat recreation or standalone historical persistence PNG is claimed. Both exercise variables were independently read as `false` on October 12. The registry-to-runtime association is observed through the user's tag lookup/capture and exact runtime comparison; no original publication-job digest is retroactively inferred, and no fresh publication is claimed for recovery attempt 3. Later commits/runs need their own release identity.

Phase 6 is complete on this evidence. Final README/report/checklist/ZIP work belongs to Phase 7. No rollback, new deployment, workflow trigger, Git publication, or application build/test was executed by Codex for this completion update.

## Storage-preservation verification — October 12, 2026 (UTC)

**Phase 5 is complete based on the earlier release/failure/concurrency evidence and the user's supplied storage-preservation, isolation, cleanup, and runner-service results.** The dates in this section follow the host's UTC timestamps. Codex reviewed the supplied output without executing these commands on EC2.

| Check | User-supplied result |
| --- | --- |
| Runner suspended before manual recreation | `inactive (dead)` at `2026-10-12 00:19:31 UTC` |
| Captured staging API image, exported as `APP_IMAGE` | `gui2506/web-fiap-carbono@sha256:a3ae2c511135228a589224d1c31bbcebb3dbfe7d46d2dfc147ee491411a0b3ff` |
| Original staging MongoDB container ID | `aab2af55d7435324efc82e79ca954999b86ebfe5eca37ad00464d52061d27506` |
| Recreated staging MongoDB container ID | `5580882b53d1be47b16a116c89371ebb8740344be69dfc91157053b9dfc0f565` |
| `/data/db` mount before and after recreation | `carbono-staging_fiap_carbono_mongodb_data` |
| Temporary product business key | `CRUD-TEMP-PHASE5-PERSISTENCE` |
| Temporary product ObjectId | `6acc284f1465a1dca91ca03a` |
| Temporary product name | `CRUD-TEMP: Phase 5 persistence` |
| Temporary product creation/update time | `2026-10-12T00:22:39.955Z` |
| Compose recreation result | Staging API and MongoDB both reported `Healthy` |
| Before/after record comparison | Printed `Persistence comparison passed`; no differences reported |
| Matching product count in production | `0` before insertion and after staging recreation |
| Host-local API responses after recreation | `Port 8081: HTTP 200` and `Port 8082: HTTP 200` |
| Exact temporary-record cleanup | `acknowledged: true`, `deletedCount: 1`, `Remaining: 0` |
| Runner service restored | `active (running)` at `2026-10-12 00:30:51 UTC`, with the service enabled |

The captured `a3ae2c...` image is a later staging observation. No corresponding commit or workflow run was supplied for this check; do not attach it to run `38184996037`, whose recorded digest remains `dc309ed...`. No new production image reference was supplied either. Preserve both observations separately.

The user copied seeded product `PRO-001`, checked its company reference, assigned a new ObjectId and UTC timestamps, removed its copied `legacyId`, and changed its code/name to the temporary values above. The original seeded product was not modified. The saved snapshot retained the product's BSON/decimal environmental attributes and existing `empresaId` reference.

The user recreated only staging using its captured image, existing project, and protected environment file:

```bash
docker compose \
  --file /home/ubuntu/carbono/repository/docker-compose.yml \
  --project-name carbono-staging \
  --env-file /home/ubuntu/carbono/staging/staging.env \
  up -d \
  --no-build --pull missing \
  --force-recreate \
  --wait --wait-timeout 120 \
  api mongodb
```

The before snapshot was `/tmp/carbono-phase5-before.json` on EC2. After recreation, the user asserted exactly one matching product and saved it to `/tmp/carbono-phase5-after.json`. These are the `mongosh printjson` snapshots, not downloaded repository artifacts. The comparison ran in a `set -euo pipefail` subshell:

```bash
(
  set -euo pipefail
  diff -u \
    /tmp/carbono-phase5-before.json \
    /tmp/carbono-phase5-after.json

  printf 'Persistence comparison passed\n'
)
```

The supplied success message establishes that `diff` returned `0`; a missing or changed record would have stopped the preceding fail-fast block. The MongoDB container ID changed while its volume name stayed the same, establishing actual recreation with retained storage. Production's matching count remained zero, and both database-backed API URLs returned HTTP `200` afterward.

Cleanup matched the exact ObjectId, business key, and temporary name. The user then restarted the service `actions.runner.guidonunes-Web.Fiap.Carbono.carbono-ec2.service` through `/home/ubuntu/actions-runner/svc.sh`; its status showed `active (running)` and `enabled`.

This check satisfies the second Phase 5 exit gate and the record-persistence item planned for Phase 6. Verification covers staging recreation using the existing digest and unchanged temporary record. Initialization logs were not supplied, so no claim is made that every seed/startup message was inspected. Production availability was checked after staging recreation, not continuously throughout it. A production container recreation, rollback to an older image, GitHub runner online/idle status, and new pipeline/screenshot evidence were not demonstrated by these outputs. The remaining Phase 6 checks and evidence collection stay pending.

## Manual recovery to a previously verified release

Use this procedure after a later release fails or needs to be replaced. Select an earlier passing release's exact commit and registry digest from a recorded run. The `57bf502...` / `dc309ed...` pair above is a documented known-good baseline for recovery; the later `a3ae2c...` observation does not replace that run/commit record. No rollback has been executed in the recorded persistence check. Do not use the failed run's digest, a local image ID, or a mutable tag as recovery evidence.

Recovery changes the API image using the existing [deployment script](../scripts/deploy.sh). Preserve both environment files, Compose project names, MongoDB volumes, and database contents. Application rollback does not reverse database writes. Automatic rollback is outside this task.

### 1. Prepare the matching manual checkout on EC2

Run these commands **inside the EC2 SSH terminal**, using the existing private-repository access. The checkout at `/home/ubuntu/carbono/repository` is for manual administration; automated jobs use their own runner checkout. This block refuses a dirty manual checkout, fetches source, and selects the recorded recovery revision without resetting user changes:

```bash
(
  set -euo pipefail
  cd /home/ubuntu/carbono/repository

  if [[ -n "$(git status --porcelain)" ]]; then
    printf 'Preserve local changes before preparing recovery\n' >&2
    exit 1
  fi

  git fetch origin master
  git switch --detach '57bf5021356a8796b33df175ddde056e13559df6'
  test "$(git rev-parse HEAD)" = '57bf5021356a8796b33df175ddde056e13559df6'
  bash -n scripts/deploy.sh scripts/smoke.sh
)
```

Expected: the checkout matches the recorded commit and syntax validation exits `0`. If preparation fails, stop and resolve the source/access issue before deployment. Do not modify the runner's active `_work` checkout or put credentials into commands or tracked files.

### 2. Prevent overlap and record the existing state

In GitHub Actions, wait for all running release workflows to finish and resolve any queued release before starting manual recovery. Confirm this EC2 runner is the only runner serving `carbono-deploy`. Do not stop its service while a job is running. GitHub workflow concurrency does not serialize commands typed into an SSH terminal, so stop the **idle** runner for the entire manual staging-to-production sequence:

```bash
cd /home/ubuntu/actions-runner
sudo ./svc.sh stop
sudo ./svc.sh status
```

Verify the service is stopped/inactive before proceeding; the status command may return nonzero for an inactive service. A newly triggered deployment job will wait while this runner is stopped. Coordinate pushes during this window and review any newly queued release before resuming the runner.

Record the existing API references and MongoDB mounts without printing environment configuration:

```bash
docker inspect --type container \
  --format '{{.Name}}: {{.Config.Image}}' \
  carbono-staging-api-1 carbono-production-api-1

docker inspect --type container \
  --format '{{.Name}}: {{range .Mounts}}{{if eq .Destination "/data/db"}}{{.Name}}{{end}}{{end}}' \
  carbono-staging-mongodb-1 carbono-production-mongodb-1
```

Retain the actual output and date for comparison. The expected database mounts are the distinct volumes in the environment table above. Resolve an unexpected/missing mount before recovering; do not initialize a replacement database to hide the mismatch.

### 3. Recover staging, then production, without deleting storage

Inside EC2, run the following after preparation and runner suspension succeed. The subshell stops if staging fails, so production is invoked only after staging's health and database-backed smoke check pass:

```bash
(
  set -euo pipefail
  cd /home/ubuntu/carbono/repository
  test "$(git rev-parse HEAD)" = '57bf5021356a8796b33df175ddde056e13559df6'

  CARBONO_RECOVERY_IMAGE='gui2506/web-fiap-carbono@sha256:dc309ed5084582eaa51a8f0143370a26e12dd41a69d811c9e85ba7bebb478b8e'

  bash scripts/deploy.sh \
    carbono-staging \
    /home/ubuntu/carbono/staging/staging.env \
    "$CARBONO_RECOVERY_IMAGE" \
    http://127.0.0.1:8081/api/empresas

  bash scripts/deploy.sh \
    carbono-production \
    /home/ubuntu/carbono/production/production.env \
    "$CARBONO_RECOVERY_IMAGE" \
    http://127.0.0.1:8082/api/empresas
)
```

Expected: exit `0`, a passing smoke check, and `Deployment verified` for each project. The script pulls the selected API digest and uses `up --no-build --wait`, retaining existing project volumes and fresh-database-only initialization. Do not run `down --volumes`, volume removal/pruning, database reset, or the replacing seed during recovery. If a check fails, retain the failure output, leave subsequent deployment blocked, and troubleshoot the affected environment.

### 4. Verify and resume automatic delivery

Repeat the two inspections in step 2. Both API references must match `CARBONO_RECOVERY_IMAGE` from step 3; each MongoDB mount must match its own saved pre-recovery mount. Run the existing smoke script once for each environment from the prepared manual checkout:

```bash
cd /home/ubuntu/carbono/repository
bash scripts/smoke.sh http://127.0.0.1:8081/api/empresas
bash scripts/smoke.sh http://127.0.0.1:8082/api/empresas
```

Record recovery time, source commit, digest, environment results, and unchanged mount names. Mount checks alone do not prove record persistence; the separate completed staging snapshot comparison is recorded above and satisfies Phase 5's storage-preservation acceptance. A future recovery needs its own results rather than reusing that earlier check as proof.

Review queued runs and the intended next release before restarting the idle deployment service. A queued newer release can replace the manually selected API image once automatic delivery resumes. Keep the recovery checkout detached until deliberately selecting another revision; no push is needed for manual recovery.

```bash
cd /home/ubuntu/actions-runner
sudo ./svc.sh start
sudo ./svc.sh status
```

Expected: the service is running and the runner is online/idle in repository settings. Resume only after both environments have passed verification or the remaining failure has an explicit recovery plan. This procedure has been reviewed against the current script/workflow/Compose source and shell syntax; service operations, deployment, storage comparisons, and recovery results have not been executed by Codex.

## Record later releases

For each accepted release, append its date, run URL, full commit SHA, published digest, staging/production job timing, and actual smoke results. Keep failures, recovery attempts, and manual verification observations separate. Use narrow image/mount inspections rather than dumping container environment values. Reuse the recorded staging-persistence result for Phase 6, then collect its real screenshots and the remaining evidence without inventing missing run/commit associations.
