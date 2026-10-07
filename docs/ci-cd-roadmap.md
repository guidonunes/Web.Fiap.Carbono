# Web.Fiap.Carbono — Academic CI/CD Roadmap

## Goal and working agreement

Deliver a simple, functional CI/CD implementation for **Projeto - Cidades ESGInteligentes** by **October 13, 2026**. Complete implementation and packaging by October 12; reserve October 13 for final checks and submission.

This is the active roadmap for CI/CD work. Follow [AGENTS.md](../AGENTS.md): the user implements application and infrastructure changes to learn; Codex explains, supplies snippets, reviews, and troubleshoots. Guidance requests do not authorize edits. An explicit implementation request authorizes only its named changes and necessary supporting work, without repeated confirmation. Work on one bounded phase at a time and explain the purpose and verification of each change.

Preserve .NET 8, MongoDB, JWT authentication/authorization, existing API behavior, decimal calculations, immutable emission snapshots, and exactly five ESG domain collections: `empresas`, `produtos`, `fornecedores`, `fatores_emissao`, and `emissoes_carbono`. Do not add unrelated features, framework upgrades, Kubernetes, extra hosts, or a new database design.

[roadmap.md](../roadmap.md) and [mongodb-migration.md](mongodb-migration.md) are migration history and architectural references. Their phase order does not govern CI/CD. Preserve historical evidence and distinguish it from new verification. A checked box or existing file alone does not prove a phase passed.

## Observed starting point — October 6, 2026

These observations come from repository inspection only. No builds, tests, database scripts, container operations, or deployments were executed for this documentation update. All CI/CD tasks and exit gates below remain unverified.

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

Historical migration documents record earlier test results. Those results are not a current CI/CD baseline. Record new results in `docs/ci-cd-baseline.md` only when Phase 1 is executed.

## Planned architecture

Use GitHub Actions for CI/CD, Docker Hub for the application image, and Docker Compose on **one Ubuntu deployment host**. The host is a prerequisite to prepare in Phase 3, not an existing resource. Each environment contains its own API and MongoDB instance.

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

### Tasks

- [ ] Inspect `git status`, SDK/Docker prerequisites, and existing test configuration. Ensure clean-checkout tests receive synthetic JWT settings and do not depend on ignored developer files. Preserve unrelated local work.
- [ ] Run restore, build, and the complete existing tests before moving files; investigate failures and record actual outputs, warnings, failed/skipped tests, and blockers.
- [ ] Create `docs/ci-cd-baseline.md` during this phase with date, commit/revision, environment/tool versions, commands, exit codes, test counts, and sanitized evidence references. Separate results before and after reorganization.
- [ ] Move `Web.Fiap.Carbono/` and `Web.Fiap.Carbono.Migration/` under `src/`. Retain `Web.Fiap.Carbono.Tests/`, solution, `global.json`, `database/`, and `archive/` at the root.
- [ ] Move `Web.Fiap.Carbono/Dockerfile` to root `Dockerfile`; retain repository-root Docker build context and update `COPY`, restore, and working-directory paths.
- [ ] Update solution paths and both test project references. Check the migration-to-API reference, which remains between sibling directories.
- [ ] Update the API's linked `.dockerignore` path to reach the root, and the migration utility's archived `DatabaseContext.cs` link to reach root `archive/`.
- [ ] Verify WebApplicationFactory content-root resolution, test script discovery from the root solution, and any remaining path assumptions. Repair only what the move requires.
- [ ] Update current README/setup commands, migration utility guide links/commands, and other live path references affected by the move. Preserve `roadmap.md` as migration history and do not rewrite historical execution records as new results.
- [ ] Repeat solution verification, build the relocated Dockerfile, and record actual results. Retain existing .NET 8 and dependency choices unless a narrowly scoped baseline fix is required.

### Verification

From the repository root, execute these commands during Phase 1, before and after reorganization as applicable:

```bash
git status --short
dotnet --info
docker version
dotnet restore Web.Fiap.Carbono.sln
dotnet build Web.Fiap.Carbono.sln --no-restore
dotnet test Web.Fiap.Carbono.sln --no-build
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

- [ ] Restore/build/tests pass before and after the move; actual results are recorded in `docs/ci-cd-baseline.md`.
- [ ] The required layout is present, the root Dockerfile builds, references resolve, and current instructions use valid paths.
- [ ] Business behavior and historical artifacts remain preserved; no unexplained test regressions or missing prerequisites remain.

## Phase 2 — October 7–8: complete local Compose

**Purpose:** start the API and its persistent database reproducibly from the repository.

### Tasks

- [ ] Rename/consolidate the current MongoDB-only `compose.yaml` into canonical root `docker-compose.yml`; avoid leaving competing default Compose files. Update active commands accordingly.
- [ ] Define `api` and `mongodb`, API image/build settings, runtime environment variables, a project-scoped network, and a named MongoDB data volume.
- [ ] Remove the fixed MongoDB container name and unnecessary host database port. Parameterize the API host port; use `8080` inside the API container.
- [ ] Expand `.env.example` with all required nonsecret runtime/Compose values. Verify actual `.env`, `staging.env`, `production.env`, development settings, and credentials are excluded from Git, the image context, and packaging.
- [ ] Configure MongoDB readiness and bounded API readiness; make API startup wait for completed fresh-database initialization. Verify HTTP behavior with the existing HTTPS-redirection middleware and chosen container configuration.
- [ ] Wire only `01-create-collections.js`, `02-create-indexes.js`, and `03-seed.js` in that order into fresh-volume initialization, for example via Mongo's first-start init directory. Preserve `fiap_carbono`, five domain collections, validators, indexes, and coherent data.
- [ ] Ensure database ping success alone cannot race incomplete initialization. Check a known seeded record before accepting API readiness.
- [ ] Document fresh-volume versus existing-volume behavior. The seed replaces matching records: do not rerun it on every deployment. Never drop the database or automatically reset a failed/partially initialized volume. Report initialization failure for deliberate recovery.
- [ ] Keep `04-crud-demo.js` and `05-aggregation-queries.js` available for manual evidence; do not execute demos as startup hooks.
- [ ] Verify startup from a newly named development project/volume, then restart/recreate containers without deleting volumes and confirm stored data remains.

### Verification

After preparing an untracked local `.env` from the example, use the following planned commands. These depend on the Phase 2 file/services being implemented:

```bash
docker compose -f docker-compose.yml -p carbono-local --env-file .env config --quiet
docker compose -f docker-compose.yml -p carbono-local --env-file .env up -d --build --wait
docker compose -f docker-compose.yml -p carbono-local --env-file .env ps
curl --fail --silent --show-error --max-time 15 http://localhost:8081/api/empresas
docker compose -f docker-compose.yml -p carbono-local --env-file .env restart
```

Use `API_PORT=8081` in that local example. Inspect the returned JSON and a known seed key. Use `docker compose ... exec mongodb mongosh` with the chosen authentication configuration to inspect the five collections, counts, and indexes. Confirm at least ten coherent seed documents in each collection on the fresh demonstration database. Compare counts and a deliberately edited temporary record after restart/recreation to prove data was not reset. Do not use `down -v` or a database drop. Avoid capturing fully rendered Compose configuration containing secrets.

### Exit gate

- [ ] A fresh local project starts API plus MongoDB successfully with documented configuration and readiness checks.
- [ ] Initialization creates the expected five-collection dataset, and later restarts/recreations preserve stored data without reseeding.
- [ ] A database-backed API request succeeds; a database-unavailable check fails within a bounded time.

## Phase 3 — October 8: prepare the host and verify isolation

**Purpose:** make the single Ubuntu deployment host ready before automating deployment.

### Tasks

- [ ] Obtain the host and confirm Ubuntu, reachable SSH access, enough memory/disk for two API/MongoDB pairs, and required inbound API/SSH and outbound registry connectivity. Record the actual host setup without credentials.
- [ ] Install/verify Docker Engine and the Compose plugin; establish a deployment user able to run the required commands and verify the SSH host key.
- [ ] Prepare separate staging/production working directories and untracked environment files, distinct JWT signing configuration, and any chosen MongoDB credentials.
- [ ] Start `carbono-staging` on proposed API port `8081` and `carbono-production` on `8082`, each using its own MongoDB, volume, and network. Until Phase 4 publishes an image, a manual host build of the Phase 2 image is sufficient for this prerequisite check.
- [ ] Confirm the host permits required API access and does not expose MongoDB publicly.
- [ ] Verify both environments independently and record actual access URLs and project/resource names for later workflow configuration.

### Verification

Run `docker version`, `docker compose version`, and `docker compose ls` on the host. From each environment directory, run `docker compose -f docker-compose.yml -p carbono-staging --env-file staging.env config --quiet` (substitute the production project/file there) and then `up -d --wait` and `ps` with the same options.

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
