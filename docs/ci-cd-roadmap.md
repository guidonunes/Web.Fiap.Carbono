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

The user subsequently launched the host in **US East (Ohio), `us-east-2`**, availability zone **`us-east-2c`**. Use this actual region for host operations; N. Virginia remains the initial preference only. Deployment access uses SSH, as already selected by the pipeline plan. Docker/Compose installation and Docker access for the `ubuntu` deployment account are confirmed by user-supplied terminal output on October 9. The user also confirmed the SSH host-key fingerprint matched the AWS system log on October 9.

## Planned architecture

**GitHub Actions builds and tests the application, publishes versioned images to Docker Hub, and deploys the selected image version to one Ubuntu AWS EC2 host.** Docker Engine and Docker Compose are the deployment runtime; the application remains ASP.NET Core / .NET 8 with MongoDB. The user has confirmed host provisioning, SSH access and host-key comparison in Phase 3, and supplied successful Docker/Compose verification output. Deployment of the two environments remains pending. Each environment contains its own API and MongoDB instance. This hosting decision does not introduce CodePipeline, CodeBuild, ECR, ECS, EKS, or a managed database.

| Setting | Staging | Production |
| --- | --- | --- |
| Compose project | `carbono-staging` | `carbono-production` |
| Services | `api`, `mongodb` | `api`, `mongodb` |
| Proposed API host/container ports | `8081:8080` | `8082:8080` |
| Database name | `fiap_carbono` | `fiap_carbono` |
| MongoDB address from its API | Its own `mongodb:27017` service | Its own `mongodb:27017` service |
| Persistent storage | Project-scoped named MongoDB volume | Different project-scoped named MongoDB volume |
| Network | Project-scoped Compose network | Different project-scoped Compose network |
| Proposed host configuration file | `staging.env`, untracked | `production.env`, untracked |
| Application image | Published `repository@sha256:...` | Exact digest that passed staging |

Use the same Compose definition with separate project names and environment files. Avoid fixed container names, shared external networks/volumes, or explicit resource names that bypass project isolation. MongoDB need not publish a host port; both instances can listen on container port `27017` independently. Use `docker compose exec` for database inspection. The same database name does not mean shared data.

The API container must receive `MongoDb__ConnectionString`, `MongoDb__DatabaseName`, `Jwt__SecretKey`, `Jwt__Issuer`, `Jwt__Audience`, and `Jwt__ExpirationMinutes`. Connect to the MongoDB service name, not `localhost` inside the API container. Document the chosen ASP.NET Core environment and HTTP port configuration. Swagger currently runs only in Development; a Swagger page is not a deployment readiness check.

Proposed Compose interpolation keys are `APP_IMAGE` (a digest reference for deployment) and `API_PORT`. A Compose `--env-file` supplies interpolation values; explicitly map the API's required values into the container. Keep credentials in GitHub Secrets or untracked host environment files, with independent environment settings. Commit placeholders only. Ensure `.dockerignore` excludes development settings and all actual secret files before building/publishing; Git ignore rules alone do not protect a Docker build context.

## Planned pipeline and checks

```text
Pull request → restore → build → existing tests → CI result

Push to master → restore → build → existing tests
  → build and push Docker Hub image tagged with full commit SHA
  → capture registry digest
  → SSH deploy staging by digest → readiness + database-backed smoke check
  → SSH deploy production by the same digest → readiness + database-backed smoke check
```

Production follows automatically only after staging succeeds. Build once; do not rebuild for production or promote a mutable `latest` tag. Carry the registry digest as the publish job's output. Deploy the Compose/scripts revision associated with that commit as well, rather than pulling an unrelated newer `master` on the host.

Failures must return a nonzero exit status and block dependent jobs. PRs must never publish or receive deployment credentials. Use job dependencies to enforce checks → publication → staging → production, including smoke checks. Serialize the **whole staging-to-production deployment sequence across runs**, using one shared deployment concurrency group and `cancel-in-progress: false`; independent per-environment groups alone can interleave releases. Superseded queued runs need not deploy, but running deployments must not overlap. Manual deployments must not bypass that serialization.

Readiness means the API can serve requests after MongoDB and initialization are ready. Prefer a bounded retry check against the existing public `GET /api/empresas` endpoint; a new health endpoint is not required. Separately validate HTTP `200`, a JSON array, and a known seeded company's business key to prove a database-backed response. Choose and record that key after inspecting the seed; do not accept an empty response, HTML, redirect, or merely a running container. Confirm a database outage makes the check fail. Any container health-check command must actually be available in its image.

Plan GitHub Secrets for Docker Hub credentials, SSH private key, and sensitive environment values as needed. Store the host address/user and independently verified SSH host key in appropriate repository/environment configuration. Verify the host key; do not disable checking. Supply production configuration on the host or securely through the workflow without printing it. Use bounded SSH, readiness, and smoke-check timeouts; retain sanitized failure logs.

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
- [ ] **4. Prepare environment configuration.** Use separate staging/production working directories and untracked environment files, distinct JWT signing configuration, and any chosen MongoDB credentials.
- [ ] **5. Start both environments.** Run `carbono-staging` on proposed API port `8081` and `carbono-production` on `8082`, each using its own MongoDB, volume, and network. Until Phase 4 publishes an image, a manual host build of the Phase 2 image is sufficient for this prerequisite check.
- [ ] **6. Verify network exposure.** Confirm the host permits required API access and does not expose MongoDB publicly.
- [ ] **7. Verify isolation and record results.** Check both environments independently and record actual access URLs and project/resource names for later workflow configuration.

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

Task 3 is complete based on the supplied Docker output and the user's host-key comparison confirmation. Screenshots, environment configuration/deployment, and Phase 3 isolation/exit-gate checks remain pending. The next bounded task is task 4: separate staging/production directories and untracked environment files with distinct JWT signing configuration.

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

For the later environment tasks, from each environment directory, run `docker compose -f docker-compose.yml -p carbono-staging --env-file staging.env config --quiet` (substitute the production project/file there) and then `up -d --wait` and `ps` with the same options.

Request `http://HOST:8081/api/empresas` and `http://HOST:8082/api/empresas` using the actual host address. Inspect container mounts/networks to prove distinct resources; compare a temporary staging-only record against production and verify it is absent there. Both may initially contain the same seed data, so matching counts alone do not prove isolation.

### Exit gate

- [ ] The host and access prerequisites are verified, and both environments serve database-backed responses at their documented addresses.
- [ ] Volumes, networks, configuration, and data are independent; MongoDB is not publicly published.

## Phase 4 — October 9: implement CI and publish the image

**Purpose:** check each proposed change and produce one traceable application image for a successful `master` revision.

### Tasks

- [ ] Add `.github/workflows/` checks for pull requests and pushes to `master`: checkout, install the .NET 8 SDK, restore, build, and execute the existing full test suite on a Docker-capable Ubuntu runner.
- [ ] Supply synthetic test JWT settings and verify the current host-network/fixed-port fixture works without relying on developer files. Keep tests isolated from staging/production databases.
- [ ] Make failed restore/build/tests block publication; retain test reports and sanitized failure diagnostics.
- [ ] Configure the Docker Hub repository and credentials in GitHub Secrets. Build the root Dockerfile with root context only after successful `master` checks.
- [ ] Push an application image tagged with the full commit SHA; capture its registry digest as a job output for deployment. Do not publish from PRs.
- [ ] Verify `.dockerignore` excludes secrets before publication; record the image repository, SHA tag, digest, and corresponding workflow run without credentials.

### Verification

Inspect an actual PR workflow run for executed restore/build/tests and absence of publish/deploy steps. After an explicitly authorized push/merge to `master`, inspect the successful check/publish run and Docker Hub tag. Pull `DOCKERHUB_NAMESPACE/IMAGE@sha256:DIGEST` with the actual values and inspect its repo digest; verify the image starts with runtime configuration and contains the API rather than the migration tool. Confirm a controlled failing PR test results in failed CI and no publication.

### Exit gate

- [ ] Real PR and `master` runs execute the existing tests; failures block subsequent work.
- [ ] A successful `master` run publishes a SHA-tagged image with a recorded usable digest; PRs cannot publish or deploy.

## Phase 5 — October 10: automate staging and production deployment

**Purpose:** deliver the tested image automatically through staging to production.

### Tasks

- [ ] Add small deployment/smoke scripts under root `scripts/` with explicit project, environment-file, image-digest, and API URL inputs. Reject missing inputs and fail on command errors.
- [ ] Deploy the matching Compose/scripts revision over SSH with host-key verification. Set `APP_IMAGE` to the published digest, pull it, and start/update the staging API with the existing persistent database.
- [ ] Wait with bounded retries for readiness, then validate HTTP status, JSON shape, and a known database record through the API. Return nonzero for timeout or invalid output.
- [ ] Deploy production only after staging and its smoke checks succeed. Use the same digest and production's own configuration/resources, then run equivalent production checks.
- [ ] Enforce job dependencies and shared concurrency across the complete deployment sequence; do not use `continue-on-error` or unconditional production deployment.
- [ ] Keep registry/SSH/runtime credentials out of scripts, logs, command tracing, and image layers. If the image is private, provide host pull credentials securely.
- [ ] Record commit, digest, environment, and result. Document a simple manual recovery using the previous known digest without deleting MongoDB storage; automatic rollback is not required.

### Verification

Use a real successful `master` workflow to inspect check → publish → staging → production order and both smoke-check results. On the host, run `docker compose ... images` and inspect each API container's image against the registry digest; both must match the digest output of that run. Readiness must reject unexpected redirects/HTML and must not rely on Swagger being enabled.

### Exit gate

- [ ] A successful `master` push automatically deploys and verifies staging, then production, with identical application digests.
- [ ] Failures stop dependent stages, deployment runs cannot overlap, and updates preserve database storage.

## Phase 6 — October 11: verify the full flow and collect evidence

**Purpose:** prove delivery, failure handling, persistence, and isolation through actual execution.

### Tasks and verification

- [ ] Run the complete happy path and record the PR/run links, commit SHA, Docker Hub digest, deployment sequence, and both database-backed API responses.
- [ ] Use a temporary PR change to produce a real test failure and show that publication/deployment do not occur; restore the passing state afterward.
- [ ] In a controlled staging verification window, cause a bounded staging readiness/smoke failure and prove production is skipped and its previous image remains in place. Restore staging and rerun successfully; do not damage persistent data or deliberately interrupt production.
- [ ] Exercise close successive deployment triggers and inspect timestamps/concurrency status to prove complete deployment sequences do not overlap. Document any superseded pending run.
- [ ] Create a clearly named temporary record in staging, recreate/redeploy its containers without removing volumes, and prove the record persists and seed initialization has not overwritten it.
- [ ] Show that the staging record is absent in production. Restart staging alone and confirm production stays available. Verify separate volumes, networks, ports, and environment configuration without exposing values of secrets.
- [ ] Verify login, role restrictions, and representative emission/analytics behavior using existing contracts. Keep any destructive demonstration confined to explicitly named `CRUD-TEMP` records.
- [ ] Capture real evidence in the planned `docs/images/ci-cd/` directory and record dates, commit/digest, environment, commands, and results. Do not invent screenshots, replace execution with source-code images, or reuse historical migration screenshots as deployment proof.

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
- [ ] Successful `master` pushes publish the application image to Docker Hub with the commit SHA tag and a recorded digest.
- [ ] Staging deploys automatically over SSH and passes readiness plus a database-backed API smoke check.
- [ ] Production receives the same digest only after staging succeeds and passes its own verification.
- [ ] Deployments do not overlap; controlled failures demonstrate blocked later stages.
- [ ] Both isolated Compose projects run on the prepared Ubuntu host with independent API/MongoDB instances, volumes, networks, environment configuration, and documented ports.
- [ ] Persistence and environment isolation have been verified through real operations.
- [ ] Credentials remain in GitHub Secrets or untracked environment configuration and are absent from tracked files, images, evidence, and the ZIP.
- [ ] README title is **Projeto - Cidades ESGInteligentes** and contains **Como executar localmente com Docker**, **Pipeline CI/CD**, **Containerização**, **Prints do funcionamento**, and **Tecnologias utilizadas**.
- [ ] PDF or PPT includes the project title, actual contributors, pipeline explanation, Docker architecture/commands/image, real pipeline/environment screenshots, and actual challenges with their solutions.
- [ ] Real evidence has readable captions and traceable run/environment/commit context; no fabricated results or screenshots are used.
- [ ] This completed checklist is included at the end of the README or PDF and matches the verified artifacts.
- [ ] Source ZIP has been extracted and verified, required artifacts submitted by October 13, 2026, and submission confirmation retained.
