# Web.Fiap.Carbono — Academic CI/CD Roadmap

## Goal and working agreement

Deliver a simple, functional CI/CD implementation for **Projeto - Cidades ESGInteligentes** by **October 13, 2026**. Complete implementation and packaging by October 12; reserve October 13 for final checks and submission.

This is the active roadmap for CI/CD work. Follow [AGENTS.md](../AGENTS.md): the user implements application and infrastructure changes to learn; Codex explains, supplies snippets, reviews, and troubleshoots. Guidance requests do not authorize edits. An explicit implementation request authorizes only its named changes and necessary supporting work, without repeated confirmation. Work on one bounded phase at a time and explain the purpose and verification of each change.

Preserve .NET 8, MongoDB, JWT authentication/authorization, existing API behavior, decimal calculations, immutable emission snapshots, and exactly five ESG domain collections: `empresas`, `produtos`, `fornecedores`, `fatores_emissao`, and `emissoes_carbono`. Do not add unrelated features, framework upgrades, Kubernetes, extra hosts, or a new database design.

[roadmap.md](../roadmap.md) and [mongodb-migration.md](mongodb-migration.md) are migration history and architectural references. Their phase order does not govern CI/CD. Preserve historical evidence and distinguish it from new verification. A checked box or existing file alone does not prove a phase passed.

## Observed starting point — October 6, 2026

These observations come from the October 6 repository inspection only. No builds, tests, database scripts, container operations, or deployments were executed for that documentation update. Subsequent Phase 1 evidence is recorded below; the starting-point table is a historical snapshot.

| Component | Observed in the repository | Work still required |
| --- | --- | --- |
| Solution | Root `Web.Fiap.Carbono.sln` references the API, migration utility, and root test project; all target .NET 8 | Establish a fresh passing baseline and update paths after reorganization |
| API | `Web.Fiap.Carbono/Program.cs` registers MongoDB repositories/services and JWT; the API project references `MongoDB.Driver` without Oracle/EF Core packages | Preserve behavior and supply reproducible runtime configuration |
| Migration utility | `Web.Fiap.Carbono.Migration/` retains Oracle/EF Core dependencies and links `archive/oracle/Data/Contexts/DatabaseContext.cs` | Move under `src/` and repair linked paths; keep it separate from the API image/runtime |
| Tests | `Web.Fiap.Carbono.Tests/` contains xUnit, WebApplicationFactory, and Testcontainers fixtures | Run existing tests from a clean checkout; verify JWT configuration, content-root discovery, script paths, and runner compatibility |
| Test environment | API fixture uses Linux host networking and fixed port `27019`; migration fixture locates the solution root and `database/mongodb/` scripts | Use a Docker-capable Ubuntu runner and avoid port conflicts; verify these assumptions after moving projects |
| Dockerfile | `Web.Fiap.Carbono/Dockerfile` has .NET 8 build/publish/runtime stages and repository-root build context | Move to root and update copy/build paths; verify the resulting image |
| Compose | Root `compose.yaml` defines only `mongodb`, image `mongo:8.0.29-noble`, fixed `container_name`, loopback host port `27017`, named storage, and a ping health check | Establish canonical `docker-compose.yml` with API plus MongoDB; remove cross-project name/port collisions |
| Configuration | `.env.example` contains MongoDB connection/database examples; `.gitignore` excludes `.env`, `*.env`, and development appsettings | Add all required nonsecret examples, including JWT and deployment variables; verify secrets stay out of Git, image context, and ZIP |
| Database scripts | `database/mongodb/01-create-collections.js` through `05-aggregation-queries.js` exist; the seed uses `replaceOne(..., { upsert: true })` | Initialize only fresh databases; prevent later deploys from replacing existing records |
| Delivery automation | No `.github/workflows/` or deployment scripts found | Implement checks, image publishing, SSH deployment, and smoke checks |
| Documentation/artifacts | README documents MongoDB and manual Docker execution; migration report still contains older hybrid-persistence statements; a prior `Web.Fiap.Carbono.Source.zip` exists | Update current instructions in the assigned phases and create a new verified CI/CD package; do not reuse the old ZIP as proof |
| Deployment host | No provisioned host or working staging/production deployment verified | Obtain an Ubuntu host, access, capacity, and network connectivity before deployment work |

Historical migration documents record earlier test results. Those results are not a current CI/CD baseline. Record actual Phase 1 results in [ci-cd-baseline.md](ci-cd-baseline.md), keeping unverified work separate.

## Hosting decision — October 8, 2026

**Project:** Carbon API, repository `guidonunes/Web.Fiap.Carbono`, assignment deadline **October 13, 2026**. **Selected provider:** AWS EC2 with an Ubuntu virtual machine. Available AWS Free Plan credits and the local computer's disk-space constraint motivated using a cloud VM.

**Dated account observation, reported by the user on October 8, 2026:** the AWS console showed the Free Plan, **US$100 in available credits**, and a maximum free-period end date of **April 8, 2027**. Access can end earlier if credits are exhausted; this is not a guarantee of VM runtime until April. Keep the exercise within the existing Free Plan and available credits. A paid-plan upgrade is outside the current scope.

- [x] Select AWS EC2 as the deployment provider: explicitly confirmed by the user on October 8, 2026.
- [x] Check Free Plan status and available credits: the dated user-reported console observation is recorded above; remaining credits must be monitored during the exercise.

### Proposed host configuration

These are the original planning values, not proof of provisioned settings. Phase 3 records the actual host information and checks confirmed by the user. The EC2 instance type and EBS storage class have not been separately recorded from the console.

| Setting | Proposed value |
| --- | --- |
| Operating system / architecture | Ubuntu Server 24.04 LTS, x86-64 / AMD64 |
| Instance type / resources | `m7i-flex.large`, 2 vCPUs, 8 GiB RAM |
| Storage | 64 GiB gp3 |
| Initial preferred region | US East (N. Virginia), `us-east-1`, subject to account availability |
| API host ports | Staging `8081`; production `8082` |

The user subsequently launched the host in **US East (Ohio), `us-east-2`**, availability zone **`us-east-2c`**. Use this actual region for host operations; N. Virginia remains the initial preference only. Manual host administration uses SSH; automated deployment now uses a self-hosted GitHub Actions runner on the same EC2 VM, as selected on October 11 and recorded in Phase 5 task 2. Docker/Compose installation and Docker access for the `ubuntu` deployment account are confirmed by user-supplied terminal output on October 9. The user also confirmed the SSH host-key fingerprint matched the AWS system log on October 9.

## Planned architecture

**GitHub Actions builds and tests the application, publishes versioned images to Docker Hub, and deploys the selected image version to one Ubuntu AWS EC2 host.** Docker Engine and Docker Compose are the deployment runtime; the application remains ASP.NET Core / .NET 8 with MongoDB. The user has confirmed host provisioning, SSH access and host-key comparison in Phase 3, and supplied successful Docker/Compose verification output. On October 9, the user supplied healthy API/MongoDB container status for both environments and successful database-backed API responses from the EC2 host. Subsequent task 6 output confirms HTTP access from the authorized local computer and no published MongoDB host ports, with an external MongoDB connection timing out. Task 7 output confirms separate database volumes and networks, staging-only test data, and cleanup; both Phase 3 exit gates are satisfied based on user-supplied evidence. Automated deployment is subsequently verified in Phase 5 task 7; the user-supplied staging recreation/snapshot comparison recorded below verifies storage/record persistence and closes Phase 5. Each environment contains its own API and MongoDB instance. This hosting decision does not introduce CodePipeline, CodeBuild, ECR, ECS, EKS, or a managed database.

| Setting | Staging | Production |
| --- | --- | --- |
| Compose project | `carbono-staging` | `carbono-production` |
| Services | `api`, `mongodb` | `api`, `mongodb` |
| API host/container ports (Phase 3 task 5) | `8081:8080` | `8082:8080` |
| API URL from the authorized client (October 9, task 6) | `http://18.222.63.49:8081/api/empresas` | `http://18.222.63.49:8082/api/empresas` |
| Database name | `fiap_carbono` | `fiap_carbono` |
| MongoDB address from its API | Its own `mongodb:27017` service | Its own `mongodb:27017` service |
| MongoDB volume mounted at `/data/db` (October 9, task 7) | `carbono-staging_fiap_carbono_mongodb_data` | `carbono-production_fiap_carbono_mongodb_data` |
| Compose network (October 9, task 7) | `carbono-staging_default` | `carbono-production_default` |
| Host environment file (Phase 3 task 4) | `/home/ubuntu/carbono/staging/staging.env`, outside the Git checkout | `/home/ubuntu/carbono/production/production.env`, outside the Git checkout |
| Secrets | Staging-only runtime secrets and JWT signing key | Independent production runtime secrets and JWT signing key |
| Planned release image | Published `repository@sha256:...` | Exact digest that passed staging |

Use the same Compose definition with separate project names and environment files. Avoid fixed container names, shared external networks/volumes, or explicit resource names that bypass project isolation. MongoDB need not publish a host port; both instances can listen on container port `27017` independently. Use `docker compose exec` for database inspection. The same database name does not mean shared data.

The API container must receive `MongoDb__ConnectionString`, `MongoDb__DatabaseName`, `Jwt__SecretKey`, `Jwt__Issuer`, `Jwt__Audience`, and `Jwt__ExpirationMinutes`. Connect to the MongoDB service name, not `localhost` inside the API container. Document the chosen ASP.NET Core environment and HTTP port configuration. Swagger currently runs only in Development; a Swagger page is not a deployment readiness check.

Compose now supports `APP_IMAGE` (the deployment digest reference, with `web-fiap-carbono:local` as the local default) and `API_PORT`. Phase 5 task 1 records the user's implemented image selection and manual staging verification. A Compose `--env-file` supplies interpolation values; explicitly map the API's required values into the container. Keep credentials in GitHub Secrets or untracked host environment files, with independent environment settings. Commit placeholders only. Ensure `.dockerignore` excludes development settings and all actual secret files before building/publishing; Git ignore rules alone do not protect a Docker build context.

## Planned pipeline and checks

```text
Pull request → restore → build → existing tests → CI result

Push to master → restore → build → existing tests
  → build and push Docker Hub image tagged with full commit SHA
  → capture registry digest
  → EC2 runner deploys staging locally by digest → readiness + database-backed smoke check
  → EC2 runner deploys production locally by the same digest → readiness + database-backed smoke check
```

CI and image publication run on GitHub-hosted Ubuntu runners. Deployment jobs use the existing EC2 host's deployment runner, selected with `[self-hosted, linux, x64, carbono-deploy]`; pull requests do not execute on that runner. It receives jobs through outbound HTTPS and invokes Docker Compose locally. The user confirmed successful staging execution in task 2, then an all-green workflow including production and matching staging/production image references in task 4. Task 5 records user-confirmed failure blocking/concurrency; task 7 records a subsequently inspected successful run and both smoke results. The later user-supplied storage-preservation check completes Phase 5; screenshots and remaining Phase 6 checks stay pending.

Production follows automatically only after staging succeeds. Build once; do not rebuild for production or promote a mutable `latest` tag. Carry the registry digest as the publish job's output. Deploy the Compose/scripts revision associated with that commit as well, rather than pulling an unrelated newer `master` on the host.

Failures must return a nonzero exit status and block dependent jobs. PRs must never publish or receive deployment credentials. Use job dependencies to enforce checks → publication → staging → production, including smoke checks. Serialize the **whole staging-to-production deployment sequence across runs**, using one shared deployment concurrency group and `cancel-in-progress: false`; independent per-environment groups alone can interleave releases. Superseded queued runs need not deploy, but running deployments must not overlap. Manual deployments must not bypass that serialization.

**Phase 5 is complete:** tasks 5–7 record failure blocking/concurrency, credential checks, and the verified release/manual recovery procedure. The later user-supplied staging recreation, unchanged record comparison, retained volume, production checks, cleanup, and runner-service restart satisfy the final storage-preservation exit gate. Results are recorded below and in [docs/ci-cd-deployment.md](ci-cd-deployment.md). No A/B run links or raw timestamps were supplied; retain those for Phase 6 evidence collection. The next bounded work is Phase 6; do not begin it automatically.

**Latest deployment incident and recovery:** run `38183852655` failed staging's image-selection check and skipped production. The fixed revision `57bf5021356a8796b33df175ddde056e13559df6` then passed CI/publication/staging/production in independently inspected [run 38184996037](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38184996037). Both environments selected digest `sha256:dc309ed5084582eaa51a8f0143370a26e12dd41a69d811c9e85ba7bebb478b8e` and passed their database-backed smoke checks. Keep failed and successful run records separate; details follow in Phase 5.

Readiness means the API can serve requests after MongoDB and initialization are ready. Prefer a bounded retry check against the existing public `GET /api/empresas` endpoint; a new health endpoint is not required. Separately validate HTTP `200`, a JSON array, and a known seeded company's business key to prove a database-backed response. Choose and record that key after inspecting the seed; do not accept an empty response, HTML, redirect, or merely a running container. Confirm a database outage makes the check fail. Any container health-check command must actually be available in its image.

Use GitHub Secrets for Docker Hub publication credentials. Keep staging/production runtime secrets in their separate untracked host environment files, outside the runner checkout. The local deployment job does not need EC2 SSH secrets, AWS credentials, or security-group changes; do not print runner credentials or environment contents. Retain SSH host-key verification for manual administration. Use bounded job, readiness, and smoke-check timeouts; retain sanitized failure logs.

## Phase 1 — October 6–7: establish a passing baseline and reorganize

**Purpose:** prove the current solution works, then make the required submission layout without changing business behavior.

### Progress — October 7, 2026

The user reported that the pre-reorganization Release test run passed after disabling the VPN. Inspection of `artifacts/test-results/before/baseline.trx` confirms 106 executed tests, all passed, with zero failures or timeouts. The command, timestamps, evidence limits, and reported workaround are recorded in [ci-cd-baseline.md](ci-cd-baseline.md). The VPN's exact effect was not established.

The user also confirmed that `dotnet restore Web.Fiap.Carbono.sln` and `dotnet build Web.Fiap.Carbono.sln --configuration Release --no-restore` passed. Pre-reorganization restore/build success is based on user confirmation; missing original console details remain explicitly uncaptured.

Phase 1 implementation is complete. The API and migration utility are under `src/`, the Dockerfile is at the root, and project references/current documentation are updated. Post-move restore and Release build passed with zero build warnings/errors; all 107 tests passed from a clean source snapshot without developer settings. The extra test verifies synthetic JWT signing/validation configuration. The root Dockerfile built `web-fiap-carbono:baseline`. Commands, environment versions, exit codes, the TRX, and the local image ID are recorded in [ci-cd-baseline.md](ci-cd-baseline.md). No Phase 2 implementation or deployment was performed.

### Tasks

- [x] Inspect `git status`, SDK/Docker prerequisites, and existing test configuration. Ensure clean-checkout tests receive synthetic JWT settings and do not depend on ignored developer files. Preserve unrelated local work.
- [x] Run restore, Release build, and the complete existing tests before moving files: restore/build passed per user confirmation; the TRX records 106/106 tests passed after the reported VPN workaround.
- [x] Complete the baseline evidence with available console outputs, warning counts, exit codes, and environment details; the earlier timeout's exact cause remains unconfirmed.
- [x] Create `docs/ci-cd-baseline.md` during this phase with date, commit/revision, environment/tool versions, commands, exit codes, test counts, and sanitized evidence references. Separate results before and after reorganization.
- [x] Move `Web.Fiap.Carbono/` and `Web.Fiap.Carbono.Migration/` under `src/`. Retain `Web.Fiap.Carbono.Tests/`, solution, `global.json`, `database/`, and `archive/` at the root.
- [x] Move `Web.Fiap.Carbono/Dockerfile` to root `Dockerfile`; retain repository-root Docker build context and update `COPY`, restore, and working-directory paths.
- [x] Update solution paths and both test project references. Check the migration-to-API reference, which remains between sibling directories.
- [x] Update the API's linked `.dockerignore` path to reach the root, and the migration utility's archived `DatabaseContext.cs` link to reach root `archive/`.
- [x] Verify WebApplicationFactory content-root resolution, test script discovery from the root solution, and any remaining path assumptions. Repair only what the move requires.
- [x] Update current README/setup commands, migration utility guide links/commands, and other live path references affected by the move. Preserve `roadmap.md` as migration history and do not rewrite historical execution records as new results.
- [x] Repeat solution verification, build the relocated Dockerfile, and record actual results. Retain existing .NET 8 and dependency choices unless a narrowly scoped baseline fix is required.

### Verification

From the repository root, execute these commands during Phase 1, before and after reorganization as applicable:

```bash
git status --short
dotnet --info
docker version
dotnet restore Web.Fiap.Carbono.sln
dotnet build Web.Fiap.Carbono.sln --configuration Release --no-restore
dotnet test Web.Fiap.Carbono.sln --configuration Release --no-build
```

After the move, check references and build the image:

```bash
dotnet sln Web.Fiap.Carbono.sln list
rg -n 'ProjectReference|Compile Include|Content Include' src Web.Fiap.Carbono.Tests
docker build --file Dockerfile --tag web-fiap-carbono:baseline .
git diff --check
```

Verify all linked files exist and tests discover the real MongoDB scripts. Docker must be available for Testcontainers; a build alone is insufficient. Do not skip failing integration tests to establish a passing baseline.

### Exit gate

- [x] Restore/build/tests pass before and after the move; actual results are recorded in `docs/ci-cd-baseline.md`.
- [x] The required layout is present, the root Dockerfile builds, references resolve, and current instructions use valid paths.
- [x] Business behavior and historical artifacts remain preserved; no unexplained test regressions or missing prerequisites remain.

## Phase 2 — October 7–8: complete local Compose

**Purpose:** start the API and its persistent database reproducibly from the repository.

### Tasks

- [x] Rename/consolidate the current MongoDB-only `compose.yaml` into canonical root `docker-compose.yml`; avoid leaving competing default Compose files. Update active commands accordingly.
- [x] Define `api` and `mongodb`, API image/build settings, runtime environment variables, a project-scoped network, and a named MongoDB data volume.
- [x] Remove the fixed MongoDB container name and unnecessary host database port. Parameterize the API host port; use `8080` inside the API container.
- [x] Expand `.env.example` with all required nonsecret runtime/Compose values. Verify Git/image exclusion rules for `.env`, `staging.env`, `production.env`, and development settings, and document packaging exclusions. Final ZIP contents must still be verified in Phase 7; Git/Docker ignore rules alone do not verify a ZIP.
- [x] Configure MongoDB readiness and bounded API readiness; make API startup wait for completed fresh-database initialization. Verify HTTP behavior with the existing HTTPS-redirection middleware and chosen container configuration.
- [x] Wire only `01-create-collections.js`, `02-create-indexes.js`, and `03-seed.js` in that order into fresh-volume initialization, for example via Mongo's first-start init directory. Preserve `fiap_carbono`, five domain collections, validators, indexes, and coherent data. Fresh initialization verified in task 13 below.
- [x] Ensure database ping success alone cannot race incomplete initialization. Check a known seeded record before accepting API readiness.
- [x] Document fresh-volume versus existing-volume behavior. The seed replaces matching records: do not rerun it on every deployment. Never drop the database or automatically reset a failed/partially initialized volume. Report initialization failure for deliberate recovery. See the task 10 verification below.
- [x] Keep `04-crud-demo.js` and `05-aggregation-queries.js` available for manual evidence; do not execute demos as startup hooks. See the task 11 verification below.
- [x] Document local Compose setup, fresh initialization, existing-volume reuse, readiness checks, restart/recreation, and troubleshooting in README (task 12). Documentation and command syntax checked on October 8, 2026; this does not complete the remaining runtime exit gates.
- [x] Verify startup from a newly named development project/volume, then restart/recreate containers without deleting volumes and confirm stored data remains. Fresh startup passed in task 13; edited temporary-record persistence passed in task 15.

### Task 10 — preserve existing data without reseeding

The MongoDB image's first-start entrypoint runs the three mounted initialization scripts in filename order for a fresh `/data/db`. When MongoDB storage files already exist, it skips these scripts. This depends on initialized storage, not whether the five domain collections contain records. The API has no seed startup hook, and Compose does not override the MongoDB entrypoint to rerun the seed.

Keep the named volume and the same Compose project name when restarting or recreating an environment. A different project name selects a different project-scoped volume. Do not use `down -v`, delete the volume, or manually rerun `03-seed.js` against existing data: its replacing upserts can overwrite changes even when IDs and counts remain the same.

If first initialization fails, inspect `docker compose -p PROJECT logs --no-color mongodb` and the API health status. Preserve the volume for inspection and deliberate recovery; do not automatically reset it or rerun the seed. A partially initialized volume can already contain storage files, so restarting may skip the scripts without repairing the dataset. The API check requires `EMP-001`, but is not proof that every collection, index, or seed verification completed. Recovery must account for the actual failure and existing data.

**Verified on October 8, 2026:** the existing local `carbono-init-check` project was recreated using:

```bash
API_PORT=8083 docker compose -p carbono-init-check up \
  -d --force-recreate --wait --wait-timeout 120
```

The command exited successfully and both services became healthy. MongoDB's container changed from `8798f3595e52` to `0ac70efb0da2`, retaining volume `carbono-init-check_fiap_carbono_mongodb_data`. Before and after recreation, read-only queries sorted each collection by `_id`, serialized all documents with `EJSON.stringify(..., { relaxed: false })`, and computed SHA-256 hashes. All five hashes matched; all 55 documents were unchanged, including IDs, timestamps, and BSON values. Counts remained:

| Collection | Before | After |
| --- | --- | --- |
| `empresas` | 10 | 10 |
| `produtos` | 10 | 10 |
| `fornecedores` | 10 | 10 |
| `fatores_emissao` | 10 | 10 |
| `emissoes_carbono` | 15 | 15 |

The recreated MongoDB container's logs contained no initialization-script execution or seed-completion messages. No database writes, resets, or manual seed executions were performed during this check. The local API remains on port `8083`. This verifies task 10's existing-volume recreation behavior. Database-outage verification is recorded in task 14 below; temporary-record mutation/persistence is recorded in task 15.

### Task 11 — keep demonstrations out of startup

Keep `database/mongodb/04-crud-demo.js` and `database/mongodb/05-aggregation-queries.js` in the repository for deliberate manual evidence collection. Compose mounts only scripts `01`, `02`, and `03` individually and read-only into `/docker-entrypoint-initdb.d`. Do not replace these mounts with the whole `database/mongodb/` directory, which would include the demonstration scripts in fresh-database initialization.

**Verified on October 8, 2026:** both demonstration files exist in the repository. Inspection found no calls to them in the three initialization scripts, the Dockerfile, or API startup. The following read-only command against the running local MongoDB container exited successfully:

```bash
docker compose -p carbono-init-check exec -T mongodb \
  ls -1 /docker-entrypoint-initdb.d
```

Its complete output was:

```text
01-create-collections.js
02-create-indexes.js
03-seed.js
```

The demonstration scripts are therefore excluded from the running container's startup directory. Neither demonstration was executed for this check, and no database data was modified.

### Task 13 — verify fresh initialization

**Passed on October 8, 2026.** Before startup, `docker ps -a` showed no containers for `carbono-task13-20261008`, `docker volume ls --filter name=carbono-task13-20261008` returned no volumes, and port `8084` had no listener. The following command built the current API and created a separate local project:

```bash
API_PORT=8084 docker compose -p carbono-task13-20261008 --env-file .env up \
  -d --build --wait --wait-timeout 120
```

The Docker build succeeded; the API build step reported zero warnings/errors. Compose created network `carbono-task13-20261008_default` and volume `carbono-task13-20261008_fiap_carbono_mongodb_data`. Both API and MongoDB became healthy, and the command exited with code `0`. Application image: `web-fiap-carbono:local`, ID `sha256:59623e5f18e5cd07f8d7065b0a8d6f0782e9d7fd23c0708e50623e9266505937`.

The MongoDB log confirmed this order:

```text
running /docker-entrypoint-initdb.d/01-create-collections.js
running /docker-entrypoint-initdb.d/02-create-indexes.js
running /docker-entrypoint-initdb.d/03-seed.js
Phase 4 seed verification passed.
MongoDB init process complete; ready for start up.
```

The seed's historical message says “Phase 4”; it is the seed script's own verification, not completion of CI/CD Phase 4. It follows the script's reference, formula, activity-type, and GHG-scope assertions. The log also reported `formulaMismatchCount: 0`.

A temporary read-only `mongosh` verifier then checked the actual database and exited with code `0`. It required exactly the five domain collections, at least ten documents per collection, JSON Schema validators with `validationLevel: strict` and `validationAction: error`, and zero stored documents failing those validators. It compared all 12 domain index names, ordered keys, unique flags, and partial filters with the requirements in `02-create-indexes.js`, and checked each collection's `_id_` index. It also confirmed exactly one company with `codigo: "EMP-001"`.

| Collection | Documents | Domain indexes | Total indexes including `_id_` |
| --- | --- | --- | --- |
| `empresas` | 10 | 2 | 3 |
| `produtos` | 10 | 1 | 2 |
| `fornecedores` | 10 | 2 | 3 |
| `fatores_emissao` | 10 | 1 | 2 |
| `emissoes_carbono` | 15 | 6 | 7 |

Verified unique constraints include company/supplier CNPJ and code, product company/code, factor code/version, and emission code with the partial filter `{ codigo: { $type: "string" } }`. Emission query indexes cover product/date, company/date, supplier/date, factor reference, and stage category. All 55 stored documents satisfied their collection schemas.

For read-only inspection of this environment, use:

```bash
docker compose -p carbono-task13-20261008 ps
docker compose -p carbono-task13-20261008 logs --no-color mongodb
docker compose -p carbono-task13-20261008 exec -T mongodb \
  mongosh --quiet fiap_carbono --eval '
    printjson(db.getCollectionNames().sort().map(name => ({
      collection: name,
      count: db.getCollection(name).countDocuments({}),
      options: db.getCollectionInfos({ name })[0].options,
      indexes: db.getCollection(name).getIndexes()
    })));
  '
```

The local project remains running on API port `8084` with its volume retained. Reusing this name tests an existing volume, not another fresh initialization. Existing projects and their data were left unchanged. No source/configuration fixes were needed, demonstration scripts were not run, and the full solution test suite was not run for this task. Task 14's database-outage check and task 15's temporary-record persistence check are recorded below.

### Task 14 — database-backed API and bounded outage failure

**Passed on October 8, 2026, 18:55:09–18:56:52 UTC.** Verification used only the isolated local project `carbono-task13-20261008`, API `http://127.0.0.1:8084/api/empresas`, and its existing MongoDB volume. A temporary verifier included a `finally` block to start MongoDB again even if an outage assertion failed. It printed status, counts, and timings without exposing JWT configuration or complete company records.

Before the outage, a read-only MongoDB query retrieved the `_id` of `EMP-001`. The HTTP response was a JSON array of ten companies, and the returned company's `id` matched that MongoDB document. The verifier also executed the API container's actual configured health-check command, obtained from `.Config.Healthcheck.Test`, successfully.

It then ran:

```bash
docker compose -p carbono-task13-20261008 --env-file .env stop --timeout 10 mongodb
```

After confirming MongoDB was stopped, it repeated the HTTP probe with `curl --max-time 5` and executed the same container health-check command. The observations were:

| Check | Result | Observed elapsed time |
| --- | --- | --- |
| HTTP before outage | Curl exit `0`, HTTP `200`, JSON array with ten companies and matching `EMP-001` | 0.022 s |
| HTTP during outage | Curl exit `28` (timeout); no HTTP response received | 5.019 s |
| Actual health-check command during outage | Exit `1` | 5.113 s |
| Docker API health status | Became `unhealthy`, within the verifier's 120-second limit | 90.825 s after MongoDB stopped |
| HTTP after recovery | Curl exit `0`, HTTP `200`, ten companies and the same `EMP-001` ID | 0.014 s |

Curl's reported status `000` during the outage means no HTTP response was received; it is not an API status code. The bounded failure is enforced by the client's five-second timeout. A single failed check does not immediately mark the container unhealthy: the configured retry count and interval account for the longer Docker status transition. The retained Docker health log entries showed failed checks exiting `1` after approximately five seconds each.

The verifier restored MongoDB using:

```bash
docker compose -p carbono-task13-20261008 --env-file .env start mongodb
```

It waited for both services to become healthy again, revalidated the HTTP response, and reran the actual health-check command successfully. The MongoDB container ID and volume `carbono-task13-20261008_fiap_carbono_mongodb_data` were unchanged. Both services remain running on the original ports; only this project's MongoDB was stopped and started. No application records were written, no volumes were removed, and no seed or demonstration scripts were manually executed.

No application/configuration fixes, image builds, or full solution tests were needed for this task. These results verify local API/readiness failure and recovery; CI/CD stage blocking remains a later pipeline verification. Task 15's temporary-record persistence check is recorded below.

### Task 15 — edited record survives restart and recreation

**Passed on October 8, 2026, 18:58:54–18:59:26 UTC.** Verification used the existing isolated project `carbono-task13-20261008` on API port `8084`. Both services were healthy before the check. Read-only queries captured the five collection names, counts, and SHA-256 hashes of all documents sorted by `_id` and serialized as canonical Extended JSON.

A temporary verifier first confirmed its chosen company ID, code, and CNPJ did not already exist. It inserted one company with code `CRUD-TEMP-PERSISTENCE-TASK15`, then changed `nomeFantasia` from `CRUD-TEMP-PERSISTENCE-INITIAL` to `CRUD-TEMP-PERSISTENCE-EDITED` and updated `atualizadoEm`. The company satisfied the existing schema; no validators or indexes were changed. The API returned HTTP `200`, eleven companies, and the edited temporary company with the matching ID.

The verifier executed these lifecycle commands, with `API_PORT=8084` supplied throughout:

```bash
API_PORT=8084 docker compose -p carbono-task13-20261008 --env-file .env restart
API_PORT=8084 docker compose -p carbono-task13-20261008 --env-file .env up \
  -d --no-build --wait --wait-timeout 120
API_PORT=8084 docker compose -p carbono-task13-20261008 --env-file .env up \
  -d --no-build --force-recreate --wait --wait-timeout 120
```

All commands exited successfully. After each lifecycle operation, it verified:

- both services returned to healthy;
- volume `carbono-task13-20261008_fiap_carbono_mongodb_data` was retained;
- the temporary company's complete BSON document, including ID, creation/update timestamps, and edited value, matched the post-edit snapshot;
- counts and hashes for all five collections matched the post-edit snapshot;
- the API returned HTTP `200` with eleven companies and the edited record;
- initialization did not rerun: initialization-message counts were unchanged after restart, and the recreated MongoDB container's logs contained no initialization-script or seed-completion messages.

Restart retained both container IDs. Recreation replaced API container `78f6cdf9f840` with `f5e76b3ba884` and MongoDB container `0c66abcc6853` with `71576f439efe`, while retaining the same named volume.

Cleanup used `deleteOne` restricted to the exact generated `_id`, temporary code, and CNPJ. It deleted exactly one test record. A final comparison showed that all five collection hashes and counts matched the original pre-test database, and the API again returned ten companies without the temporary record:

| Collection | Before test | After edit/restart/recreation | After cleanup |
| --- | --- | --- | --- |
| `empresas` | 10 | 11 | 10 |
| `produtos` | 10 | 10 | 10 |
| `fornecedores` | 10 | 10 | 10 |
| `fatores_emissao` | 10 | 10 | 10 |
| `emissoes_carbono` | 15 | 15 | 15 |

Both services remain healthy on the original ports. The original 55 documents are unchanged, and no database or volume was dropped. Other local projects were not restarted or modified. No application/configuration fixes, image builds, or full solution tests were needed; this task verified the existing persistence configuration and recorded its results. Phase 3 host preparation was not started.

### Phase 2 review — October 8, 2026

The working tree was clean before this review. Source/configuration inspection and the recorded task 13–15 runtime results support completion of the local Compose phase; the earlier unchecked implementation items have now been reconciled. The deployment host, automated pipeline, and final submission package remain later-phase work.

The review found and corrected a readiness defect: `jq -e` returned success for an empty HTTP body and could accept a stream containing multiple JSON documents when the last result passed. The API health check now uses `jq -e -s` and requires exactly one parsed JSON value, which must be an array containing `EMP-001`. HTTP status and the five-second request timeout remain required. Twelve focused simulated response cases passed: valid seeded response, empty body, whitespace, multiple JSON documents, empty array, missing seed, HTML, object, null, redirect, server error, and curl timeout. Temporary response files were removed on every path.

The example JWT signing key is now empty, so copying `.env.example` without configuring a key fails Compose validation instead of accepting a public example key. Missing/empty-key rejection and successful validation with the existing local key were checked without printing that key. The local `.env` was not modified. README configuration and readiness guidance were updated accordingly.

Inspection also confirmed:

- `docker-compose.yml` is the sole root Compose configuration, with the root Dockerfile as the API build definition;
- both services use the same project-scoped default network; MongoDB has no fixed container name or host port, and uses a named volume;
- API port overrides `8081` and `8082` render correctly against internal port `8080`;
- MongoDB connection/database values match the internal service and seeded database; they are deliberately fixed in Compose, as documented in README;
- secret environment/development files are ignored and untracked, `.env.example` is tracked, and Docker context exclusion patterns cover those secret files and ZIPs;
- the configured local JWT key was absent from tracked non-archive files. Filename inspection of the historical tracked source ZIP found none of the four secret configuration filenames checked; this was not a complete credential audit or validation of the final submission archive. README now states the required ZIP exclusions and distinguishes that historical ZIP from the future CI/CD delivery.

Only the isolated API was recreated to apply the corrected health check:

```bash
API_PORT=8084 docker compose -p carbono-task13-20261008 --env-file .env up \
  -d --no-build --no-deps --force-recreate --wait --wait-timeout 120 api
```

It became healthy. The actual updated container health-check command exited successfully, and an HTTP request on port `8084` returned `200`, exactly one JSON array, ten companies, and `EMP-001`. MongoDB remained healthy and was not restarted or modified. Diff, shell syntax, and local documentation-link checks passed. No image build or full solution test suite was rerun: the changes affect Compose health validation and documentation, with fresh initialization, outage/recovery, and persistence evidence retained in tasks 13–15.

### Verification

Follow [Como executar localmente com Docker](../README.md#como-executar-localmente-com-docker) for `.env` setup, automatic initialization, health checks, restart/recreation, and recovery guidance. Task 12 replaced the obsolete instructions to connect to the Compose database through host port `27017` and manually run all five scripts. The README also explains that the current Compose file fixes MongoDB connection/database values rather than interpolating them from `.env`.

The commands below target `carbono-local` with `API_PORT=8081`. To reuse the already verified `carbono-init-check` database, keep that project name and `API_PORT=8083` instead. Never change the project name unintentionally when checking persistence.

```bash
docker compose -f docker-compose.yml -p carbono-local --env-file .env config --quiet
docker compose -f docker-compose.yml -p carbono-local --env-file .env up -d --build --wait --wait-timeout 120
docker compose -f docker-compose.yml -p carbono-local --env-file .env ps
curl --fail --silent --show-error --include --max-time 5 http://localhost:8081/api/empresas
docker compose -f docker-compose.yml -p carbono-local --env-file .env restart
docker compose -f docker-compose.yml -p carbono-local --env-file .env up -d --wait --wait-timeout 120
```

Expect HTTP `200` and a JSON array containing `codigo: "EMP-001"`. Use `docker compose ... exec mongodb mongosh` with the chosen authentication configuration to inspect the five collections, counts, and indexes. Confirm at least ten coherent seed documents in each collection on the fresh demonstration database. Compare counts and a deliberately edited temporary record after restart/recreation to prove data was not reset. Do not use `down -v` or a database drop. Avoid capturing fully rendered Compose configuration containing secrets.

Task 12 verification was limited to documentation diff/path checks, shell syntax checks, local Compose CLI help, and quiet Compose configuration validation. No builds, application tests, database scripts, restarts, or deployments were run for that documentation task. Runtime results from tasks 10 and 11 remain recorded separately above.

### Exit gate

- [x] A fresh local project starts API plus MongoDB successfully with documented configuration and readiness checks. Verified in task 13 on October 8, 2026.
- [x] Initialization creates the expected five-collection dataset, and later restarts/recreations preserve stored data without reseeding. Verified by tasks 13 and 15, including an edited temporary record and cleanup.
- [x] A database-backed API request succeeds; a database-unavailable check fails within a bounded time. Verified with recovery in task 14 on October 8, 2026.

## Phase 3 — October 8: prepare the host and verify isolation

**Purpose:** prepare the single Ubuntu AWS EC2 host and verify independent staging/production environments before automating deployment.

### Tasks

- [x] **1. Provision the EC2 host and confirm its status checks.** The user reported instance `i-08dab6645a2296e9a` launched in `us-east-2c` and all status checks passed on October 8, 2026.
- [x] **2. Connect over SSH and check host prerequisites.** The user confirmed successful SSH access and matching OS, architecture, CPU, memory, disk, and outbound registry-check results on October 8, 2026. Results and evidence limits are recorded below.
- [x] **3. Install/verify Docker Engine and the Compose plugin.** Establish a deployment user able to run the required commands and verify the SSH host key. Docker/Compose and `ubuntu` Docker access passed based on the supplied October 9 terminal output; the user confirmed the SSH fingerprint matched the AWS system log on the same date. See the task 3 verification record below.
- [x] **4. Prepare environment configuration.** Separate staging/production directories and untracked environment files are prepared. On October 9, the user supplied successful quiet Compose validation and file-permission output, and confirmed staging port `8081`, production port `8082`, and different JWT signing keys. See the task 4 verification record below. The current Compose configuration does not enable MongoDB authentication; no MongoDB credentials were added for this task.
- [x] **5. Start both environments.** The user's October 9 `ps` output shows healthy API/MongoDB pairs in `carbono-staging` on API host port `8081` and `carbono-production` on `8082`; both host-local `/api/empresas` requests returned HTTP `200` and displayed seeded company `EMP-001`. Both API containers report `web-fiap-carbono:local`. See the task 5 record below and task 7 for subsequent volume/network/data verification; Docker Hub publication/promotion remains later pipeline work.
- [x] **6. Verify network exposure.** On October 9, the user's local computer received HTTP `200` from both APIs at `18.222.63.49:8081` and `18.222.63.49:8082`. Both MongoDB containers reported no published host ports; the external TCP probe of port `27017` timed out with exit code `1`. The supplied inbound rules restrict TCP `22`, `8081`, and `8082` to the client's public IPv4 address with `/32`. See the task 6 verification record below.
- [x] **7. Verify isolation and record results.** The user's October 9 output confirms distinct MongoDB volumes and Compose networks, a temporary staging-only product absent from production, and successful cleanup. Actual URLs, project names, environment-file paths, and resource names are recorded in the architecture table and verification records. See task 7 below; Codex did not independently run the EC2 checks.

### Progress — October 8, 2026

Tasks 1 and 2 are complete based on the user's confirmations. The host is instance `i-08dab6645a2296e9a`, in region `us-east-2` (Ohio), availability zone `us-east-2c`; the user reported all EC2 status checks passed. SSH initially timed out. The user identified a mismatch between the current client IP and the SSH inbound-rule source, then confirmed a successful connection after receiving correction instructions.

The user subsequently confirmed that the following remote checks matched the expected results:

| Check | User-confirmed result |
| --- | --- |
| `cat /etc/os-release` | Ubuntu 24.04 LTS |
| `uname -m` | `x86_64` |
| `nproc` | 2 CPUs |
| `free -h` | Approximately 8 GiB RAM |
| `lsblk -o NAME,SIZE,TYPE,MOUNTPOINTS` and `df -h /` | 64 GiB disk; root free-space check matched expectations, with no exact free-space figure recorded |
| Docker Hub registry HTTPS check | HTTP `401`, the expected unauthenticated registry response |
| Microsoft Container Registry HTTPS check | HTTP `200` |

These are user-confirmed results, not commands independently executed by Codex. Raw terminal outputs and screenshots were not captured in the repository for these checks. Resource checks do not independently identify the EC2 instance type or EBS storage class. At this October 8 checkpoint, Docker installation, deployment-user setup, SSH host-key verification, API-port access, environment deployment, and isolation checks remained pending; Phase 3's exit gate was not complete.

### Task 3 verification — October 9, 2026

The user supplied terminal output for the host's Docker checks. The output confirms the runtime and deployment-account requirements:

| Command | Result in the supplied output |
| --- | --- |
| `whoami` | `ubuntu`, the selected deployment account |
| `id -nG` | Includes `docker`; Docker commands were run without `sudo` |
| `systemctl is-active docker` | `active` |
| `systemctl is-enabled docker` | `enabled`, confirming Docker is configured to start at boot |
| `docker version` | Client and Server: Docker Engine Community `29.9.0`, API `1.56`, `linux/amd64` |
| `docker compose version` | Docker Compose `v5.6.0` |
| `docker compose ls` | Returned the table header with no Compose projects listed |
| `docker run --rm hello-world` | Pulled the image from Docker Hub and printed `Hello from Docker!` |

The pulled **hello-world test image** reported digest `sha256:5e23090353324d887c48ad5e5c56d294eab81588df9605b07d1afe895f9cc8f8`. This is not the Carbon API application image or a staging/production release digest.

The runtime checks passed based on the user-supplied output; Codex did not execute commands on EC2. The subsequent October 9 AWS boot log supplied by the user did not contain the original SSH fingerprint block. After guidance to print the existing ED25519 public-key fingerprint to the serial console and retrieve the updated AWS system log, the user confirmed the SHA256 values matched. The fingerprint value itself was not supplied, so this comparison result is recorded as user-confirmed rather than independently compared by Codex.

Task 3 is complete based on the supplied Docker output and the user's host-key comparison confirmation. At that checkpoint, screenshots, environment configuration/deployment, and Phase 3 isolation/exit-gate checks were still pending. Subsequent task 4 progress is recorded below.

### Task 4 verification — October 9, 2026

The user supplied successful configuration-validation and file-permission output from EC2, then confirmed the intended API ports and different JWT signing keys. One repository checkout supplies the Compose definition at `/home/ubuntu/carbono/repository/docker-compose.yml`; the environment files are in separate directories outside that checkout:

| Check | Result supplied or confirmed by the user |
| --- | --- |
| Staging environment file | `/home/ubuntu/carbono/staging/staging.env`; permissions `600`, owner `ubuntu` |
| Production environment file | `/home/ubuntu/carbono/production/production.env`; permissions `600`, owner `ubuntu` |
| API port configuration | Staging `8081`; production `8082` |
| JWT signing keys | Different keys in the two environment files; values were not shared or recorded |
| `carbono-staging` with its environment file: `config --quiet` | Exit code `0` |
| `carbono-production` with its environment file: `config --quiet` | Exit code `0` |

These results are based on the user's output and confirmations, not commands independently run by Codex on EC2. Quiet validation confirms that Compose resolves each configuration; it does not start containers or prove readiness, actual port bindings, resource isolation, or database-backed responses. The MongoDB connection and database name remain fixed in the current Compose definition as `mongodb://mongodb:27017` and `fiap_carbono`; MongoDB authentication is not configured.

Task 4 is complete. At that checkpoint, the next bounded task was task 5: start both environments and verify API/MongoDB readiness. Tasks 5–7, screenshots, and both Phase 3 exit-gate checks were still pending. No host build, environment deployment, or pipeline execution was reported for task 4. Subsequent startup and readiness results are recorded below.

### Task 5 verification — October 9, 2026

The user supplied `docker compose ps` output for both projects using the shared Compose definition and their separate environment files. All four containers were running and healthy:

| Container | Image reported | Status | Host port mapping |
| --- | --- | --- | --- |
| `carbono-staging-api-1` | `web-fiap-carbono:local` | Running, healthy | `8081` to container `8080`, on IPv4 and IPv6 |
| `carbono-staging-mongodb-1` | `mongo:8.0.29-noble` | Running, healthy | None; `27017/tcp` is a container port only |
| `carbono-production-api-1` | `web-fiap-carbono:local` | Running, healthy | `8082` to container `8080`, on IPv4 and IPv6 |
| `carbono-production-mongodb-1` | `mongo:8.0.29-noble` | Running, healthy | None; `27017/tcp` is a container port only |

The user then supplied results of API requests run inside the EC2 SSH session:

| Request | Result shown | HTTP `Date` header |
| --- | --- | --- |
| `GET http://127.0.0.1:8081/api/empresas` | HTTP `200 OK`; displayed JSON company records include `EMP-001` | October 9, 2026, `18:09:11 GMT` |
| `GET http://127.0.0.1:8082/api/empresas` | HTTP `200 OK`; displayed JSON company records include `EMP-001` | October 9, 2026, `18:09:22 GMT` |

These results establish manual startup and host-local database-backed readiness based on the user's supplied output and completion confirmation. Codex did not execute builds, deployments, or requests on EC2. The pasted response bodies are truncated at the end; complete response files, build/startup/curl exit codes, screenshots, and application image IDs/digests were not supplied. The shared local image tag alone does not verify identical image contents or a published release digest. Preserve the later build-once and digest-promotion requirements.

The MongoDB status output shows no published host port. At the task 5 checkpoint, external API access and the AWS security-group rules still needed verification in task 6; subsequent results are recorded below. Container names and successful requests alone do not establish volume/network/data isolation; task 7 records that subsequent inspection. The matching seeded business records do not prove shared data or isolation.

Task 5 is complete. At that checkpoint, the next bounded task was task 6: verify external API access on ports `8081` and `8082` and confirm MongoDB is not publicly exposed. Task 7, screenshots, both Phase 3 exit-gate checks, and pipeline execution were still pending. Subsequent network-access results are recorded below.

### Task 6 verification — October 9, 2026

The user confirmed the EC2 public IPv4 address `18.222.63.49` and the matching client public IP. The supplied inbound-rule excerpt permits TCP `22` (SSH), `8081` (staging), and `8082` (production) from the client's public IPv4 address with `/32`; no MongoDB rule appears in that excerpt. The client address is the rule's source, and the EC2 address is the connection destination. The AWS account and security-group attachment were not independently inspected by Codex.

External checks ran on the user's local computer; Docker port checks ran inside the EC2 SSH session:

| Check | Result supplied by the user |
| --- | --- |
| `nc -vz -w 5 18.222.63.49 22` | TCP connection succeeded |
| `nc -vz -w 5 18.222.63.49 8081` | TCP connection succeeded |
| `nc -vz -w 5 18.222.63.49 8082` | TCP connection succeeded |
| Local-computer request to `http://18.222.63.49:8081/api/empresas` | `Staging: HTTP 200` |
| Local-computer request to `http://18.222.63.49:8082/api/empresas` | `Production: HTTP 200` |
| `docker port carbono-staging-mongodb-1` on EC2 | No output: no published port mappings |
| `docker port carbono-production-mongodb-1` on EC2 | No output: no published port mappings |
| `nc -vz -w 5 18.222.63.49 27017`, followed immediately by `echo $?`, on the local computer | Connection timed out; exit code `1` |

The external HTTP checks used `--noproxy '*'`, a ten-second timeout, and discarded the response bodies while printing the status. HTTP access from the authorized client is confirmed; these checks did not repeat the JSON/body validation already evidenced by the host-local requests and container readiness in task 5. The two empty Docker port results and the failed external TCP probe confirm the expected MongoDB exposure behavior for this configuration and tested client. A timeout alone would not establish that result.

Initial API attempts timed out. During troubleshooting, the user clarified that commands intended for the local computer had been run inside the SSH session. The later checks from the correct terminals passed. The exact cause of the earlier timeouts was not independently established; no proxy fault, firewall change, rebuild, or deployment result is inferred from the successful checks.

Task 6 is complete based on the user's supplied output and confirmations, not commands independently executed by Codex. At that checkpoint, tasks 1–6 satisfied the host/readiness/access exit-gate check; task 7, the isolation exit gate, overall Phase 3 completion, screenshots, and the CI/CD pipeline were still pending. Subsequent isolation results are recorded below. The API URLs above are dated observations; confirm the current EC2 public IPv4 address before later checks.

### Task 7 verification — October 9, 2026

The user supplied output from checks run inside the EC2 SSH session. Formatted container inspection identified the named volume mounted at each MongoDB container's `/data/db`; network inspection listed the containers attached to each environment's network:

| Check | Result in the supplied output |
| --- | --- |
| `carbono-staging-mongodb-1`: `/data/db` volume | `carbono-staging_fiap_carbono_mongodb_data` |
| `carbono-production-mongodb-1`: `/data/db` volume | `carbono-production_fiap_carbono_mongodb_data` |
| `carbono-staging_default`: attached containers | `carbono-staging-api-1`, `carbono-staging-mongodb-1` only |
| `carbono-production_default`: attached containers | `carbono-production-api-1`, `carbono-production-mongodb-1` only |

Data isolation was checked in each instance's existing `fiap_carbono.produtos` collection using the exact temporary business key `CRUD-TEMP-PHASE3-ISOLATION`. The staging insertion copied seeded product `PRO-001`, checked its company reference, assigned a new ObjectId and timestamps, removed the copied `legacyId`, and changed its code and name. The original seed product was not modified. No collection, validator, index, or authentication setting was changed by the supplied commands.

| Step | Result supplied by the user |
| --- | --- |
| Before insertion: count matching temporary products | Staging `0`; production `0` |
| Insert the temporary product into staging | `acknowledged: true`; `insertedId: ObjectId('6ac9422a4ff17a26b3fc2cd7')` |
| After insertion: count matching temporary products | Staging `1`; production `0` |
| Delete the temporary staging product by its ObjectId, code, and name | `acknowledged: true`; `deletedCount: 1` |
| After cleanup: count matching temporary products | Staging `0`; production `0` |

The separate volume mounts and network membership, together with the staging-only write, verify resource and data isolation for these environments. Matching initial seed counts alone would not prove isolation. Cleanup removed exactly the identified temporary product; retain both environments, their networks, persistent volumes, and environment files for later phases.

Task 7 and Phase 3 are complete based on the user's supplied terminal output and earlier prerequisite/configuration/readiness/access confirmations. Task 4 provides separate configuration and JWT-key confirmation, task 6 provides MongoDB exposure checks, and task 7 supplies the remaining volume/network/data evidence. Codex did not independently execute these EC2 commands. The supplied output is recorded here as text; no screenshots were added. This check did not recreate/redeploy containers, test persistence across deployment, or restart staging while checking production availability; those checks and screenshots remain Phase 6 tasks. Image publication, digest promotion, automated deployment, and failure blocking remain pending. The next bounded work is Phase 4: implement CI and publish the application image.

### Verification

For task 2, run the following inside the SSH session on the EC2 host. These are the checks whose results the user confirmed above:

```bash
cat /etc/os-release
uname -m
nproc
free -h
lsblk -o NAME,SIZE,TYPE,MOUNTPOINTS
df -h /

curl --silent --show-error --max-time 10 \
  --output /dev/null --write-out 'Docker Hub: HTTP %{http_code}\n' \
  https://registry-1.docker.io/v2/

curl --silent --show-error --max-time 10 \
  --output /dev/null --write-out 'Microsoft: HTTP %{http_code}\n' \
  https://mcr.microsoft.com/v2/
```

Expect Ubuntu 24.04 LTS, `x86_64`, 2 CPUs, approximately 8 GiB RAM, and a 64 GiB disk with available root space. Docker Hub's HTTP `401` confirms the unauthenticated registry is reachable; Microsoft's expected response is HTTP `200`. These connectivity checks do not prove authenticated image pulls or deployments succeed.

For task 3, run these commands on the host without `sudo`; successful results are summarized above:

```bash
whoami
id -nG
systemctl is-active docker
systemctl is-enabled docker
docker version
docker compose version
docker compose ls
docker run --rm hello-world
```

For SSH host-key verification, select the instance in the AWS console, then **Actions → Monitor and troubleshoot → Get system log**. Find `BEGIN SSH HOST KEY FINGERPRINTS` and compare the appropriate entry with the fingerprint presented by SSH. For an existing session, run `ssh-keygen -lf /etc/ssh/ssh_host_ed25519_key.pub` and compare the SHA256 value with the **ED25519** entry in the AWS system log. Record the actual match; accepting an SSH prompt or running the fingerprint command alone does not establish a verified comparison.

If the current boot log omits the original fingerprint block, the workaround used for this host is to print its existing public-key fingerprint to the serial console from the SSH session:

```bash
ssh-keygen -lf /etc/ssh/ssh_host_ed25519_key.pub \
  | sudo tee /dev/ttyS0
```

Retrieve the latest system log for the same instance through AWS and compare its new `SHA256:` value with the terminal output. The user confirmed this comparison passed on October 9. This prints the existing public-key fingerprint as verification evidence. AWS documents latest serial-output retrieval for Nitro instances in [Instance console output](https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/troubleshoot-unreachable-instance.html#instance-console-console-output).

For task 4, run these checks inside the EC2 SSH session. They use the shared checkout and the separate environment files at the paths confirmed above:

```bash
docker compose \
  -f /home/ubuntu/carbono/repository/docker-compose.yml \
  -p carbono-staging \
  --env-file /home/ubuntu/carbono/staging/staging.env \
  config --quiet
echo $?

docker compose \
  -f /home/ubuntu/carbono/repository/docker-compose.yml \
  -p carbono-production \
  --env-file /home/ubuntu/carbono/production/production.env \
  config --quiet
echo $?

stat -c '%a %U %n' \
  /home/ubuntu/carbono/staging/staging.env \
  /home/ubuntu/carbono/production/production.env
```

Expect no validation errors, exit code `0` for each Compose check, and `600 ubuntu` for both files. Confirm the intended ports and different JWT keys privately; do not print keys or the resolved Compose configuration into logs or screenshots.

For task 5, retain the same Compose file, project names, and environment-file options for `up -d --wait` and `ps`. If the local application image is absent, build it once before starting staging, then reuse it for production. Startup/readiness is now confirmed by the user-supplied output above; no build log or image-ID comparison was supplied. Do not rebuild merely to repeat a status check. To inspect the running environments from the EC2 SSH session:

```bash
docker compose \
  -f /home/ubuntu/carbono/repository/docker-compose.yml \
  -p carbono-staging \
  --env-file /home/ubuntu/carbono/staging/staging.env \
  ps

docker compose \
  -f /home/ubuntu/carbono/repository/docker-compose.yml \
  -p carbono-production \
  --env-file /home/ubuntu/carbono/production/production.env \
  ps

curl --fail --silent --show-error --include --max-time 5 \
  http://127.0.0.1:8081/api/empresas

curl --fail --silent --show-error --include --max-time 5 \
  http://127.0.0.1:8082/api/empresas
```

Expect all four containers to be healthy, the API mappings `8081:8080` and `8082:8080`, and both requests to return HTTP `200` with a JSON array containing `EMP-001`. These URLs are local to the EC2 host; success does not prove access from the user's local computer. Separate project names scope Compose networks and named volumes; task 7 records the actual mounts, network membership, and data-isolation results.

For task 6, confirm the current EC2 public IPv4 address in the AWS console and the source IP allowed by the instance's attached security groups. Keep SSH and the API rules limited to the intended client source; do not add a public MongoDB rule. Run the following on the **local computer, outside the EC2 SSH session**, substituting the current host address if it differs from the October 9 observation:

```bash
CARBONO_HOST=18.222.63.49

curl --noproxy '*' --fail --silent --show-error --max-time 10 \
  --output /dev/null --write-out 'Staging: HTTP %{http_code}\n' \
  "http://${CARBONO_HOST}:8081/api/empresas"

curl --noproxy '*' --fail --silent --show-error --max-time 10 \
  --output /dev/null --write-out 'Production: HTTP %{http_code}\n' \
  "http://${CARBONO_HOST}:8082/api/empresas"

nc -vz -w 5 "$CARBONO_HOST" 27017
echo $?
```

Expect `Staging: HTTP 200`, `Production: HTTP 200`, and a failed MongoDB TCP connection with a nonzero exit code. `echo $?` must immediately follow the probe to report its result. Run the Docker checks **inside the EC2 SSH session**:

```bash
docker port carbono-staging-mongodb-1
docker port carbono-production-mongodb-1
```

Expect no output from either command. Assess the external probe together with the published-port checks and the intended inbound rules; the TCP timeout alone does not prove MongoDB is private.

For task 7, run these read-only resource checks inside the **EC2 SSH session**; their supplied results are recorded above. Formatted inspection avoids printing container environment values:

```bash
docker inspect --type container \
  --format '{{.Name}}: {{range .Mounts}}{{if eq .Destination "/data/db"}}{{.Name}}{{end}}{{end}}' \
  carbono-staging-mongodb-1 \
  carbono-production-mongodb-1

docker network inspect \
  --format '{{.Name}}: {{range .Containers}}{{.Name}} {{end}}' \
  carbono-staging_default \
  carbono-production_default

for CARBONO_ENV in staging production; do
  printf '%s: ' "$CARBONO_ENV"
  docker exec "carbono-${CARBONO_ENV}-mongodb-1" \
    mongosh --quiet mongodb://localhost:27017/fiap_carbono \
    --eval 'print(db.produtos.countDocuments({codigo: "CRUD-TEMP-PHASE3-ISOLATION"}))'
done
```

Expect the two distinct volumes and each network's own API/MongoDB pair from the task 7 table. The count loop was used before insertion, after the staging-only insertion, and after cleanup; it now reports zero matching products in both environments because cleanup passed. Preserve the recorded intermediate `staging: 1` / `production: 0` evidence. Repeating a zero count alone cannot reproduce the data-isolation proof; a future mutation check must use an explicitly named temporary product and targeted cleanup.

### Exit gate

- [x] The host and access prerequisites are verified, and both environments serve database-backed responses at their documented addresses. Completed based on the user's prerequisite, readiness, and external-access output/confirmations in tasks 1–6 on October 8–9, 2026.
- [x] Volumes, networks, configuration, and data are independent; MongoDB is not publicly published. Completed based on the user's task 4 configuration confirmations, task 6 exposure checks, and task 7 resource/data/cleanup output on October 9, 2026.

## Phase 4 — October 9: implement CI and publish the image

**Purpose:** check each proposed change and produce one traceable application image for a successful `master` revision.

### Tasks

- [x] Add `.github/workflows/` checks for pull requests and pushes to `master`: checkout, install the .NET 8 SDK, restore, build, and execute the existing full test suite on a Docker-capable Ubuntu runner. Task 1 implementation is present in [ci.yml](../.github/workflows/ci.yml), with local verification recorded below. Successful `master` runs were independently inspected; the user confirmed successful PR checks on October 10, completing the remaining acceptance. See task 4's completion record for the evidence limits.
- [x] Supply synthetic test JWT settings and verify the current host-network/fixed-port fixture works without relying on developer files. Keep tests isolated from staging/production databases. Existing configuration passed on the GitHub Ubuntu runner: 107 tests passed with zero failures or skipped tests in the October 10 run inspected by Codex. See task 2 below; no implementation changes were needed.
- [x] Make failed restore/build/tests block publication; retain test reports and sanitized failure diagnostics. October 11 run `38145711002` verifies failed tests, retained summary/TRX, and skipped publication on `master`; recovery run `38145909543` succeeded. The user confirmed downloaded failure and successful PR TRX contents. See task 3's completion record for results and attribution.
- [x] Configure the Docker Hub repository and credentials in GitHub Secrets. Build the root Dockerfile with root context only after successful `master` checks. Both secret names were independently observed, and the October 10 run verifies CI followed by Docker Hub login and the image build. The user confirmed creating the intended Docker Hub repository and checking successful PR CI with the image job skipped. Task 5's successful publication subsequently verifies push permission for `gui2506/web-fiap-carbono`. See the completion records below.
- [x] Push an application image tagged with the full commit SHA; capture its registry digest as a job output for deployment. Do not publish from PRs. Run `38090160366` verifies successful publication and GitHub setting the digest output; independent Docker Hub inspection verifies the matching SHA tag/digest. The unchanged PR exclusion gate and earlier user-confirmed skipped image job are recorded below.
- [x] Verify `.dockerignore` excludes secrets before publication; record the image repository, SHA tag, digest, and corresponding workflow run without credentials. Ignore rules and image identifiers/run were already inspected. The user's October 11 EC2 output verifies published-image startup, a database-backed API response, and the expected API files with no matching migration/environment/development files under `/app`. Task 6's completion record below includes the actual output; screenshots remain Phase 6 evidence work.

### Task 1 implementation and local verification — October 9, 2026

The working tree was clean before this task. [`.github/workflows/ci.yml`](../.github/workflows/ci.yml) now defines a single `ci` job for pull requests and pushes to `master`. It runs directly on `ubuntu-24.04`, installs .NET `8.0.x`, checks SDK/Docker availability, restores the solution, builds Release, and executes the complete existing test suite. The job has a twenty-minute timeout and read-only repository permissions; checkout does not persist its credentials.

The existing test factory already supplies synthetic JWT configuration, and the MongoDB fixtures create disposable test containers. No application, fixture, Compose, SDK configuration, or deployment resource was changed for task 1. At this October 9 implementation checkpoint, GitHub runner compatibility and execution from its clean checkout still required real workflow evidence; task 2 was pending. Subsequent verification is recorded below.

Codex executed the following local checks with .NET SDK `8.0.422` and Docker Engine `29.8.2` (`linux/amd64`):

| Check | Actual local result |
| --- | --- |
| Workflow YAML parsing and structure checks | Passed: PR/`master` triggers, .NET 8 selection, read-only permissions, sequential full-solution commands, and no test filter |
| `bash -n` for each workflow command block | Passed; GitHub Actions itself was not executed and `actionlint` was not installed locally |
| `dotnet restore Web.Fiap.Carbono.sln` | Passed; all three projects restored, exit code `0` |
| `dotnet build Web.Fiap.Carbono.sln --configuration Release --no-restore` | Passed outside the sandbox; zero warnings/errors, exit code `0` |
| Release full-solution tests with `--no-build` and the TRX logger | Passed outside the sandbox: 107 passed, 0 failed, 0 skipped; exit code `0` |

The local test command used the same test options as the workflow, with a temporary results directory:

```bash
dotnet test Web.Fiap.Carbono.sln --configuration Release --no-build \
  --logger 'trx;LogFileName=ci.trx' \
  --results-directory /tmp/carbono-ci-task1-20261009-test-results
```

The local TRX is `/tmp/carbono-ci-task1-20261009-test-results/ci.trx`; it is temporary verification output, not a committed submission artifact. The workflow writes its report to `artifacts/test-results/ci/ci.trx` on the runner. Downloadable artifact retention remains task 3.

The sandbox denied Docker socket access, and the initial sandboxed MSBuild attempt failed without a compiler diagnostic. Authorized execution outside the sandbox confirmed Docker access and a successful build/test run; the initial failed attempts are not counted as passing checks.

Task 1's workflow implementation and local checks are complete. At the October 9 implementation checkpoint, its real-run acceptance and both Phase 4 exit gates were pending; Codex had not committed, pushed, opened a PR, triggered a GitHub run, published an image, or deployed an environment. Subsequent October 10 inspection verified successful `master` runs, and the user's later PR-check confirmation completes task 1, as recorded with task 4 below. Image publication and deployment belong to their later tasks/phases; the October 11 task 3 and task 6 records satisfy both Phase 4 exit gates.

### Task 2 verification — October 10, 2026

The user reported CI working correctly. Codex then independently inspected existing GitHub Actions runs through read-only `gh run list` and `gh run view --log` calls, and reviewed the workflow and test fixtures. The working tree was clean. No test, workflow, application, or deployment configuration changes were needed to satisfy task 2.

| Existing run inspected | Event / branch | Commit | Verified result |
| --- | --- | --- | --- |
| [October 10 run 38079294318](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38079294318), created `19:18:47 UTC` | `push` / `master` | `f0f42c75c466bcef85cf9ed7625d81e0c1baccb2` | Completed successfully; job log independently inspected |
| [October 9 run 38002363520](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38002363520), created `23:02:03 UTC` | `push` / `master` | `f645225e17edc3d38c44b5b58b6b8f649f894015` | Completed successfully according to run metadata; full log not inspected |

The October 10 job log records the following actual results:

| Check | Result in the inspected log |
| --- | --- |
| Runner image | `ubuntu-24.04`, image version `20261004.327.1` |
| Docker availability | Client and Server version `28.0.4` |
| Restore and Release build | Both steps completed successfully; build printed `Build succeeded.` |
| Full solution tests, with no test filter | 107 passed, 0 failed, 0 skipped; reported duration 25 seconds |
| TRX creation | `/home/runner/work/Web.Fiap.Carbono/Web.Fiap.Carbono/artifacts/test-results/ci/ci.trx` |

Source inspection confirms why these tests work without deployment settings:

- [CustomWebApplicationFactory.cs](../Web.Fiap.Carbono.Tests/Config/CustomWebApplicationFactory.cs) supplies synthetic JWT signing, issuer, audience, and expiration settings before the application entry point reads them. [MongoDbConfigurationTest.cs](../Web.Fiap.Carbono.Tests/Config/MongoDbConfigurationTest.cs) includes the signing/validation regression test; the unfiltered run executed all tests with none skipped.
- [MongoApiFixture.cs](../Web.Fiap.Carbono.Tests/Config/MongoApiFixture.cs) creates its own MongoDB container with Linux host networking, loopback port `27019`, and database `fiap_carbono_api_tests`; it overrides the API's connection settings with that container's address. The API test collection disables parallel execution.
- [MongoRepositoryFixture.cs](../Web.Fiap.Carbono.Tests/Data/MongoDb/MongoRepositoryFixture.cs) and [MigrationMongoFixture.cs](../Web.Fiap.Carbono.Tests/Migration/MigrationMongoFixture.cs) use disposable containers with dynamically allocated host ports and uniquely named test databases. Their connections do not target staging or production.
- The workflow checks out repository source onto the GitHub-hosted runner, selects .NET `8.0.x`, and supplies no deployment environment files or runtime secrets. `git ls-files` inspection found no tracked actual `.env`/`*.env` files or `appsettings.Development.json` files. The successful run therefore verifies the existing test configuration without ignored developer settings.

Task 2 is complete using the existing implementation and the independently inspected October 10 CI evidence. Codex inspected existing results rather than triggering a workflow or rerunning local tests for this verification. No screenshots or downloaded TRX artifacts were added. The logged TRX path proves report creation; downloadable artifact retention remains task 3.

At this inspection, the run listing returned two `push` runs on `master` and no PR run. Task 1's PR-trigger verification, the controlled failure/publication checks, and both Phase 4 exit gates remained pending. The next bounded task at that checkpoint was task 3: retain reports and sanitized diagnostics, and make failed CI block image publication. Subsequent implementation is recorded below. Docker Hub publication and deployments remain subsequent work.

### Task 3 implementation and local verification — October 10, 2026

The working tree already contained the task 2 documentation updates in `AGENTS.md` and this roadmap; those changes were preserved. [ci.yml](../.github/workflows/ci.yml) now gives the existing restore/build/test steps IDs and adds two retention steps. Their commands, triggers, .NET version, runner, permissions, and timeout are unchanged. The checks still return their normal failures; no `continue-on-error` or error suppression was added.

After successful or failed checks, `if: ${{ !cancelled() }}` allows the diagnostic summary and artifact upload to execute. Canceled runs skip these steps. The summary records only the commit SHA, run ID, and restore/build/test outcomes; it does not dump environment variables or configuration. Detailed restore/build errors remain in the GitHub job log, and generated TRX reports contain test failure details. A successful test step without a nonempty `artifacts/test-results/ci/ci.trx` fails the summary step; the summary is written before that check so it remains available for upload.

The original task 3 implementation used `actions/upload-artifact@v4`; before task 4, the workflow was updated to `@v7` to address the Node.js 20 deprecation warning. The current upload step is configured to retain a `ci-results` artifact for fourteen days, subject to repository retention limits. Its explicit upload paths are:

```text
artifacts/ci/summary.txt
artifacts/test-results/ci/*.trx
```

If restore/build fails or tests fail before creating a report, the artifact can contain only the summary. Successful upload does not clear an earlier check failure. Environment files, credentials, full Docker inspection output, and unrelated workspace files are not selected for upload. CI continues to use synthetic test configuration; do not introduce real deployment secrets into its reports.

Codex ran focused local verification rather than rerunning the unchanged application commands:

| Check | Actual local result |
| --- | --- |
| YAML parsing, workflow structure, and `bash -n` for every command block | Passed; original check commands/triggers/permissions were preserved. `actionlint` is not installed and was not run. |
| Eight shell scenarios executing the actual summary command with supplied stage outcomes | Passed: successful tests with a report; restore failure; build failure; test failure with/without a report; successful tests with a missing/empty report; skipped checks. |
| Missing/empty report after a supplied successful test outcome | Both returned exit code `1`, retaining the summary; expected failure of the guard. |
| Artifact-path selection and summary contents in those temporary workspaces | Passed: only the summary and matching TRX files were selected; an unrelated synthetic canary environment value and files were excluded. |

The shell scenarios used synthetic report files to check presence and upload selection. They are not application test results, GitHub condition evaluation, or proof of artifact upload. Temporary scenario workspaces were removed. No application build/test, workflow run, image publication, deployment, screenshot, commit, push, or PR creation was performed for this task.

**Acceptance pending at the October 10 checkpoint:** successful PR artifact contents, a retained controlled-failure report, and actual publication blocking still required verification. The planned exercise used a temporary PR, followed by removal of the failing change and a passing run before merging. The actual October 11 failure/recovery and downloaded-report confirmations are recorded below. Use a temporary PR for any future controlled-failure exercise; do not deliberately break `master` or either deployment environment.

At the task 3 implementation checkpoint, the workflow contained only the `ci` job. Task 4 below adds a gated image-build job; publication in task 5 must preserve `needs: ci`, with `if: ${{ success() && github.event_name == 'push' && github.ref == 'refs/heads/master' }}`. Keep failure-tolerant conditions confined to diagnostics/retention; do not apply them to publication. Verify the real dependency and absence of publication after failed checks when publication is added. Task 3 and both Phase 4 exit gates were unchecked at that checkpoint; subsequent task 3 acceptance is recorded below.

### Task 3 failure, recovery, and artifact verification — October 11, 2026

Task 3 is complete. Codex independently inspected the existing failure and recovery runs through read-only GitHub metadata and log checks. The user separately confirmed downloading the reports and checking their contents.

| Evidence | Verified result and source |
| --- | --- |
| Controlled failure run | [38145711002](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38145711002), created `2026-10-11 14:04:34 UTC`; event `push`, branch `master`, commit `6b3b6aa9ccd637a38efa385a52eee6c25b627138`; conclusion `failure`. Independently inspected by Codex |
| Restore and Release build | Both succeeded in the failed run |
| Full solution tests | 107 passed, 1 failed, 0 skipped; 108 total. The only failure was `Web.Fiap.Carbono.Tests.Failure.CiFailureVerificationTest.ControlledFailureProducesDiagnostics`, with `CI-FAILURE-VERIFICATION: expected temporary failure.`; test command exited `1` |
| Failure diagnostics and report retention | **Write CI diagnostic summary** and **Upload CI results** succeeded; the log records creation of `artifacts/test-results/ci/ci.trx` |
| Publication blocking | **Build and publish application image**, job `114488680378`, was `skipped` with no steps executed after failed CI |
| Temporary test removal and recovery | Removal commit `2fbd381667aae4fe73995c99a2519d2172d6f524`; [run 38145909543](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38145909543), created `2026-10-11 14:07:29 UTC`, event `push`, branch `master`, conclusion `success`. Restore, build, tests, summary, artifact upload, and image login/build/publication all succeeded |
| Downloaded failure `ci.trx` | The user confirmed the report contains the expected temporary failure |
| Downloaded successful PR `ci.trx` | The user confirmed zero failures; no PR/run identifier or screenshot was supplied |

The user also supplied this summary from the failed run:

```text
Commit: 6b3b6aa9ccd637a38efa385a52eee6c25b627138
Run: 38145711002
Restore: success
Build: success
Tests: failure
```

The controlled failure occurred on `master`, although the original exercise was planned for a temporary PR. Its skipped image job directly verifies that failed CI blocks publication on the publishing branch. The temporary test has been removed and the recovery run passed; no further failing change is required for this acceptance. Use a temporary PR for any future controlled-failure exercise.

Report download and content inspection are user-confirmed; Codex did not download the TRX files. Recovery test counts and the recovery image digest were not inspected, so the task 5 release identifiers below remain unchanged. Actual restore/build failures and canceled-run behavior were not exercised by these runs. No screenshots were added.

Together with the earlier PR checks and successful `master` runs, this evidence satisfies task 3 and the first Phase 4 exit gate. At this task 3 checkpoint, task 6 and the second exit gate were still pending; the subsequent task 6 record below completes them and Phase 4. Automated deployment and staging-to-production failure blocking remain later phases.

This documentation update preserves the existing user change to the test project. Codex did not modify implementation files, rerun builds/tests, trigger workflows, publish images, deploy, commit, push, or open a PR for this update.

### Task 4 implementation and local verification — October 10, 2026

The working tree was clean before this task. The user reported adding the Docker Hub credentials to GitHub Secrets. Codex independently ran `gh secret list --repo guidonunes/Web.Fiap.Carbono --json name,updatedAt` and observed `DOCKERHUB_USERNAME` and `DOCKERHUB_TOKEN`. Only metadata was read: this does not verify the values, token permissions, registry login, or Docker Hub repository existence. The intended image repository is `web-fiap-carbono` under the personal namespace supplied by `DOCKERHUB_USERNAME`; its creation/access remain to confirm.

[ci.yml](../.github/workflows/ci.yml) now adds an `image` job named **Build application image**. It requires `needs: ci` and successful CI on a `push` to `refs/heads/master`. PRs skip this job, and failed CI prevents it from running. It uses `ubuntu-24.04`, a twenty-minute timeout, and the existing read-only repository permissions. Checkout does not persist its credentials; `docker/login-action@v4` receives the two Docker Hub secrets. The existing `ci` job, including `actions/upload-artifact@v7`, was preserved unchanged.

The build step uses the root Dockerfile and repository-root context:

```bash
docker build \
  --file Dockerfile \
  --tag "${DOCKERHUB_USERNAME}/web-fiap-carbono:${GITHUB_SHA}" \
  .
```

`GITHUB_SHA` supplies the full commit SHA. Credentials are provided to registry login, not as Docker build arguments. The API Dockerfile, `.dockerignore`, application, MongoDB configuration, and deployment environments were not changed. At this task 4 checkpoint, the job built the API image without pushing it. Subsequent task 5 implementation below appends publication and digest capture to this same job so each delivery builds the image once; a separate runner would not share this job's Docker image storage.

Codex ran focused local verification:

| Check | Actual result |
| --- | --- |
| GitHub Secrets metadata | Both expected names are present; values and registry permissions were not accessed or verified. |
| YAML parsing and workflow structure | Passed: original CI job/triggers/permissions preserved; image job requires CI success and `master` push; correct secret references; no publication step. |
| `bash -n` for every workflow command block | Passed; `actionlint` is not installed and was not run. |
| Build-command dry runs with a temporary Docker substitute | Passed: one build invocation selects the root Dockerfile, root context, and full-SHA tag. Substitute exit codes `0` and `17` propagate unchanged. No real Docker command ran. |
| Docker source paths | Root Dockerfile and `src/Web.Fiap.Carbono/Web.Fiap.Carbono.csproj` exist; Dockerfile and `.dockerignore` were preserved byte-for-byte. |

The local filesystem had approximately `3.12 GiB` free. A real local image build was not attempted; the intended build verification is on GitHub's runner. Temporary command-verification files were removed. No application build/test, registry login, workflow trigger, image push, deployment, screenshot, commit, push, or PR creation was performed for task 4.

At the task 4 implementation checkpoint, real PR/`master` job execution and Docker Hub repository confirmation were pending. Subsequent `master` verification and user-confirmed repository/PR completion are recorded below. Both Phase 4 exit gates were unchecked at that checkpoint; the October 11 task 3 record satisfies the first gate.

### Task 4 GitHub verification — October 10, 2026

The user reported a successful CI run. With a clean working tree, Codex independently inspected existing [run 38085383384](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38085383384) through read-only `gh run view` metadata and log calls. The run was created at `20:51:29 UTC` on October 10 for a `push` to `master`, commit `0d59ad9c68fdd634d7093ba8047e4ab67a7299c9`, and completed successfully.

| Check | Result in the inspected run |
| --- | --- |
| Restore and Release solution build | Both succeeded; build reported zero warnings and errors |
| Full solution tests | 107 passed, 0 failed, 0 skipped; reported duration 23 seconds |
| Diagnostic summary and TRX | Summary step succeeded; log records creation of `artifacts/test-results/ci/ci.trx` |
| Artifact upload | `ci-results` successfully uploaded, 40,168 bytes, [artifact 11682366173](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38085383384/artifacts/11682366173); contents were not downloaded or inspected |
| Job order | CI completed at `20:52:34 UTC`; the image job started at `20:52:37 UTC` and completed at `20:53:09 UTC` |
| Docker Hub authentication | Login step succeeded; log reports `Login Succeeded!`; secret values were not read |
| Root Dockerfile image build | Succeeded; log records the tag `docker.io/***/web-fiap-carbono:0d59ad9c68fdd634d7093ba8047e4ab67a7299c9`, with the namespace masked by GitHub |
| Runner-local image ID | `sha256:2647cab8ac7bb39ddc9ac45e1b14b532ac2eb963085ae78730accb82b64e96fb`; this is not a published registry digest |

This verifies task 4's successful `master` CI, authenticated Docker Hub login, and application image build, plus task 3's successful artifact upload. The workflow revision executed by this run had no image-push or deployment step, so neither Docker Hub publication nor an environment update is established by this run. Login and a local image tag do not prove the intended Docker Hub repository exists or the token can push to it.

After this run inspection, the user confirmed on October 10 that the intended Docker Hub repository had already been created and reported successful CI in the PR context. At that checkpoint, Codex's read-only run listing for `guidonunes/Web.Fiap.Carbono` returned only successful `push` runs on `master`; a listing filtered by `--event pull_request` and `gh pr list --state all` both returned no entries. An overall successful run alone did not establish whether the image job was skipped, so the PR job-status check was explained to the user. Subsequent confirmation is recorded below.

Codex inspected the existing run; no local build/test, workflow trigger, image push, deployment, screenshot capture, commit, push, or PR creation was performed for this documentation update. No application or workflow files were changed.

### Tasks 1 and 4 completion confirmation — October 10, 2026

After receiving instructions to inspect the PR run's job statuses, the user confirmed that all checks were complete: **Restore, build and test** succeeded on the PR, and **Build application image** was skipped. Together with the earlier user confirmation that the intended Docker Hub repository exists and the independently inspected successful `master` CI/login/build run, this completes tasks 1 and 4.

The PR result and Docker Hub repository creation are user-confirmed evidence. Codex's read-only PR-run listing during this documentation update still returned no entries. No PR/run URL, PR commit, PR test counts, or screenshot was supplied; do not invent these details or describe the PR checks as independently inspected. Preserve the earlier CLI observations and collect the actual PR/run link and real screenshots for the later evidence tasks.

At the tasks 1 and 4 completion checkpoint, task 3's artifact-content downloads, failing-run report retention, and controlled failure-blocking checks remained pending, as did both Phase 4 exit gates. The next implementation task was task 5: append SHA-tagged publication and registry-digest capture to the existing `image` job, preserving its gate and building once. Subsequent implementation is recorded below. Actual publication will verify push permission for the user-confirmed repository. Task 6's published-image verification and automated deployment remain later work.

The existing uncommitted updates to `AGENTS.md` and this roadmap were preserved. This completion record changes documentation only; no build, test, workflow trigger, image publication, deployment, commit, push, or PR creation was performed.

### Task 5 implementation and local verification — October 10, 2026

The working tree already contained the task 4 documentation updates in `AGENTS.md` and this roadmap; those changes were preserved. [ci.yml](../.github/workflows/ci.yml) now adds a **Publish application image** step to the existing `image` job, which is named **Build and publish application image**. Its `needs: ci`, successful `master` push condition, Ubuntu runner, timeout, permissions, original CI job, and login/build steps are preserved. There is still one application image build per delivery run.

The `publish` step uploads `${DOCKERHUB_USERNAME}/web-fiap-carbono:${GITHUB_SHA}` using the existing registry login. It captures push output in the runner's temporary directory, extracts a registry digest, and requires exactly one `sha256:` value with 64 lowercase hexadecimal characters. Missing, malformed, or multiple matching digests fail the step. `set -euo pipefail` preserves failures from `docker push` or `tee`; no digest output or success message is written after those failures. The Docker token is not supplied to the shell step or as a build argument. [Docker push documentation](https://docs.docker.com/reference/cli/docker/image/push/)

The step writes `digest=sha256:...` to `$GITHUB_OUTPUT`; `jobs.image.outputs.digest` maps `${{ steps.publish.outputs.digest }}`. Later dependent jobs can use `${{ needs.image.outputs.digest }}`. Only the digest is exported, and the final log message records the commit SHA and registry digest. Do not substitute the runner-local image ID recorded for task 4 or rebuild for production. [GitHub job-output documentation](https://docs.github.com/en/actions/how-tos/write-workflows/choose-what-workflows-do/pass-job-outputs)

Codex ran focused local verification:

| Check | Actual local result |
| --- | --- |
| Workflow YAML parsing and structure | Passed: original CI job, triggers, permissions, CI/`master` gate, and login/build steps preserved; publication is in the same job; the digest output maps to the `publish` step |
| `bash -n` for every workflow command block | Passed; `actionlint` is not installed and was not run |
| Nine scenarios executing the actual publish command with a temporary Docker substitute | Passed: successful push, existing layers, push failure despite digest text, missing digest, short digest, wrong algorithm, non-hex digest, multiple digest lines, and log-write failure |
| Failure behavior and outputs in those scenarios | Substitute push failure preserved exit code `17`; digest/log failures returned `1`; successful cases returned `0` and emitted the expected digest; failures emitted no digest or success message |
| Publication arguments and credential canary | Exactly one push of the expected synthetic full-SHA tag; no build invocation; a synthetic token value was absent from captured logs and outputs |
| Dockerfile and `.dockerignore` preservation | Both unchanged byte-for-byte; inspection confirms existing environment/development-settings exclusions. Actual image-content verification remains task 6 |

The scenario workspaces were removed. These checks used synthetic command output and a Docker substitute; they are not real Docker Hub publication, application tests, or GitHub condition/output evaluation. No actual image build/push, registry login, workflow trigger, deployment, screenshot, commit, push, or PR creation was performed by Codex at this implementation checkpoint. Subsequent read-only publication and registry verification is recorded below.

At this implementation checkpoint, task 5 and both Phase 4 exit gates remained unchecked pending actual publication, the matching Docker Hub SHA tag/digest, and digest-output confirmation. Subsequent task 5 acceptance is recorded below. Task 3's controlled failure/artifact checks and task 6's published-image runtime/content checks were pending at that checkpoint; task 3 acceptance is recorded above. Automated deployment remains Phase 5.

### Task 5 publication and registry verification — October 10, 2026

The user reported that the workflow worked as intended, supplied Docker Hub repository `gui2506/web-fiap-carbono`, and confirmed their registry inspection returned the expected digest. Codex independently inspected the existing GitHub run/job metadata and image-job log, then queried Docker Hub's registry metadata without pulling or starting the image.

| Evidence | Observed result |
| --- | --- |
| Workflow run | [38090160366](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38090160366), created `2026-10-10 22:06:25 UTC`; event `push`, branch `master`, conclusion `success` |
| Commit / full SHA tag | `6458819e04eecf1d35a13648a509210f90bf8fc1` |
| CI job | **Restore, build and test** succeeded from `22:06:28` to `22:07:28 UTC`; restore, build, tests, diagnostic summary, and artifact upload steps all reported success. Test counts and artifact contents were not inspected for this run |
| Image job | [Build and publish application image](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38090160366/job/114324975076) succeeded from `22:07:31` to `22:08:12 UTC`, after CI; checkout, Docker Hub login, build, and publish steps succeeded |
| Docker Hub repository / published tag | [gui2506/web-fiap-carbono](https://hub.docker.com/r/gui2506/web-fiap-carbono/tags), tag `6458819e04eecf1d35a13648a509210f90bf8fc1` |
| Registry digest | `sha256:78030c6e4e85e5fce8ce0be4766062e7f36c028e0442950e744795abeb5f3e13` in both the push log and independent registry inspection |
| GitHub job output | The log records `Evaluate and set job outputs` and `Set output 'digest'`; no skipped/redacted digest-output warning was observed. The workflow exposes `jobs.image.outputs.digest`; no deployment job consumes it yet |
| Independent registry command | `docker buildx imagetools inspect gui2506/web-fiap-carbono:6458819e04eecf1d35a13648a509210f90bf8fc1` returned exit code `0` and the matching top-level `Digest:` |

The registry inspection returned:

```text
Name:      docker.io/gui2506/web-fiap-carbono:6458819e04eecf1d35a13648a509210f90bf8fc1
MediaType: application/vnd.docker.distribution.manifest.v2+json
Digest:    sha256:78030c6e4e85e5fce8ce0be4766062e7f36c028e0442950e744795abeb5f3e13
```

The full commit SHA is the image's tag; the registry digest identifies its published content. Future deployment must use this exact reference for this release:

```text
gui2506/web-fiap-carbono@sha256:78030c6e4e85e5fce8ce0be4766062e7f36c028e0442950e744795abeb5f3e13
```

Task 5 is complete based on successful real publication, the digest job output, the independent SHA-tag/digest match, and the unchanged PR exclusion gate together with the earlier user-confirmed skipped image job. No new PR run was independently verified; retain the earlier attribution and collect its actual link/screenshots later. A registry metadata match does not verify startup, image contents, absence of secrets in image layers, staging/production use of this digest, or failure blocking. At this October 10 checkpoint, task 6 and task 3's remaining checks kept both Phase 4 exit gates pending. The October 11 task 3 record satisfies the first gate; subsequent task 6 verification below satisfies the second gate and completes Phase 4. No image was pulled/run, no workflow was triggered, and no deployment, screenshot capture, commit, push, or PR creation was performed by Codex for this verification or documentation update.

### Task 6 published-image verification and Phase 4 completion — October 11, 2026

**Task 6 and Phase 4 are complete.** The user supplied the following EC2 execution results for the published release recorded in task 5: repository `gui2506/web-fiap-carbono`, SHA tag `6458819e04eecf1d35a13648a509210f90bf8fc1`, digest `sha256:78030c6e4e85e5fce8ce0be4766062e7f36c028e0442950e744795abeb5f3e13`. These host checks are user-supplied evidence; Codex did not independently run them on EC2.

| Check | User-supplied result |
| --- | --- |
| Published-image startup | `docker start carbono-image-check` returned the container name |
| Database-backed API request | `http://127.0.0.1:8083/api/empresas` returned HTTP `200`; response saved to `/tmp/carbono-image-check-empresas.json` |
| JSON response and seeded data | `jq -e -s 'length == 1 and (.[0] \| type == "array" and any(.[]; .codigo == "EMP-001"))'` returned `true` |
| Required application files | Nonempty `/app/Web.Fiap.Carbono.dll`, `Web.Fiap.Carbono.deps.json`, and `Web.Fiap.Carbono.runtimeconfig.json` |
| Excluded application files | No matches under `/app` for `Web.Fiap.Carbono.Migration*`, `.env`, `.env.*`, `*.env`, or case-insensitive `appsettings.Development*.json` |
| Image-content command | Temporary container with `--rm --network none --entrypoint sh` printed `Application file checks passed`; `echo $?` printed `0` |

The image-content command uses the previously selected digest and reads packaged files without starting the API or supplying environment files:

```bash
CARBONO_IMAGE='gui2506/web-fiap-carbono@sha256:78030c6e4e85e5fce8ce0be4766062e7f36c028e0442950e744795abeb5f3e13'

docker run --rm --network none \
  --entrypoint sh "$CARBONO_IMAGE" -eu -c '
  for file in Web.Fiap.Carbono.dll \
              Web.Fiap.Carbono.deps.json \
              Web.Fiap.Carbono.runtimeconfig.json; do
    if [ ! -s "/app/$file" ]; then
      echo "Missing API file: $file"
      exit 1
    fi
  done

  unexpected=$(find /app \( \
    -name "Web.Fiap.Carbono.Migration*" -o \
    -name ".env" -o \
    -name ".env.*" -o \
    -name "*.env" -o \
    -iname "appsettings.Development*.json" \
  \) -print -quit)

  if [ -n "$unexpected" ]; then
    echo "Unexpected application file: $unexpected"
    exit 1
  fi

  echo "Application file checks passed"
'
echo $?
```

The user supplied this actual result:

```text
Application file checks passed
0
```

The scan verifies the named files and exclusions in the final `/app` filesystem. It does not inspect all historical image layers or prove the absence of every possible embedded secret. No configuration values were printed. Keep the inspected SHA/digest associated with this release; the recovery run in task 3 published another revision whose digest was not inspected here.

Together with tasks 1–5, the startup, database-backed response, and file checks satisfy both Phase 4 exit gates. Actual screenshots remain to collect in Phase 6. This manual image check does not establish automated staging/production deployment, production promotion, deployment failure blocking, concurrency, or persistence after redeployment.

At this Phase 4 completion checkpoint, the next bounded work was Phase 5, beginning with deployment/smoke scripts. Its tasks and exit gates were unchecked; no Phase 5 implementation was included in that documentation update. Subsequent Phase 5 task 1 completion is recorded below. Existing local changes were preserved. Codex did not rerun application builds/tests, execute Docker/database/deployment commands, trigger workflows, provision resources, capture screenshots, commit, push, or open a PR for this update.

### Verification

Inspect an actual PR workflow run for executed restore/build/tests and absence of publish/deploy steps. After an explicitly authorized push/merge to `master`, inspect the successful check/publish run and Docker Hub tag. Pull `DOCKERHUB_NAMESPACE/IMAGE@sha256:DIGEST` with the actual values and inspect its repo digest; verify the image starts with runtime configuration and contains the API rather than the migration tool. Confirm a controlled test failure produces failed CI, retained diagnostics, and no image publication. Task 3 records the actual `master` failure/recovery and user-confirmed PR reports; use a temporary PR for any future controlled-failure exercise.

### Exit gate

- [x] Real PR and `master` runs execute the existing tests; failures block subsequent work. Successful PR checks and downloaded zero-failure reports are user-confirmed; independently inspected October 11 `master` failure/recovery runs verify retained diagnostics and blocked image publication after failed tests.
- [x] A successful `master` run publishes a SHA-tagged image with a recorded usable digest; PRs cannot publish or deploy. Task 5 verifies publication and the SHA-tag/digest match; the user's October 11 task 6 output verifies startup, a database-backed HTTP `200` response, and application contents. PR exclusion retains the inspected gate and user-confirmed skipped image job; no deployment jobs exist yet.

## Phase 5 — October 10: automate staging and production deployment

**Purpose:** deliver the tested image automatically through staging to production.

### Tasks

- [x] Add small deployment/smoke scripts under root `scripts/` with explicit project, environment-file, image-digest, and API URL inputs. Reject missing inputs and fail on command errors. The user implemented `scripts/deploy.sh`, `scripts/smoke.sh`, and Compose `APP_IMAGE` selection; local syntax/missing-argument checks passed, and the user's October 11 manual EC2 staging deployment returned healthy containers, a passing database-backed smoke check, and exit `0`. See the task 1 record below.
- [x] Deploy the matching Compose/scripts revision on the EC2 deployment runner. Set `APP_IMAGE` to the published digest, pull it, and start/update staging with its existing persistent database. The user confirmed all workflow jobs ran successfully after adding the runner label; the execution result is user-reported, with run/digest evidence still to collect below.
- [x] Wait with bounded retries for readiness, then validate HTTP status, JSON shape, and a known database record through the API. Return nonzero for timeout or invalid output. The existing smoke implementation passed thirteen real loopback HTTP scenarios on October 11, including timeout and invalid-response rejection; the user confirmed successful staging workflow execution. See task 3 below for scope and results.
- [x] Deploy production only after staging and its smoke checks succeed. Use the same digest and production's own configuration/resources, then run equivalent production checks. On October 11, the user reported all workflow jobs green and supplied identical staging/production image references with digest `sha256:4deaaaa81939cf9cc27ac140cc993631139b576d281ad991452170e92a1e4da7`. See task 4's acceptance record and evidence limits below.
- [x] Enforce job dependencies and shared concurrency across the complete deployment sequence; do not use `continue-on-error` or unconditional production deployment. Workflow concurrency is configured in task 2 and production dependencies in task 4. Task 5 records user-supplied failure blocking, unchanged/available production, successful recovery, and A/B serialization: B waited for A and started after A finished. Run links/raw timing evidence remain to collect; Codex did not independently inspect the A/B runs.
- [x] Keep registry/runner/SSH/runtime credentials out of scripts, logs, command tracing, and image layers. Source inspection and the user's protected-file, packaged-key, and log-review confirmations satisfy task 6's checks. Docker Hub is user-confirmed public, so host pull credentials are unnecessary. See the task 6 record for verification scope.
- [x] Record commit, digest, environment, and result. Document a simple manual recovery using the previous known digest without deleting MongoDB storage; automatic rollback is not required. [docs/ci-cd-deployment.md](ci-cd-deployment.md) records successful run `38184996037`, its full commit/digest, both environments' results, and a reviewed manual recovery procedure. The recovery procedure has not been executed; the separate staging-recreation verification below now satisfies storage-preservation acceptance.

### Task 1 scripts and manual staging verification — October 11, 2026

**Task 1 is complete.** The user created [scripts/deploy.sh](../scripts/deploy.sh) and [scripts/smoke.sh](../scripts/smoke.sh), and changed the API's [Compose image](../docker-compose.yml) to `"${APP_IMAGE:-web-fiap-carbono:local}"`. At documentation inspection, Compose was modified and `scripts/` was untracked; Codex preserved those user changes.

Source inspection confirms the deployment script accepts the Compose project, environment file, full Docker Hub image digest, and host-local API URL. It rejects mismatched environments/URLs, unreadable environment files, invalid digests, and missing tools/scripts; it validates Compose without printing runtime configuration and confirms the selected image before pulling. It uses `--no-build`, a 120-second Compose health wait, and the existing project/volume configuration. Normal command failures return nonzero.

The smoke script uses up to ten requests, with a two-second connection timeout, five-second request timeout, and two-second delays between failed attempts. Success requires exactly HTTP `200`, a single JSON array, and company `EMP-001`; redirects, HTML, missing seeded data, and connection failures cannot satisfy that condition. The temporary response file is removed on exit, and response bodies/runtime secrets are not printed.

Codex ran only these local script checks:

| Local check | Observed result |
| --- | --- |
| `bash -n scripts/deploy.sh` | Exit `0`; no syntax errors |
| `bash -n scripts/smoke.sh` | Exit `0`; no syntax errors |
| `bash scripts/deploy.sh` without arguments | Printed usage; exit `1` as expected, before Docker commands |
| `bash scripts/smoke.sh` without arguments | Printed usage; exit `1` as expected, before API requests |

Create/edit these files in the local repository, then copy the scripts and updated Compose file to the EC2 checkout. Execute deployment commands inside EC2's SSH session from `/home/ubuntu/carbono/repository`; the relative path `scripts/deploy.sh` does not resolve from `/home/ubuntu`. Host environment files remain outside the checkout.

The user supplied successful manual EC2 execution for:

| Deployment setting/result | User-supplied evidence |
| --- | --- |
| Compose project | `carbono-staging` |
| Environment file | `/home/ubuntu/carbono/staging/staging.env` |
| Application release | `gui2506/web-fiap-carbono`; digest `sha256:78030c6e4e85e5fce8ce0be4766062e7f36c028e0442950e744795abeb5f3e13`, previously matched to SHA tag `6458819e04eecf1d35a13648a509210f90bf8fc1` in Phase 4 |
| Image pull | Selected digest pulled successfully |
| Compose health | `carbono-staging-mongodb-1` and `carbono-staging-api-1` both reported `Healthy` |
| API URL / smoke result | `http://127.0.0.1:8081/api/empresas`; `Smoke check passed: HTTP 200 and EMP-001` |
| Deployment result | `Deployment verified: carbono-staging`; `echo $?` returned `0` |

Reproduce the manual check on EC2 with:

```bash
cd /home/ubuntu/carbono/repository

CARBONO_IMAGE='gui2506/web-fiap-carbono@sha256:78030c6e4e85e5fce8ce0be4766062e7f36c028e0442950e744795abeb5f3e13'

bash scripts/deploy.sh \
  carbono-staging \
  /home/ubuntu/carbono/staging/staging.env \
  "$CARBONO_IMAGE" \
  http://127.0.0.1:8081/api/empresas

echo $?
```

The user's output ended with:

```text
Smoke check passed: HTTP 200 and EMP-001
Deployment verified: carbono-staging
0
```

The EC2 results are user-supplied; Codex did not run the deployment or independently access the host. This manual execution is not a GitHub Actions deployment run, production promotion, or proof of persistence after redeployment. The script implements readiness/smoke logic, but its pipeline integration and controlled timeout/failure checks remain later work. No deployment-script commit SHA, new pipeline run, or screenshot is claimed.

At the task 1 documentation checkpoint, tasks 2–7 and both Phase 5 exit gates remained pending, and the next planned task was deployment over SSH from the workflow. Codex changed only `AGENTS.md` and this roadmap for that update; no implementation edits, application builds/tests, Docker/database commands, deployments, resource provisioning, commits, pushes, or PR creation were performed. The subsequent runner decision and task 2 implementation below replace the automated SSH access plan while preserving manual SSH administration.

### Task 2 EC2 runner and staging workflow — October 11, 2026

**Task 2 workflow execution is complete based on the user's subsequent successful-run confirmation.** At implementation, real-run acceptance was pending. The user reported that the attempted AWS advanced account/identity setup required a paid-plan upgrade. To stay within the existing Free Plan, the user selected and authorized a self-hosted GitHub Actions deployment runner on the existing EC2 VM. This replaces GitHub-hosted deployment over SSH; no AWS OIDC role, paid-plan upgrade, additional host, or dynamic security-group rule is required by this job. The user confirmed that `guidonunes/Web.Fiap.Carbono` is private.

The user supplied successful registration output for runner **`carbono-ec2`**, with the default `self-hosted`, `Linux`, and `X64` labels and `_work` folder. The user initially skipped the custom label and later found `carbono-deploy` missing while the deployment job was queued. After adding the label and following the runner-service diagnostics, the user confirmed all jobs ran and were green. The prescribed service account/path remain `ubuntu` and `/home/ubuntu/actions-runner`; no service-status output was supplied, and Codex did not inspect EC2 or independently verify runner availability. GitHub documents [runner selection by labels](https://docs.github.com/en/actions/how-tos/manage-runners/self-hosted-runners/use-in-a-workflow) and the runner's [outbound HTTPS connection](https://docs.github.com/en/actions/reference/runners/self-hosted-runners).

[ci.yml](../.github/workflows/ci.yml) now adds:

| Workflow setting | Implementation |
| --- | --- |
| Staging job | `deploy_staging`, displayed as **Deploy and verify staging** |
| Gate | Requires successful `image`; only a `push` to `refs/heads/master` |
| Runner | `[self-hosted, linux, x64, carbono-deploy]` |
| Checkout | Exact `${{ github.sha }}`; `persist-credentials: false` |
| Release input | `${{ needs.image.outputs.digest }}` through `CARBONO_DIGEST`; full `gui2506/web-fiap-carbono@sha256:...` reference |
| Deployment | Existing `scripts/deploy.sh`; project `carbono-staging`, `/home/ubuntu/carbono/staging/staging.env`, `http://127.0.0.1:8081/api/empresas` |
| Bounds and failure handling | Ten-minute job timeout; existing Compose/smoke bounds and nonzero failure propagation |
| Workflow concurrency | `carbono-release-${{ github.ref }}`, `cancel-in-progress: false` |

CI and image-publication jobs are unchanged and remain on GitHub-hosted `ubuntu-24.04` runners. Deployment uses the scripts/Compose from the matching runner checkout, so it does not depend on a manual pull into `/home/ubuntu/carbono/repository`. The explicit Compose project retains staging's existing named volume/network even though the checkout path changes. The database and deployment scripts are unchanged; no build, storage removal, or reseeding command is added. Keep environment files outside the runner workspace so checkout cleanup cannot remove them.

All `master` runs share one workflow concurrency group covering checks, publication, staging, and production. PRs have separate groups by ref. A running release is not canceled to start a newer one; with GitHub's default queue, a newer pending run can replace an older pending run. This is not a guarantee that every queued commit deploys. See [GitHub's concurrency rules](https://docs.github.com/en/actions/how-tos/write-workflows/choose-when-workflows-run/control-workflow-concurrency). At the task 2 checkpoint, production gating was pending; task 4 below adds it within this same serialized workflow. Live overlap and controlled pipeline-failure verification remain pending.

Codex verified locally:

| Check | Observed result |
| --- | --- |
| YAML parse and workflow structure | Passed; prior CI/publication jobs and triggers unchanged; staging gate, runner labels, exact checkout SHA, digest input, and workflow concurrency checked |
| `bash -n` on the staging step and both existing scripts | Exit `0`; no syntax errors |
| Eight synthetic integration scenarios | Passed: success, empty/malformed digest, missing environment file, failed pull, failed health wait, HTTP failure, and missing seeded company |

The synthetic checks used the actual staging shell block and existing scripts in an isolated temporary checkout, with a synthetic environment file, mock Docker/curl/sleep commands, and real `jq`. They verified argument forwarding, `--no-build`/bounded wait, invalid-input rejection before Docker, failure propagation, and absence of a deployment-success message on errors. They made no network requests or live container changes. These are local checks, not GitHub scheduling, application tests, or EC2 deployment evidence.

Retain the following verification steps to capture task 2's actual run and digest evidence:

1. Confirm **Restore, build and test** and **Build and publish application image** succeed before **Deploy and verify staging** starts on `carbono-ec2`.
2. Match the run's commit and published digest to the staging job log. Expect `Smoke check passed: HTTP 200 and EMP-001` and `Deployment verified: carbono-staging`.
3. Inside EC2, run `docker inspect --type container --format '{{.Config.Image}}' carbono-staging-api-1`; expect `gui2506/web-fiap-carbono@` followed by that run's exact registry digest.
4. Record the actual run link, commit, digest, and result. Confirm a PR skips image/staging jobs; a failed prerequisite must prevent staging from running.

At the task 2 implementation checkpoint, acceptance and tasks 3–7 were pending. Codex changed only the workflow, `AGENTS.md`, and this roadmap for that implementation; no application builds/tests, live Docker/database/deployment commands, resource provisioning, workflow triggers, screenshots, commits, pushes, or PR creation were performed.

**Subsequent user-reported acceptance:** after the runner troubleshooting, the user confirmed all jobs ran and were green. Source inspection confirms `deploy_staging` invokes the fail-fast deployment script and database-backed smoke check, so that report satisfies task 2's successful execution acceptance. No new run URL, commit, registry digest, runtime-image comparison, or screenshot was supplied; collect these for the release/evidence records. Codex has not independently verified that GitHub run or EC2 state. Do not reuse Phase 4's older digest as the identifier of this later run. At this acceptance checkpoint, production automation and both Phase 5 exit gates remained pending; task 4's subsequent implementation is recorded below.

### Task 3 smoke-check verification — October 11, 2026

**Task 3 is complete for the smoke-check contract.** The existing [scripts/smoke.sh](../scripts/smoke.sh) and [scripts/deploy.sh](../scripts/deploy.sh) required no fixes. Deployment waits up to 120 seconds for Compose health before invoking smoke; smoke allows ten requests, a two-second connection timeout, a five-second request timeout, and two-second delays between failed attempts. It requires HTTP `200`, one JSON array, and seeded company `EMP-001`, and exits nonzero after retry exhaustion. The user-reported green staging job supplies successful API/deployment execution evidence; the controlled checks below independently verify script behavior against temporary HTTP fixtures.

Added [scripts/verify-smoke.py](../scripts/verify-smoke.py), a repeatable verifier using Python's standard library and the real Bash/curl/jq smoke script. Each scenario uses a separate temporary loopback port. It reserves a non-listening port for connection refusal and delays HTTP replies beyond curl's timeout for the timeout case. No Docker, application/database process, host environment file, AWS resource, or deployment is used by the verifier.

Run from the repository root with Python 3, Bash, curl, and jq installed:

```bash
bash -n scripts/smoke.sh scripts/deploy.sh
python3 scripts/verify-smoke.py
```

The verifier runs independent scenarios concurrently with the actual retry/delay values, takes about seventy seconds, returns `0` only when every assertion passes, and closes its fixture servers. Each case checks exit status, attempts, expected messages, response-body log exclusion, and temporary-file cleanup. It does not shorten retries or mock curl/jq.

Codex executed these checks against local base commit `d6c449a0d6dfa4d8a60599b7363ff3ba40d974f3` plus the uncommitted verifier:

| Scenario | Observed result |
| --- | --- |
| HTTP `200`, array containing `EMP-001` | Exit `0`, one request, `0.3s` |
| Two HTTP `503` responses followed by valid data | Exit `0`, three requests, `4.3s` |
| HTTP `201` or `503` with otherwise valid data | Exit `1`, ten attempts each, `18.4s` |
| Redirect to an endpoint with valid data | Exit `1`, ten requests, `18.4s`; redirect not followed |
| HTML, malformed JSON, multiple JSON values, object instead of array, empty array, or missing `EMP-001` | All six cases exited `1` after ten attempts, `19.5–19.8s` |
| Connection refused | Exit `1`, ten failed attempts, `18.3s` |
| Replies delayed beyond the request timeout | Exit `1`, ten requests, `68.4s` |

Both Bash scripts and the Python verifier passed syntax checks. The initial sandbox run exited `1` because local socket creation was prohibited (`Operation not permitted`); the permitted rerun outside that sandbox passed all thirteen scenarios and exited `0`. Failed cases printed `Smoke check failed after 10 attempts`; successful cases printed `Smoke check passed: HTTP 200 and EMP-001`. Every case removed its temporary response file, and the fixture response-body marker was absent from script output.

These are real local HTTP/script checks using artificial responses. They do not independently prove EC2 database readiness or a failed GitHub deployment blocking production. No application builds/tests, Docker/database commands, EC2 access, workflow trigger, screenshots, commits, pushes, or PR creation occurred for this task. Live database-outage, complete pipeline blocking, concurrency, and persistence evidence remain in their later tasks. At this task 3 checkpoint, the next bounded implementation was task 4: production deployment after staging succeeds, using the same digest. Its subsequent implementation and acceptance are recorded below. Both Phase 5 exit gates were unchecked at that task 3 checkpoint; task 4's acceptance below satisfies the first gate.

### Task 4 production workflow — October 11, 2026

**Task 4 is complete based on the user's subsequent successful-workflow report and matching runtime image references.** At implementation, real-run acceptance was pending. [ci.yml](../.github/workflows/ci.yml) adds `deploy_production`, displayed as **Deploy and verify production**, within the existing workflow and concurrency group. The new job requires both `image` and `deploy_staging` to succeed, runs only for a `push` to `refs/heads/master`, and uses the existing `[self-hosted, linux, x64, carbono-deploy]` runner. Its checkout uses the exact `${{ github.sha }}` with `persist-credentials: false`, so production receives the same release revision as staging.

The job reads `${{ needs.image.outputs.digest }}` through `CARBONO_DIGEST`, just as staging does, and calls the existing deployment script with:

| Input | Production value |
| --- | --- |
| Compose project | `carbono-production` |
| Environment file | `/home/ubuntu/carbono/production/production.env`, outside the checkout |
| Application image | `gui2506/web-fiap-carbono@` followed by this run's published registry digest |
| Database-backed API URL | `http://127.0.0.1:8082/api/empresas` |
| Job timeout | Ten minutes |

No production image rebuild, source pull into the manual host checkout, or new SSH/AWS credential is introduced. The existing script validates the selected image, pulls it, updates the named project without building, waits up to 120 seconds for Compose health, and requires the existing bounded HTTP/JSON/`EMP-001` smoke check to pass. It uses production's existing configuration, network, and persistent MongoDB volume; no storage deletion or seed rerun is added. No `always()` or `continue-on-error` is added to the production job. GitHub's [job dependency rules](https://docs.github.com/en/actions/how-tos/write-workflows/choose-what-workflows-do/use-jobs) block this job when publication or staging fails or is skipped; its direct `image` dependency also permits [reading the published job output](https://docs.github.com/en/actions/how-tos/write-workflows/choose-what-workflows-do/pass-job-outputs).

Codex verified the uncommitted workflow against local base commit `c18d3e34341ee93ffb58f4ee8f219ee2d541b31b`:

| Local check | Observed result |
| --- | --- |
| YAML parse and workflow structure | Passed; existing jobs, triggers, permissions, and concurrency unchanged; production dependencies, success/`master` gate, runner, checkout revision, digest input, and environment arguments checked |
| `bash -n` on all workflow shell blocks and both existing deployment scripts | Exit `0`; no syntax errors |
| Twelve synthetic integration scenarios | All passed: staging and production success with the same supplied digest; production rejection of empty/invalid digest, missing environment file, Compose configuration failure, wrong Compose image, pull failure, health-wait failure, HTTP failure, invalid JSON, and missing `EMP-001` |

The synthetic checks executed the actual workflow deployment shell blocks and unchanged `deploy.sh`/`smoke.sh` in temporary checkouts, replacing only the host environment-file path with a synthetic local file. Docker, curl, and sleep were mocked; Bash and jq were real. Checks asserted project/environment/digest/URL forwarding, `--no-build`, bounded health wait, nonzero failure propagation, and absence of a deployment-success message on failures. They made no network requests or live container changes. The temporary checkouts were cleaned up. These checks do not execute GitHub scheduling, actual image pulls, EC2 health checks, or production deployment.

The following task 4 checks were prescribed at implementation; retain them for future releases and evidence collection:

1. Confirm CI and publication succeed, then staging succeeds before **Deploy and verify production** starts. Production must execute on the existing EC2 deployment runner.
2. Compare the published digest and the staging/production deployment log references. Both must use this run's exact `gui2506/web-fiap-carbono@sha256:...` value. Production must print `Smoke check passed: HTTP 200 and EMP-001` and `Deployment verified: carbono-production`.
3. Inside EC2's SSH terminal, inspect both runtime image references:

   ```bash
   docker inspect --type container \
     --format '{{.Name}}: {{.Config.Image}}' \
     carbono-staging-api-1 \
     carbono-production-api-1
   ```

   Both must match the registry digest output of that run. Confirm both environments remain healthy and retain their separate database resources.
4. Record the actual run URL, commit, digest, and production smoke result. On a PR run, publication and both deployment jobs must be skipped. Keep deliberate staging-failure and concurrency exercises for their assigned later verification tasks.

At the implementation checkpoint, task 4 and both Phase 5 exit gates remained unchecked pending real-run acceptance. Codex changed only the workflow, `AGENTS.md`, and this roadmap; no application builds/tests, actual Docker/database/deployment commands, EC2 access, resource provisioning, workflow triggers, screenshots, commits, pushes, or PR creation were performed for that implementation.

**Subsequent user-supplied acceptance — October 11:** the user reported that everything passed and all workflow jobs were green, then supplied EC2 output from the runtime image inspection above:

```text
/carbono-staging-api-1: gui2506/web-fiap-carbono@sha256:4deaaaa81939cf9cc27ac140cc993631139b576d281ad991452170e92a1e4da7
/carbono-production-api-1: gui2506/web-fiap-carbono@sha256:4deaaaa81939cf9cc27ac140cc993631139b576d281ad991452170e92a1e4da7
```

Both containers reference exactly the same image digest. Together with the all-green workflow report and the inspected job configuration, which passes `needs.image.outputs.digest` to both fail-fast deployment/smoke steps, this satisfies task 4's successful execution and same-image promotion acceptance. Passing smoke checks are inferred from the reported successful jobs and their script behavior; no new raw smoke log was supplied. Codex did not independently inspect that GitHub run, EC2 state, or registry metadata. No run URL, run commit, or screenshot was supplied for this deployment; collect these for the later evidence/release record and do not associate the new digest with Phase 4's older run or commit.

The first Phase 5 exit gate is satisfied based on this user-supplied evidence. Tasks 5–7 and the second exit gate remain pending; the next bounded task is task 5's failure-blocking and concurrency verification. This acceptance documentation update changes only `AGENTS.md` and this roadmap, without executing builds, tests, database scripts, deployments, or remote checks. Phase 5 is not complete.

### Task 5 failure blocking and concurrency verification — October 11, 2026

**Task 5 is complete based on user-supplied failure, recovery, and A/B concurrency results.** The user confirmed production was skipped in the controlled staging failure run, supplied unchanged/available production results, and reported the recovery workflow all green after removing the temporary step. Existing job dependencies already enforce `ci` → `image` → `deploy_staging` → `deploy_production`; production directly requires both `image` and `deploy_staging`, and delivery jobs retain their successful-`master`-push conditions. Workflow-level `carbono-release-${{ github.ref }}` concurrency retains `cancel-in-progress: false` across the full release sequence.

[ci.yml](../.github/workflows/ci.yml) temporarily included **Temporary staging failure verification**, immediately after staging's release checkout and before **Deploy staging by published digest**. It printed `Controlled staging failure: production must be skipped` and exited `1`. The existing fail-fast dependencies should skip staging's deployment step and the entire production job. PRs still run CI and skip delivery jobs. The temporary step had no Docker, API, database, or credential command; it deliberately blocked eligible `master` deployments until removed. CI and image publication executed before this step by configuration. Its removal below restores the normal deployment path without changing those dependencies.

Codex checked the uncommitted workflow against local base commit `6faff4572f8541924024da2c815790f1b19bf92b`:

| Local check | Observed result |
| --- | --- |
| YAML parse and structure | Passed; existing CI/image/production jobs, triggers, permissions, dependency conditions, and workflow concurrency unchanged; failure step correctly placed in staging |
| Recovery structure | Passed; removing the temporary step from the parsed workflow restores the previous workflow structure exactly |
| Bash syntax | All workflow shell blocks, the parsed recovery version's shell blocks, and the unchanged deployment/smoke scripts passed `bash -n` |
| Execute only the temporary step locally | Printed the expected controlled-failure message and exited `1`, as intended; no deployment command executed |

These local checks did not execute GitHub's scheduler, verify production availability, or prove non-overlapping releases. At that implementation checkpoint, no actual task 5 GitHub run or EC2 result had been observed or supplied. Subsequent user-supplied results are recorded below. Retain the original exercise procedure to interpret those results and complete recovery/concurrency acceptance:

1. **Inside EC2, before merging the temporary step:** save the output of `docker inspect --type container --format '{{.Id}} {{.Config.Image}} {{.State.StartedAt}}' carbono-production-api-1`. This records production's container identity, image, and start time.
2. **Commit and merge through the usual local/PR process:** CI should pass on the PR while delivery jobs are skipped. On the resulting `master` push, expect CI and publication to succeed, staging to fail at the named temporary step with exit `1`, staging deployment not to execute, and production to be skipped. Record the actual run URL, commit, and job results.
3. **Inside EC2 after that failed run:** repeat the production inspection; the container ID, image reference, and start time must match the saved output. Check availability with:

   ```bash
   curl --noproxy '*' --fail --silent --show-error \
     --max-time 10 --output /dev/null \
     --write-out 'Production HTTP %{http_code}\n' \
     http://127.0.0.1:8082/api/empresas
   ```

   Expect HTTP `200`, and retain the actual command/result. These are expected results, not checks performed by Codex.
4. **Remove the entire temporary step and its removal comment:** commit and merge the removal. Verify the new commit's run succeeds through staging and production. A rerun of the failed run uses its original commit and retains the failure step; use the new revision for recovery. Retain the failed run and recovery run links instead of overwriting their evidence.
5. **After recovery, on an up-to-date local `master` with a clean working tree:** use an empty commit to trigger release A:

   ```bash
   git commit --allow-empty -m "ci: verify deployment serialization A"
   git push origin master
   ```

   Wait until A is in progress on GitHub, then trigger release B with another empty commit:

   ```bash
   git commit --allow-empty -m "ci: verify deployment serialization B"
   git push origin master
   ```

   If branch protection requires PRs, use two permitted merges instead. B must wait for the shared workflow concurrency group while A continues, then run after A finishes. Capture both run URLs, commits, the concurrency wait, and job timestamps showing A's production job finishes before B's staging job starts. Waiting only for an available self-hosted runner is insufficient proof of workflow serialization. Keep this exercise to A and B; an additional trigger may replace an older pending run, while the running release must not be canceled by the newer trigger.

**Subsequent user-supplied results and local recovery preparation:** the user supplied two identical production inspections, an HTTP `200` availability result, and the controlled staging failure log:

| Check | Supplied result |
| --- | --- |
| Production container ID, baseline and repeated inspection | `2e28d79c4cfa482e3d0e35777c888c44b06a735817e485a13459d08c03a9c102` |
| Production image, baseline and repeated inspection | `gui2506/web-fiap-carbono@sha256:52639c502c9fe27931fc123bbb86e60173394291f3acec9a46bb11f857480c92` |
| Production start time, baseline and repeated inspection | `2026-10-11T21:23:01.578994627Z` |
| Host-local production availability | `Production HTTP 200` |
| Temporary staging step | Printed `Controlled staging failure: production must be skipped`; GitHub reported `Process completed with exit code 1` |
| Production job in that same failed run | User confirmed the job was skipped |
| Recovery workflow after failure-step removal | User reported the workflow all green |
| A/B concurrency | User reported B waiting for A, every job finishing, and B starting only after A finished |

The matching metadata shows production was not recreated between the supplied inspections; the curl result shows it was available when checked. Codex did not execute these EC2 commands or independently inspect the failed GitHub run. No run URL, run commit, or screenshot was supplied for this exercise. The `52639c...` reference is the observed production baseline for task 5, not a replacement for task 4's earlier `4deaaaa...` release record.

Codex removed only the temporary staging step and its removal comment from the workflow as recovery preparation. Local YAML/structure checks against base commit `ee8f94d746b977fdd1b478779f4e32f8b18fe4bc` confirmed all other workflow settings were unchanged, including production dependencies, digest promotion, release gates, and shared concurrency. All workflow shell blocks and both unchanged deployment scripts passed Bash syntax checks. At the local cleanup checkpoint, a real recovery deployment was still pending; the user subsequently reported the recovery workflow all green. Codex did not independently inspect that run. The old failed revision remains available in Git history and GitHub run history for inspection.

The controlled failure, skipped production, unchanged/available production, successful recovery, and serialized A/B sequence are complete based on user-supplied evidence. After reporting B waiting for A and every job finished, the user explicitly confirmed B started after A finished. No A/B run links, commit identifiers, raw timestamps, or screenshots were supplied; collect them for the later evidence record. Codex did not independently inspect those GitHub runs. At the task 5 checkpoint, the next bounded work was Phase 5 task 6: review credential handling; tasks 6–7 and the second exit gate were pending. Tasks 6–7 are subsequently complete as recorded below; the later storage-preservation record also closes the second exit gate. The controlled step tests job dependencies; Phase 6 still requires an actual controlled readiness/database failure and the remaining verification/evidence collection. Codex changed only the workflow, `AGENTS.md`, and this roadmap; no application builds/tests, live Docker/database/deployment commands, EC2 access, workflow triggers, provisioning, screenshots, commits, pushes, or PR creation occurred for either this implementation or local recovery preparation.

### Deployment image-validation fix — October 11, 2026

The user reported `Compose did not select the supplied API image`. Read-only GitHub inspection of [run 38183852655](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38183852655), a `master` push at commit `4f2e6580005f90bdcbedafb0ee4ec8d8f4095c40`, independently confirmed successful CI and image publication, failure in **Deploy staging by published digest**, and skipped production with no executed steps. The staging log received digest `sha256:4665c73bbaddd213eb455a03d35a393be3d0af927d8e821a50a65a4d783ee513` and printed the reported error before the script's pull/update commands. Codex did not independently inspect EC2 or the registry for this release.

Source inspection found a timing-dependent false rejection in `docker compose config --images | grep -Fxq "$image"` under `set -euo pipefail`. `grep -q` can exit after reading the matching API image, closing the pipe before Compose writes another service's image. The producer can then fail with a broken pipe, causing `pipefail` to reject a valid match. A controlled two-line producer reproduced pipeline exit `141` with producer/grep statuses `141 0`, and the regression verifier reproduced the original script's false rejection in both staging and production. This behavior is documented in the [GNU grep manual](https://www.gnu.org/software/grep/manual/html_node/Usage.html). The actual failed GitHub log did not expose the producer's status, so the specific EC2 SIGPIPE status is not claimed as independently observed.

[scripts/deploy.sh](../scripts/deploy.sh) now captures the complete `config --images` output before performing the exact digest match. A failed image-list command reports `Could not read Compose image configuration`; empty, incorrect, or partial image matches still fail with the original mismatch message. Validation remains before pulling/updating containers. Digest promotion, health/smoke checks, credentials, workflow dependencies/concurrency, and persistent database resources are unchanged.

Added [scripts/verify-deploy.py](../scripts/verify-deploy.py), a Python standard-library verifier using temporary checkouts, mock Docker/curl/sleep, and real Bash/grep/jq. It includes a delayed second image write to exercise the broken-pipe regression; it performs no real Docker or network operations. Run from the repository root:

```bash
bash -n scripts/deploy.sh scripts/smoke.sh
python3 scripts/verify-deploy.py
```

Codex verified against the local base commit above:

| Check | Observed result |
| --- | --- |
| Before/after regression | Original script rejected valid early matches in staging and production; the fixed script passed all fifteen scenarios |
| Valid image ordering | API image first or last passed, including production; mock health/smoke completed |
| Guard and failure cases | Invalid digest, missing environment file, invalid Compose configuration, image-list failure before/after a match, empty/wrong/partial matches, pull failure, health-wait failure, HTTP failure, and missing company all remained failures |
| Local Compose configuration | Compose `v5.1.3`, with a temporary synthetic environment file and exported `APP_IMAGE`, passed quiet validation and returned the failed run's exact API digest plus `mongo:8.0.29-noble` from `config --images` |
| Bash/Python syntax | Both existing scripts and the new verifier passed syntax checks |

The real Compose checks only rendered/validated local configuration; no image was pulled or container/database changed. The fifteen script scenarios used mocks and were not application tests, live deployments, or a successful GitHub recovery run. At that implementation checkpoint, the fix/documentation were uncommitted and a new successful run remained pending. The user subsequently committed/pushed the fix as `57bf5021356a8796b33df175ddde056e13559df6`; task 7 records independent inspection of its successful run. Do not use an occasional successful rerun of the old revision as proof the race is fixed. Tasks 6–7 are subsequently complete as recorded below; the later storage-preservation check also satisfies the second Phase 5 exit gate. No workflow was triggered, no AWS resource was provisioned, and no screenshot, commit, push, or PR was created by Codex for the fix.

### Task 6 credential verification — October 11, 2026

**Task 6 is complete based on source inspection and user-supplied results.** No new credentials or configuration changes were needed for this documentation task.

| Check | Result and attribution |
| --- | --- |
| Tracked environment/development filenames | User's root `git ls-files` output listed only `.env.example`; Codex independently repeated this filename check with the same result |
| Tracked API settings | Codex read the configuration without printing key values and verified `Jwt.SecretKey` is empty |
| Untracked host files | User supplied `600 ubuntu` for both `/home/ubuntu/carbono/staging/staging.env` and `/home/ubuntu/carbono/production/production.env` |
| Published image configuration | User supplied `true` from `jq -e '.Jwt.SecretKey == ""' /app/appsettings.json` in each deployed API container |
| Workflow logs | User confirmed reviewing them and finding no signing keys, tokens, passwords, or environment-file contents |
| Registry access | User confirmed `gui2506/web-fiap-carbono` is public; no host registry-login credential is required |
| Script/workflow/build context | Inspection shows publication credentials supplied through GitHub Secrets, runtime files outside the checkout, no secret build arguments or shell tracing, and environment/development-file exclusions in `.dockerignore` |

These checks establish the reviewed credential handling; they do not claim an exhaustive audit of every historical image layer or Git revision. Codex did not access EC2, read actual host environment files, or obtain secret values. Screenshots and final ZIP credential checks remain assigned later work.

### Task 7 release record and manual recovery — October 11, 2026

**Task 7 is complete.** Added [docs/ci-cd-deployment.md](ci-cd-deployment.md) with a real successful release record and a manual recovery procedure using the existing deployment/smoke scripts.

Read-only GitHub inspection independently verified [run 38184996037](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38184996037), a `master` push at commit `57bf5021356a8796b33df175ddde056e13559df6`. All four jobs succeeded. The CI log reports 107 passed, 0 failed, 0 skipped. Publication logged registry digest `sha256:dc309ed5084582eaa51a8f0143370a26e12dd41a69d811c9e85ba7bebb478b8e`; staging and production logs selected the same `gui2506/web-fiap-carbono@sha256:...` reference, reported healthy API/MongoDB containers, and printed both `Smoke check passed: HTTP 200 and EMP-001` and their environment-specific `Deployment verified` message. Staging completed at `23:39:45 UTC`; production started at `23:39:46 UTC` and completed at `23:39:57 UTC` on October 11. The release record includes the full source/image identifiers, SHA tag, all job timestamps, Compose projects, untracked configuration paths, API URLs, and the earlier user-supplied database-volume names.

The procedure selects a clean matching manual checkout, waits for release jobs to finish, and stops the idle EC2 deployment runner before manual deployment. It recovers staging first, blocks production if staging fails, uses a recorded digest without rebuilding, compares the preserved MongoDB mounts, and resumes the runner after verification and review of queued releases. The recorded successful release is the known-good baseline to use after a later failed release; no rollback is claimed for the current run. No volume removal, database reset, replacing seed, automatic rollback, new infrastructure, or workflow/application change is introduced.

The documentation's Bash blocks passed syntax validation and its paths/arguments were checked against the current scripts, workflow, and Compose source. `git diff --check` passed. These are documentation checks, not execution of the recovery procedure. Codex inspected existing GitHub results but did not trigger a workflow, build/test the application locally, access EC2, pull/deploy an image, operate the runner service, mutate data, capture screenshots, commit, push, or open a PR. Direct current host/registry inspection and actual recovery/persistence results were not produced.

At the task 7 checkpoint, all seven Phase 5 tasks were complete while actual storage preservation remained pending. The subsequent check below now satisfies that exit gate. The release/recovery guide alone does not prove persistence; the user supplied actual recreation and before/after record results separately.

### Storage preservation and Phase 5 completion — October 12, 2026 (UTC)

**Phase 5 and both exit gates are complete.** The user supplied actual EC2 terminal results for staging container recreation, record preservation, production isolation/availability, exact cleanup, and runner-service restart. Dates follow the host's UTC timestamps. The full case, command, and result record is in [docs/ci-cd-deployment.md](ci-cd-deployment.md).

| Check | User-supplied result |
| --- | --- |
| Captured staging API image | `gui2506/web-fiap-carbono@sha256:a3ae2c511135228a589224d1c31bbcebb3dbfe7d46d2dfc147ee491411a0b3ff` |
| Temporary product | `CRUD-TEMP-PHASE5-PERSISTENCE`, ObjectId `6acc284f1465a1dca91ca03a`, created/updated at `2026-10-12T00:22:39.955Z` |
| Staging MongoDB container before | `aab2af55d7435324efc82e79ca954999b86ebfe5eca37ad00464d52061d27506` |
| Staging MongoDB container after | `5580882b53d1be47b16a116c89371ebb8740344be69dfc91157053b9dfc0f565` |
| Database volume before/after | `carbono-staging_fiap_carbono_mongodb_data` |
| Staging recreation | `up -d --no-build --pull missing --force-recreate --wait --wait-timeout 120 api mongodb` reported both containers healthy |
| Record comparison | Exactly one matching record after recreation; fail-fast snapshot comparison printed `Persistence comparison passed` |
| Production isolation | Matching temporary-product count `0` before insertion and after staging recreation |
| API availability after recreation | `Port 8081: HTTP 200` and `Port 8082: HTTP 200` |
| Exact cleanup | `acknowledged: true`, `deletedCount: 1`, `Remaining: 0` |
| Runner-service window | Stopped/inactive at `00:19:31 UTC`; restarted/active and enabled at `00:30:51 UTC` on October 12 |

The comparison used `/tmp/carbono-phase5-before.json` and `/tmp/carbono-phase5-after.json` on EC2. The unchanged snapshot and different MongoDB container ID with the same mounted volume prove preservation across actual staging recreation. Cleanup selected the exact temporary ObjectId, code, and name; production data was not mutated by the shown commands. Runner-service status confirms it was restarted after cleanup; GitHub online/idle status was not supplied.

No source commit or workflow run was supplied for the captured `a3ae2c...` image, and no new production image reference was shown. Preserve task 7's earlier `57bf502...`/`dc309ed...` run record without substituting this later staging observation. The check did not execute the manual image-rollback procedure, continuously probe production during recreation, or provide initialization logs/screenshots. It establishes preservation of the complete temporary record, not an inspection of every seed/startup message.

Codex reviewed the supplied terminal output and updated documentation only. No application build/test, database script, deployment, EC2 command, resource provisioning, workflow trigger, screenshot, commit, push, or PR was performed for this documentation update. The existing results also satisfy Phase 6's record-persistence item; reuse them without repeating the same check. The remaining Phase 6 verification/evidence tasks and both exit gates stay pending.

### Verification

Use a real successful `master` workflow to inspect check → publish → staging → production order and both smoke-check results. On the host, run `docker compose ... images` and inspect each API container's image against the registry digest; both must match the digest output of that run. Readiness must reject unexpected redirects/HTML and must not rely on Swagger being enabled.

### Exit gate

- [x] A successful `master` push automatically deploys and verifies staging, then production, with identical application digests. Task 4 records earlier user-supplied runtime references; task 7 independently verifies run `38184996037`, both deployment logs/smoke results, matching published digest, and production starting after staging completes. No direct current EC2 inspection was performed for task 7.
- [x] Failures stop dependent stages, deployment runs cannot overlap, and updates preserve database storage. Task 5 supplies user-confirmed failure-blocking/concurrency acceptance; the later staging recreation retained its MongoDB volume and unchanged temporary record, with production isolated/available afterward, cleanup complete, and the runner service restarted. This satisfies Phase 5 based on user-supplied host results; the recovery guide itself remains unexecuted.

## Phase 6 — October 11: verify the full flow and collect evidence

**Purpose:** prove delivery, failure handling, persistence, and isolation through actual execution.

**Existing evidence for later checks:** Phase 4 task 5 preserves publication run `38090160366`, commit `6458819e04eecf1d35a13648a509210f90bf8fc1`, and its independently matched Docker Hub digest. Phase 5 task 4 preserves the earlier user-supplied staging/production reference `sha256:4deaaaa81939cf9cc27ac140cc993631139b576d281ad991452170e92a1e4da7` without an associated run/commit. Task 5 supplies user-confirmed job failure blocking and A/B serialization; task 7 adds independently inspected successful run `38184996037`, its commit/digest, deployment order, and both database-backed smoke results in [docs/ci-cd-deployment.md](ci-cd-deployment.md). The later user-supplied staging recreation and unchanged snapshot comparison also satisfy the record-persistence item below; cleanup and runner-service restart are recorded with Phase 5 completion. Reuse these dated records while collecting missing PR links, raw failure/concurrency evidence, and screenshots. The remaining Phase 6 tasks and both exit gates stay pending, including the controlled staging readiness/database failure and remaining API/resource/evidence checks.

### Tasks and verification

- [ ] Run the complete happy path and record the PR/run links, commit SHA, Docker Hub digest, deployment sequence, and both database-backed API responses.
- [x] Use a temporary PR change to produce a real test failure and show that publication/deployment do not occur; restore the passing state afterward. PR #1/run `38302777058` attempt 1 prepared the controlled test, passed restore/build, failed tests, retained diagnostics, and skipped all delivery; attempt 2 restored successful CI with delivery still skipped. Job metadata was independently inspected and user screenshots `10-failure-evidence.png`/`11-build-before-merge.png` were reviewed. Downloaded report contents were not independently inspected; retain them for delivery.
- [ ] In a controlled staging verification window, cause a bounded staging readiness/smoke failure and prove production is skipped and its previous image remains in place. Restore staging and rerun successfully; do not damage persistent data or deliberately interrupt production.
- [x] Exercise close successive deployment triggers and inspect timestamps/concurrency status to prove complete deployment sequences do not overlap. The user's A/B confirmation is now supplemented by independently inspected runs `38182523444` and `38182543656`: B's first job started at `23:01:03 UTC`, after A's production job completed at `23:01:00 UTC` on October 11. No superseded pending run was identified in this pair; screenshot capture remains pending.
- [x] Create a clearly named temporary record in staging, recreate/redeploy its containers without removing volumes, and prove the record persists and seed initialization has not overwritten it. Satisfied by the user-supplied Phase 5 storage check: `CRUD-TEMP-PHASE5-PERSISTENCE` survived unchanged, the MongoDB container ID changed, and its volume stayed the same. This credits the already-executed persistence check; initialization logs were not supplied and no claim of a complete seed-log audit is made.
- [x] Show that the staging record is absent in production. Restart staging alone and confirm production stays available. Verify separate volumes, networks, ports, and environment configuration without exposing values of secrets. Reuse the Phase 5 production counts of zero and HTTP `200` from both APIs after staging recreation, together with the user's October 12 successful environment report below. This establishes sampled availability and isolation; continuous availability is not claimed. Screenshots remain pending under task 8.
- [x] Verify login, role restrictions, and representative emission/analytics behavior using existing contracts. The user's October 12 API checker passed in both environments with exit `0`, including both roles, rejection/validation responses, fifteen persisted emission calculations per environment, analytics totals, and production rejection of staging's JWT. No domain records were changed. `09-api-auth-analytics.png` is now present and visually reviewed; the other captures remain under task 8.
- [ ] Capture real evidence in the planned `docs/images/ci-cd/` directory and record dates, commit/digest, environment, commands, and results. Do not invent screenshots, replace execution with source-code images, or reuse historical migration screenshots as deployment proof.

### Verification helpers and evidence progress — October 12, 2026

[docs/ci-cd-verification.md](ci-cd-verification.md) now provides the host commands, optional real failure exercises, evidence requirements, and remaining acceptance checks. At the implementation checkpoint, [docs/images/ci-cd/README.md](images/ci-cd/README.md) indexed captures still to collect; the subsequent screenshot review below now records five supplied images without inventing any. Phase 6 remains in progress and both exit gates stay pending.

Implemented helpers:

- [scripts/phase6-environments.py](../scripts/phase6-environments.py): read-only host checks for healthy containers, same published API digest, separate ports/volumes/networks/configuration/signing keys, no published MongoDB port, and empty packaged JWT keys. Raw environment values are not printed.
- [scripts/phase6-api.py](../scripts/phase6-api.py): hidden-password login/role checks, malformed write-route validation without domain writes, persisted decimal emission formula, analytics totals, and rejection of staging's JWT by production. The subsequent user-supplied EC2 results below complete task 7; the helper does not create a calculated emission.
- [scripts/phase6-staging-failure.sh](../scripts/phase6-staging-failure.sh): explicitly selected staging MongoDB stop, real smoke rejection, restoration of the same container, and unchanged/available production checks. Expected exit `1` plus its final recovery marker permits a real failed staging job to block production. A recovery error is not a passing exercise.
- [ci.yml](../.github/workflows/ci.yml): default-off repository variables `PHASE6_VERIFY_CI_FAILURE` (PR-only generated failing xUnit Fact) and `PHASE6_VERIFY_STAGING_FAILURE` (post-deployment staging outage/recovery). The latter retains a fourteen-day `phase6-staging-failure` artifact. Reset both variables to unset/`false` after evidence collection; existing successful-CI/`master` gates, promotion by digest, production dependency, and workflow concurrency remain unchanged.

Local verification: all seventeen mocked regression cases in [scripts/verify-phase6.py](../scripts/verify-phase6.py) passed, Bash/workflow syntax and gate-preservation checks passed, a temporary local controlled Fact produced one expected failure/TRX, and after its removal the solution rebuilt and all 107 tests passed with zero failed/skipped. Generated reports are under ignored `artifacts/`. These are local results, not real PR, EC2 outage, isolation, or deployed API evidence. Codex did not publish changes, set GitHub variables, trigger workflows, deploy, access EC2, or capture screenshots.

Read-only GitHub job inspection on October 12 supplies the missing concurrency timestamps. [Run A](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38182523444), SHA `107d6b29661a5cf8704ff8c45f0cad2ca57bf35f`, was created October 11 at `22:58:44 UTC` and completed its production job at `23:01:00 UTC`. [Run B](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38182543656), SHA `44704e096442fd385a404ed196d83d222dc0934c`, was created at `22:59:07 UTC`, started CI at `23:01:03 UTC`, and completed production at `23:03:08 UTC`. Both succeeded. Together with the earlier user-observed waiting status and the unchanged concurrency group, this completes task 4's execution evidence; its screenshot is still pending. Task 5's persistence result remains credited. At this implementation checkpoint, tasks 1–3 and 6–8 still needed the missing real results/captures described in the guide; the subsequent report below completes task 6.

### Task 6 environment report — October 12, 2026

The user supplied successful EC2 fast-forward output from `230a6eb` to `fa133df`, then ran `python3 scripts/phase6-environments.py`. The report's timestamp is `2026-10-12T19:16:15.810318+00:00`, and its exit code is `0`. Both API/MongoDB pairs are healthy; both APIs select `gui2506/web-fiap-carbono@sha256:028cd12fc13f1bd56fbe2e5182bea24259cae0f8ec6bb5867ec9e006be02a279`. Ports are `8081`/`8082`, MongoDB has no published host ports, volumes/networks are separate, runtime configuration is valid, signing keys differ, and packaged JWT keys are empty. [The verification guide](ci-cd-verification.md#user-supplied-environment-report--october-12-2026) records the full container IDs and resource names.

Together with the prior production marker absence and HTTP `200` checks after staging recreation, this satisfies task 6's isolation and sampled availability checks. The report does not establish a new workflow/commit-to-digest association or current database-backed API responses, authentication/analytics, a real outage, uninterrupted availability, or screenshot capture. Keep this runtime digest separate from older release/persistence identifiers. Tasks 4–6 have execution evidence; tasks 1–3 and 7–8 and both exit gates remain pending. Codex reviewed user-supplied output and updated documentation only, without host commands, builds/tests, deployments, workflow triggers, or Git publication.

### Task 7 API verification — October 12, 2026

The user ran `python3 scripts/phase6-api.py --environment both` on EC2. The command timestamp is `2026-10-12T19:29:29.533439+00:00`, its final message is `API verification passed; passwords/tokens omitted; no MongoDB records changed`, and its exit code is `0`.

Both ports (`8081` staging, `8082` production) passed company/product reads and both application-role logins (`200`); invalid credentials, unauthenticated/invalid-token protected requests (`401`); analyst delete/factor restrictions (`403`); and allowed-role invalid-ID/empty-calculation-payload validation (`400`). Each environment passed the decimal emission formula and applied-factor checks for fifteen persisted emissions, plus product footprint, supplier ranking, and company dashboard totals. Production also rejected staging's JWT with `401`. These are actual user-supplied contract results, not independently executed Codex checks or evidence of a newly created calculated emission.

The output supplies current database-backed API responses; the standalone `smoke.sh` results were not included. It does not include a new runtime image inspection, corresponding publication run/SHA, or screenshots. [The verification guide](ci-cd-verification.md#user-supplied-api-results--october-12-2026) records the response/status matrix and capture requirements. Tasks 4–7 have execution evidence; tasks 1–3 and 8 and both Phase 6 exit gates remain pending. Codex updated documentation only, preserving the previous environment report and existing user changes; no application tests, host commands, workflow triggers, deployments, or Git publication were performed for this update.

### Standalone smoke results — October 12, 2026

The user supplied EC2 terminal output for `bash scripts/smoke.sh http://127.0.0.1:8081/api/empresas` (staging) and the corresponding `8082` production command. Both printed `Smoke check passed: HTTP 200 and EMP-001`; each immediate `echo $?` returned `0`. These satisfy current host-local smoke checks for HTTP status, one JSON array, and the seeded company, alongside the previously recorded environment and API contract results. Exact command timestamps, a new image/run/SHA association, and screenshot files were not supplied. Attribute execution to the user, not independent Codex verification.

Task 1's current API-response evidence is recorded, but its complete release/PR record remains pending. Tasks 1–3 and 8 and both exit gates stay unchecked. The next bounded work is the PR-only controlled test failure; [the verification guide](ci-cd-verification.md#2-verify-a-real-pr-test-failure) describes the opt-in variable, actual failing test/report, skipped delivery jobs, and passing recovery. This update changes documentation only; no application/host tests, deployment, workflow trigger, or Git publication was performed by Codex.

### Supplied screenshot review and failure/recovery runs — October 12, 2026

Five user PNGs are present under `docs/images/ci-cd/`: `09-api-auth-analytics.png`, `10-failure-evidence.png`, `11-build-before-merge.png`, `12-successful-workflow.png`, and `13-failed-staging-test.png`. Codex visually inspected them, preserved their filenames/content, and added captions/mappings in the [evidence index](images/ci-cd/README.md#reviewed-captures). No passwords, signing keys, private keys, or JWT values are visible in these captures. They are currently untracked; no Git publication is claimed. Exact screenshot capture times were not supplied, so captions use execution dates/times where observed.

Read-only inspection verifies [PR #1](https://github.com/guidonunes/Web.Fiap.Carbono/pull/1), commit `70743919e4a209ddf5e46c12d3d8fcf53e5cdcdb`, and [run 38302777058](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38302777058). Its original attempt failed tests after successful preparation/restore/build, uploaded diagnostics, and skipped all delivery; attempt 2 passed CI with the optional failure step and delivery skipped. Images 10/11 show this failure/recovery pair. This satisfies task 2's execution and screenshot evidence without claiming independently inspected downloaded report contents or actual test counters.

Image 12 shows the original successful `master` [run 38304927773](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38304927773/attempts/1), commit `b1f2bdb4783e9cdc97f1b9c34c6279a772c03b22`. Image 13 shows its [controlled-failure attempt 2](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38304927773/attempts/2); metadata confirms CI/publication and normal staging deployment succeeded, the database-failure step failed, its artifact uploaded, and production was skipped. [Attempt 3](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38304927773/attempts/3) has successful staging/production with the failure step skipped. Its CI/image jobs retain attempt 2's timestamps, so recovery does not establish new CI/publication execution. The screenshot overview does not show the database restoration marker or unchanged production metadata. Retain those from the actual artifact/log; task 3 remains unchecked until that proof and post-recovery runtime results are recorded. No additional outage is needed merely for a screenshot.

Tasks 2 and 4–7 have execution evidence. Task 1 still needs the current registry/runtime digest association and complete release/report record; task 8 still needs registry, host resource/persistence, concurrency and detailed outage/recovery evidence. Both Phase 6 exit gates remain pending. This update changes documentation only, preserves all user image files, and neither creates images nor commits, pushes, triggers workflows, or accesses EC2.

### Registry tag/digest capture — October 12, 2026

The user supplied registry metadata for `gui2506/web-fiap-carbono:b1f2bdb4783e9cdc97f1b9c34c6279a772c03b22`: digest `sha256:12e855e8d4552b394e41507cb300fbd33ff99170a016306e72745dadd47ef619`, media type `application/vnd.docker.distribution.manifest.v2+json`. The subsequently supplied `docker-hub-image.png` visibly shows the same full SHA tag/digest and `linux/amd64`; Codex reviewed it and added its caption without editing/renaming the image. Six real PNGs are now present. Exact capture/command time and CLI exit code were not supplied; Codex did not independently query Docker Hub or EC2.

This satisfies the registry screenshot requirement and records the current SHA-tag/digest mapping. [The verification guide](ci-cd-verification.md#user-supplied-registry-tagdigest--october-12-2026) supplies an exact `--image` environment check to compare the recovered APIs with that digest. The current runtime comparison remains pending; the earlier `028cd12...` report and `97355d...` production baseline retain their own dates/results. Publication-job digest logs were not read, so the new digest is not retroactively assigned to an earlier run attempt. Tasks 1, 3, and 8 and both Phase 6 exit gates still need the remaining release/runtime, detailed outage, reports, host/storage and concurrency evidence. This update changes documentation only and leaves all user images and Git publication untouched.

### Evidence plan

| Proposed filename | What the actual capture must show |
| --- | --- |
| `01-pr-ci.png` | PR restore/build/test result and run identity |
| `02-master-pipeline.png` | Ordered successful publish, staging, and production jobs |
| `03-docker-hub-image.png` | Repository, commit SHA tag, and digest |
| `04-staging.png` | Staging project health and database-backed API response |
| `05-production.png` | Production project health and database-backed API response |
| `06-failure-blocking.png` | Controlled staging failure, blocked production, and prior production image |
| `07-persistence-isolation.png` | Preserved temporary record, production absence, and separate resources |
| `08-deployment-concurrency.png` | Run timing/status showing non-overlapping deployment sequences |

Use additional captures when one image cannot show the required proof legibly, including CI failure. Add a short caption and interpretation below every screenshot in the README/report. Redact tokens, JWTs, signing keys, passwords, credential-bearing connection strings, and personal data. Keep environment/run identity visible.

### Exit gate

- [ ] Happy path, failure blocking, concurrency, persistence, isolation, and preserved API behavior have real recorded results.
- [ ] Evidence is readable, correctly attributed, and free of secrets; any blocker remains unchecked rather than reported as passed.

## Phase 7 — October 12: documentation and submission package

**Purpose:** make the submission complete and reproducible from the delivered files.

### Tasks

- [ ] Finish root README with exact project title **Projeto - Cidades ESGInteligentes** and these sections: **Como executar localmente com Docker**, **Pipeline CI/CD**, **Containerização**, **Prints do funcionamento**, and **Tecnologias utilizadas**.
- [ ] Document prerequisites, `.env.example` setup, safe first initialization, startup/readiness/API checks, persistence, actual image repository, ports, pipeline triggers/order, credentials configuration, and recovery commands. Preserve relevant business/API documentation.
- [ ] Create a PDF or PPT containing the project title and actual contributors, pipeline explanation, Docker architecture/commands/image reference, real pipeline and environment screenshots, and encountered challenges with their actual solutions. Obtain contributor names from the user or confirmed project records; do not invent them.
- [ ] Copy the mandatory delivery checklist below to the end of the README or PDF and complete it only from verified results. If the course provides an official checklist template, preserve its wording and verify every item against the same evidence.
- [ ] Create a new complete source ZIP containing the layout below, including the tests, migration utility's linked archive dependency, database scripts, workflows, deployment/smoke scripts, `.env.example`, and documentation/evidence.
- [ ] Exclude real `.env`/environment files, credentials, development settings containing secrets, `.git/`, `bin/`, `obj/`, local databases, IDE caches, and old/generated ZIPs. Ensure hidden required files such as `.github/workflows/`, `.dockerignore`, and `.env.example` are actually included.
- [ ] Inspect the ZIP listing, extract it to a separate directory, supply local untracked settings from the example, and verify the documented Docker flow and solution checks from the extracted source. Record actual results; do not depend on files left outside the ZIP.
- [ ] Review the README/report/evidence links, contributor list, checklist, image digest references, and submission filenames.

### Planned package layout

This is the target layout, not a claim that these files already exist:

```text
Dockerfile
docker-compose.yml
.dockerignore
.env.example
.gitignore
global.json
Web.Fiap.Carbono.sln
src/
  Web.Fiap.Carbono/
  Web.Fiap.Carbono.Migration/
Web.Fiap.Carbono.Tests/
.github/workflows/
scripts/                         # deployment and smoke checks
database/mongodb/                # existing creation/index/seed/CRUD/aggregation scripts
archive/oracle/                  # includes the migration utility's linked source
README.md
AGENTS.md
roadmap.md                       # migration history
docs/
  ci-cd-roadmap.md
  ci-cd-baseline.md              # created from real Phase 1 execution
  images/ci-cd/                  # real evidence
  ...                           # preserved existing technical documents
```

Deliver the PDF or PPT alongside the source ZIP as required by the submission portal; verify both files are included in the final upload.

### Verification

Use `unzip -l SUBMISSION.zip` with the actual archive filename to inspect required paths and exclusions. In a separate extracted checkout, follow the finished README, run the Phase 1 solution checks and Phase 2 Docker/API checks, and inspect the report/screenshots. Check that examples contain placeholders and no actual secrets. A ZIP filename or successful archive command alone is not proof of a complete package.

### Exit gate

- [ ] The ZIP is complete, clean, and verified from extraction; README commands reproduce the delivered behavior.
- [ ] The PDF/PPT identifies actual contributors and explains the implemented pipeline, Docker setup, real evidence, and challenges/solutions.
- [ ] The mandatory checklist appears at the end of the README or PDF and every completion claim is supported by verified results.

## October 13 — final checks and submission buffer

- [ ] Recheck delivered filenames, archive contents, PDF/PPT readability, evidence links, checklist, and the actual submission portal requirements.
- [ ] Confirm staging and production respond and identify the submitted commit/image digest without making unrelated last-minute changes.
- [ ] Upload the required artifacts before the deadline and retain the actual submission confirmation. Do not mark submission complete merely because files were prepared.

## Mandatory delivery checklist

This checklist covers the requirements supplied for this assignment. It remains unchecked until execution supplies proof; include the verified checklist at the end of the README or PDF.

- [ ] Complete source ZIP includes root `Dockerfile`, `docker-compose.yml`, `src/`, root tests and solution, `README.md`, `.github/workflows/`, scripts, `.env.example`, and all linked build dependencies.
- [ ] .NET 8 application, MongoDB, five ESG collections, business behavior, and authentication/authorization are preserved; restore, build, and existing tests pass.
- [ ] Local Docker instructions start API plus MongoDB with required variables, networking, persistent storage, readiness checks, and safe fresh-database initialization.
- [ ] GitHub Actions executes restore/build/tests for pull requests; failed checks block subsequent stages.
- [x] Successful `master` pushes publish the application image to Docker Hub with the commit SHA tag and a recorded digest. Verified by run `38090160366` and the independent Docker Hub SHA-tag/digest inspection in Phase 4 task 5 on October 10, 2026.
- [ ] Staging deploys automatically on the EC2 deployment runner and passes readiness plus a database-backed API smoke check.
- [ ] Production receives the same digest only after staging succeeds and passes its own verification.
- [ ] Deployments do not overlap; controlled failures demonstrate blocked later stages.
- [ ] Both isolated Compose projects run on the prepared Ubuntu host with independent API/MongoDB instances, volumes, networks, environment configuration, and documented ports.
- [x] Persistence and environment isolation have been verified through real operations. Phase 3 records independent resources/data; the user-supplied Phase 5 check adds unchanged staging data across container recreation, the retained volume, production absence, both HTTP `200` responses, and exact cleanup.
- [ ] Credentials remain in GitHub Secrets or untracked environment configuration and are absent from tracked files, images, evidence, and the ZIP.
- [ ] README title is **Projeto - Cidades ESGInteligentes** and contains **Como executar localmente com Docker**, **Pipeline CI/CD**, **Containerização**, **Prints do funcionamento**, and **Tecnologias utilizadas**.
- [ ] PDF or PPT includes the project title, actual contributors, pipeline explanation, Docker architecture/commands/image, real pipeline/environment screenshots, and actual challenges with their solutions.
- [ ] Real evidence has readable captions and traceable run/environment/commit context; no fabricated results or screenshots are used.
- [ ] This completed checklist is included at the end of the README or PDF and matches the verified artifacts.
- [ ] Source ZIP has been extracted and verified, required artifacts submitted by October 13, 2026, and submission confirmation retained.
