# Carbon API — release record and manual recovery

This document implements Phase 5 task 7 of the [active CI/CD roadmap](ci-cd-roadmap.md). GitHub evidence below was inspected read-only on October 11, 2026. Recovery commands are a procedure for the user to execute when needed; Codex has not executed a rollback or verified persistence through recovery.

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

## Manual recovery to a previously verified release

Use this procedure after a later release fails or needs to be replaced. Select an earlier passing release's exact commit and registry digest from a recorded run. The `57bf502...` / `dc309ed...` pair above is the current known-good baseline for future recovery; it is not a claim that a rollback has already occurred. Do not use the failed run's digest, a local image ID, or a mutable tag as recovery evidence.

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

Record recovery time, source commit, digest, environment results, and unchanged mount names. These mount checks do not prove record persistence; the temporary-record/redeployment exercise remains pending under the Phase 5 exit gate and Phase 6 plan.

Review queued runs and the intended next release before restarting the idle deployment service. A queued newer release can replace the manually selected API image once automatic delivery resumes. Keep the recovery checkout detached until deliberately selecting another revision; no push is needed for manual recovery.

```bash
cd /home/ubuntu/actions-runner
sudo ./svc.sh start
sudo ./svc.sh status
```

Expected: the service is running and the runner is online/idle in repository settings. Resume only after both environments have passed verification or the remaining failure has an explicit recovery plan. This procedure has been reviewed against the current script/workflow/Compose source and shell syntax; service operations, deployment, storage comparisons, and recovery results have not been executed by Codex.

## Record later releases

For each accepted release, append its date, run URL, full commit SHA, published digest, staging/production job timing, and actual smoke results. Keep failures and recovery attempts separate. Use narrow image/mount inspections rather than dumping container environment values. Collect real screenshots and the outstanding persistence evidence in their assigned phases; do not mark them complete from this guide alone.
