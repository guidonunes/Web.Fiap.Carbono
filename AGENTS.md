# AGENTS.md

## Purpose

This file defines how coding agents must work in the **Web.Fiap.Carbono** repository during the academic CI/CD assignment, due **October 13, 2026**. The goal is a simple, functional implementation that satisfies every assignment requirement.

The user will implement application and infrastructure changes as a learning exercise. Agents should explain concepts, propose bounded tasks, review changes, and help troubleshoot. The default behavior is to inspect and guide. Do not edit code or files unless the user explicitly asks for implementation. Keep solutions suitable for an academic project; avoid unnecessary infrastructure, application features, and framework upgrades.

These instructions apply to the entire repository unless a more specific `AGENTS.md` exists in a subdirectory. Direct instructions from the user take precedence over this file.

## Project context

Web.Fiap.Carbono is a .NET 8 ASP.NET Core Web API that measures and analyzes greenhouse-gas emissions across product supply chains.

The current source uses:

- C# and .NET 8;
- ASP.NET Core Web API;
- MongoDB through the official `MongoDB.Driver`;
- JWT authentication;
- explicit document/DTO mapping;
- xUnit API and persistence integration tests with disposable MongoDB containers.

Preserve .NET 8, MongoDB, the five-collection design, emission calculation, validation, analytics, authentication, authorization, and error responses. Oracle/EF Core remains in the separate migration utility and archived source, not the active API persistence registrations. Preserve those references without making Oracle a deployment prerequisite.

The active assignment plans:

- GitHub Actions for pull-request checks and delivery from `master`;
- Docker Hub images tagged with the commit SHA and promoted by digest;
- Docker Compose on one Ubuntu AWS EC2 host, with separate staging and production projects, each with its own API, MongoDB, volume, network, and environment configuration;
- automatic staging deployment locally on the existing EC2 host's self-hosted GitHub Actions runner, readiness and database-backed smoke checks, then production deployment of the same digest and production verification;
- failure blocking, non-overlapping deployments, and secrets outside tracked configuration;
- a complete source ZIP, the required README sections, a PDF or PPT with actual contributors and real evidence, and a verified delivery checklist.

AWS EC2 is the approved hosting target, selected on October 8, 2026. Keep this exercise within the existing AWS Free Plan and available credits; a paid-plan upgrade is outside scope. The user has confirmed instance launch, status checks, SSH access, host resources, and outbound registry connectivity in `us-east-2` (Ohio). User-supplied terminal output on October 9 confirms Docker/Compose installation and Docker access for the `ubuntu` deployment account. The user also confirmed the SSH host-key fingerprint matched the AWS system log on October 9. Manual staging/production startup, local API readiness, and the user's subsequent green automated staging run are recorded below; automated staging/production success and matching runtime image references are user-confirmed in task 4 below. See [docs/ci-cd-roadmap.md](docs/ci-cd-roadmap.md) for dated account observations, the proposed configuration, actual progress, and exit gates. The previous migration assignment remains historical context; its data and audit rules below remain architectural constraints.

Phase 3 task 4 is complete based on the user's October 9 output and confirmations: separate host environment files under `/home/ubuntu/carbono/staging/` and `/home/ubuntu/carbono/production/`, API ports `8081` and `8082`, different JWT signing keys, file permissions `600` owned by `ubuntu`, and successful quiet Compose configuration validation for both projects. Codex did not independently run these checks on EC2.

Phase 3 task 5 is complete based on the user's October 9 output: both `carbono-staging` and `carbono-production` show healthy API/MongoDB containers, with API host ports `8081` and `8082`; both host-local `/api/empresas` requests returned HTTP `200` and displayed seeded company `EMP-001`. MongoDB has no published host port in the supplied `ps` output. This is user-supplied runtime evidence, not independent EC2 verification by Codex. The shared `web-fiap-carbono:local` tag is not proof of a published or promoted image digest.

Phase 3 task 6 is complete based on the user's October 9 output and confirmations: the authorized local computer reached both `/api/empresas` URLs at EC2 public IPv4 address `18.222.63.49` on ports `8081` and `8082`, each returning HTTP `200`; both MongoDB containers returned no published mappings from `docker port`, and the external TCP probe of port `27017` timed out with exit code `1`. The supplied inbound rules allow TCP `22`, `8081`, and `8082` from the client's public IPv4 address with `/32`. Run external access checks on the user's computer and Docker checks inside EC2's SSH session; the rule source is the client's public IP, while the connection destination is EC2's public IP. Codex did not independently verify AWS or execute these host/network checks. Reconfirm the current EC2 public address before later access checks.

Phase 3 task 7 is complete based on the user's October 9 terminal output: the MongoDB containers mount distinct `carbono-staging_fiap_carbono_mongodb_data` and `carbono-production_fiap_carbono_mongodb_data` volumes; `carbono-staging_default` and `carbono-production_default` each contain only their own API/MongoDB pair. The temporary product `CRUD-TEMP-PHASE3-ISOLATION` was absent in both databases, inserted only in staging, confirmed present in staging and absent in production, and removed with `deletedCount: 1`; final matching counts were zero in both. Codex did not execute these EC2 checks. Together with tasks 4–6, this satisfies both Phase 3 exit-gate checks and completes the phase based on user-supplied evidence. Phase 4 progress is recorded below. Screenshots, automated deployment, and the later persistence/redeployment and deployment failure-blocking checks remain pending. Retain the environments and their persistent volumes for subsequent phases.

Phase 4 task 2 is complete based on Codex's read-only inspection of the existing [October 10 CI run](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38079294318) for `master` commit `f0f42c75c466bcef85cf9ed7625d81e0c1baccb2`. The Ubuntu 24.04 runner with Docker passed all 107 tests with zero failures or skipped tests. Existing test code supplies synthetic JWT settings and disposable MongoDB fixtures, including the API fixture's Linux host networking on port `27019`; no tracked actual environment/development configuration files or staging/production database connections are required. No implementation changes were needed for task 2, and Codex did not trigger that workflow. Successful `master` runs are independently verified; the user's subsequent October 10 confirmation of successful PR checks completes task 1's PR-trigger acceptance, as recorded with task 4 below. Subsequent task 3 completion and task 5's verified Docker Hub publication are recorded below. Both Phase 4 exit gates are now satisfied by the task 3 and task 6 records below; automated deployment remains pending. See the active roadmap for the run evidence and acceptance conditions.

Phase 4 task 3's CI retention changes are implemented in [ci.yml](.github/workflows/ci.yml): restore/build/tests retain normal failure behavior, and non-canceled runs record a summary of commit/run/stage outcomes and upload it with any generated TRX reports as `ci-results`, with fourteen-day retention. Successful tests without a nonempty TRX report fail the summary step. Local YAML/structure and shell checks passed, including eight shell scenarios with supplied step outcomes; these are not real GitHub runs or application test results. Codex's read-only inspection of [run 38085383384](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38085383384) on October 10 confirms successful summary creation, TRX creation, and artifact upload.

Phase 4 task 3 is complete on October 11 based on independently inspected failure/recovery runs and user-confirmed downloaded reports. In [run 38145711002](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38145711002), a `push` to `master` at commit `6b3b6aa9ccd637a38efa385a52eee6c25b627138`, restore/build succeeded and tests failed: 107 passed, 1 failed, 0 skipped, 108 total. The only failure was the temporary `CiFailureVerificationTest.ControlledFailureProducesDiagnostics`; summary creation, TRX creation, and `ci-results` upload succeeded, while **Build and publish application image** was skipped with no steps executed. The user removed the temporary test in commit `2fbd381667aae4fe73995c99a2519d2172d6f524`; [recovery run 38145909543](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38145909543) succeeded through CI and image publication. The user confirmed that the downloaded failed `ci.trx` contains the expected temporary failure and a downloaded successful PR report has zero failures. Codex did not download those reports; no successful PR/run identifier was supplied. The controlled failure occurred on `master`, although the exercise had been planned for a temporary PR; use a temporary PR for any future failure exercise. This completes the first Phase 4 exit gate. Subsequent task 6 verification below completes the second gate and Phase 4; deployment failure blocking and screenshots remain later work. Preserve `needs: ci` and successful CI on a `push` to `master` as the publication gate.

Phase 4 task 4 is complete based on independently inspected `master` execution and the user's October 10 repository/PR confirmations. The `image` job in [ci.yml](.github/workflows/ci.yml) requires successful `ci` on a push to `master`, logs in using `DOCKERHUB_USERNAME`/`DOCKERHUB_TOKEN`, and builds the root Dockerfile with root context, tagged `${DOCKERHUB_USERNAME}/web-fiap-carbono:${GITHUB_SHA}`. Codex independently inspected existing [run 38085383384](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38085383384), created October 10 at `20:51:29 UTC`, for `master` commit `0d59ad9c68fdd634d7093ba8047e4ab67a7299c9`: restore/build succeeded, all 107 tests passed with none failed/skipped, `ci-results` uploaded, and Docker Hub login and the image build succeeded after CI completed. Secret values were not read. The user confirmed creating the intended Docker Hub repository and, after checking the expected PR job statuses, confirmed successful **Restore, build and test** with **Build application image** skipped. This also completes task 1's remaining PR acceptance. Repository creation and PR results are user-confirmed; Codex's read-only PR-run listing returned no entries, and no PR/run link, commit, or screenshot was supplied, so do not describe the PR result as independently verified. Preserve that evidence distinction and collect the actual PR/run link and screenshots for the later evidence tasks. At that task 4 checkpoint, the build produced a runner-local image ID and no publication or deployment was verified. Subsequent task 5 verification below confirms registry publication and push permission. Task 3 and both Phase 4 exit gates were pending at that checkpoint. The October 11 task 3 record above completes its acceptance and the first gate; task 6 below completes the second gate. Automated deployment remains pending. Codex did not trigger the workflow or rerun builds/tests for that documentation update.

Phase 4 task 5 is complete. Its publication logic in [ci.yml](.github/workflows/ci.yml) retains the CI/`master` gate, pushes the already-built full-SHA tag, validates the registry digest, and exposes it as `needs.image.outputs.digest`. Local YAML/structure, Bash syntax, and nine synthetic push/error scenarios passed at implementation; those checks were not actual publication. Subsequent read-only inspection of [run 38090160366](https://github.com/guidonunes/Web.Fiap.Carbono/actions/runs/38090160366), created October 10 at `22:06:25 UTC` for `master` commit `6458819e04eecf1d35a13648a509210f90bf8fc1`, confirms successful CI followed by image login/build/publication. The push log recorded registry digest `sha256:78030c6e4e85e5fce8ce0be4766062e7f36c028e0442950e744795abeb5f3e13`, and GitHub logged `Set output 'digest'` without a skipped/redacted-output warning. After the user supplied repository `gui2506/web-fiap-carbono` and confirmed a matching digest, Codex independently queried its SHA tag using `docker buildx imagetools inspect`; the top-level registry digest matched exactly, with exit code `0`. PR publication exclusion retains the same inspected gate and earlier user-confirmed skipped image job; no new PR run was independently verified. Preserve the run, repository, SHA, digest, command/output, and evidence attribution recorded in the active roadmap. Task 3's failure/artifact verification and the first Phase 4 exit gate are now complete, as recorded above. Subsequent task 6 verification below completes the published-image acceptance, second exit gate, and Phase 4. Automated deployment remains Phase 5. These publication findings are prerequisite evidence for Phase 6, not completion of its full-flow checks or of Phase 5 deployments. Codex did not trigger the workflow, pull/run the image, deploy, capture screenshots, commit, push, or create a PR for this verification or documentation update.

Phase 4 task 6 and Phase 4 are complete based on the user's October 11 EC2 output and the preceding CI/publication evidence. The selected release is `gui2506/web-fiap-carbono:6458819e04eecf1d35a13648a509210f90bf8fc1`, digest `sha256:78030c6e4e85e5fce8ce0be4766062e7f36c028e0442950e744795abeb5f3e13`. Earlier `carbono-image-check` startup and the database-backed `http://127.0.0.1:8083/api/empresas` check returned HTTP `200`; the JSON array/seeded `EMP-001` assertion returned `true`. The user then ran the isolated image-content command, which printed `Application file checks passed` and exited `0`: the API DLL/dependency/runtime metadata files are nonempty, and no matching migration utility, environment, or development configuration files exist under `/app`. The scan covers the final application filesystem, not every image layer or arbitrary embedded secret. Codex did not execute these EC2 checks. Both Phase 4 exit gates are satisfied; retain the older release identifiers without substituting an uninspected recovery-run digest. Phase 5 tasks 1–4 progress is recorded below; task 2 has user-reported successful workflow execution, task 3 has local smoke-contract verification, and task 4 has user-confirmed successful production workflow execution and matching staging/production image references. Screenshots and full-flow, deployment failure-blocking, concurrency, and persistence/redeployment checks remain pending. Implement remaining Phase 5 tasks only when explicitly requested.

Phase 5 task 1 is complete on October 11 based on source inspection, local syntax/missing-argument checks, and the user's successful manual EC2 staging deployment. The user created [scripts/deploy.sh](scripts/deploy.sh) and [scripts/smoke.sh](scripts/smoke.sh); [docker-compose.yml](docker-compose.yml) now selects `APP_IMAGE` with `web-fiap-carbono:local` as its local default. The scripts validate inputs, require a full application digest, pull the API without rebuilding, wait for Compose health, and verify HTTP `200`, a JSON array, and `EMP-001`. Codex ran `bash -n` on both scripts (exit `0`); invoking each without arguments printed its usage and returned `1`, before any deployment. The user's EC2 command selected `carbono-staging`, its existing environment file, and digest `sha256:78030c6e4e85e5fce8ce0be4766062e7f36c028e0442950e744795abeb5f3e13`; the supplied output shows a successful pull, healthy API/MongoDB containers, `Smoke check passed: HTTP 200 and EMP-001`, `Deployment verified: carbono-staging`, and exit `0`. Codex did not execute this host deployment. For that manual check, files were authored locally and copied to EC2, with host commands run from `/home/ubuntu/carbono/repository`; automated jobs now use their matching runner checkout. At that manual staging checkpoint, tasks 2–7, both Phase 5 exit gates, production promotion, pipeline concurrency, and controlled deployment failures remained pending; subsequent task 2 and task 3 results are recorded below. At that task 1 documentation checkpoint, existing user implementation changes were preserved and only documentation was edited; the subsequent task 2 workflow implementation is recorded below.

Phase 5 task 2's staging workflow execution is complete based on the user's subsequent October 11 confirmation that all jobs ran successfully; Codex has not independently inspected that run. The user selected the existing EC2 host's self-hosted deployment runner after reporting an AWS advanced-feature upgrade requirement, and confirmed the repository is private. The user supplied successful registration for `carbono-ec2`, later found `carbono-deploy` missing while deployment was queued, added it, and then reported all jobs green after runner-service troubleshooting. The prescribed service account/path remain `ubuntu` and `/home/ubuntu/actions-runner`; no service-status output was supplied, and Codex did not independently inspect the service or runner availability. This authorized decision replaces automated deployment over SSH; manual SSH administration remains available with host-key verification, and no paid-plan upgrade or AWS OIDC setup is introduced.

[ci.yml](.github/workflows/ci.yml) keeps CI/publication on GitHub-hosted Ubuntu and adds `deploy_staging`, gated by successful `image` on a `master` push, with `[self-hosted, linux, x64, carbono-deploy]`. It checks out the exact `github.sha` without persistent Git credentials and passes `needs.image.outputs.digest` to the existing deployment script with staging's project, environment file, and loopback URL. Runtime secrets remain outside the runner checkout. Workflow-level `carbono-release-${{ github.ref }}` concurrency uses `cancel-in-progress: false`, sharing one group across `master` releases and separating PR refs; newer pending runs may replace older pending runs, while running releases are not canceled by a newer trigger. Task 4 below adds production in this same serialized workflow and on the same promoted digest; its successful execution is user-confirmed, while controlled failure/concurrency acceptance remains pending.

At task 2's implementation checkpoint, local YAML/structure checks, Bash syntax checks, and eight synthetic integration scenarios passed using mock Docker/curl/sleep and real `jq`, without live deployments or network requests. The user's subsequent all-jobs-green report completes successful staging execution acceptance. No new run URL, commit, registry digest, runtime-image comparison, or screenshot was supplied; collect those for release/evidence records and do not substitute Phase 4's older identifiers. At that task 2 acceptance checkpoint, production implementation, controlled deployment failures, concurrency runtime proof, persistence/redeployment, screenshots, and both Phase 5 exit gates remained pending. Task 4 implementation and subsequent user-supplied execution acceptance are recorded below. See the active roadmap for the checks and evidence attribution.

Phase 5 task 3 is complete for the smoke-check contract on October 11. The existing deployment/smoke scripts required no fixes. Added [scripts/verify-smoke.py](scripts/verify-smoke.py), which runs thirteen independent temporary loopback HTTP scenarios using Python's standard library and the real Bash/curl/jq smoke script. HTTP `200` with a JSON array and `EMP-001` passed; two initial `503` responses followed by recovery passed; incorrect status codes, redirects, HTML, malformed/multiple JSON values, wrong JSON shape, missing seeded data, refused connections, and request timeouts all returned `1`. The real timeout case exhausted ten attempts in `68.4s`. Temporary response files were removed and fixture response bodies were absent from logs. Bash/Python syntax checks and the permitted verifier run passed with exit `0`; the initial sandbox attempt was blocked by local socket permissions. Reproduce with `python3 scripts/verify-smoke.py` from the repository root; it requires Python 3, Bash, curl, and jq, takes about seventy seconds, and uses no Docker or application/database processes. These artificial local responses verify script behavior; the successful staging job is user-reported. No independent EC2/GitHub verification, application builds/tests, deployment, or screenshot is claimed. At that task 3 checkpoint, production automation was the next bounded implementation (task 4); its subsequent implementation and acceptance are recorded below. At that time, full pipeline blocking, runtime concurrency, live database-outage, persistence/redeployment, and both Phase 5 exit gates remained pending; task 4 below subsequently satisfies the first exit gate.

Phase 5 task 4 is complete based on the user's October 11 successful-workflow report and matching runtime image references below; real-run acceptance was pending at the implementation checkpoint. [ci.yml](.github/workflows/ci.yml) adds `deploy_production`, requiring successful `image` and `deploy_staging` on a `master` push, with the existing self-hosted deployment labels and ten-minute timeout. It checks out the exact `github.sha` without persistent Git credentials and passes the same `needs.image.outputs.digest` used by staging to `scripts/deploy.sh`, selecting `carbono-production`, `/home/ubuntu/carbono/production/production.env`, and `http://127.0.0.1:8082/api/empresas`. Existing CI/publication/staging jobs, workflow concurrency, deployment scripts, application, Compose, and database scripts are unchanged. YAML/structure and Bash syntax checks passed; twelve synthetic integration scenarios passed using the actual workflow shell blocks and deployment/smoke scripts in temporary checkouts with mock Docker/curl/sleep and real jq. These checked staging/production success with one supplied digest, invalid release inputs, missing configuration, Compose failures, pull/health failures, invalid HTTP/JSON, and missing seeded data. They made no live network/container/deployment changes. At implementation, task 4 and both Phase 5 exit gates remained unchecked pending a successful real production run and matching runtime digest checks. Subsequent user-supplied acceptance is recorded below. Codex did not run application builds/tests, access EC2, trigger workflows, deploy, provision resources, capture screenshots, commit, push, or create a PR.

Phase 5 task 4 acceptance is user-supplied: the user reported all workflow jobs green, then provided EC2 `docker inspect` output showing both `carbono-staging-api-1` and `carbono-production-api-1` using `gui2506/web-fiap-carbono@sha256:4deaaaa81939cf9cc27ac140cc993631139b576d281ad991452170e92a1e4da7`. The matching references and successful-job report, together with both jobs reading `needs.image.outputs.digest`, satisfy successful production deployment and same-image promotion acceptance. Passing smoke checks are inferred from the reported successful jobs and the fail-fast scripts; no new raw smoke log was supplied. Codex did not independently inspect the GitHub run, EC2 state, or registry metadata. No new run URL, run commit, or screenshot was supplied; collect them for the evidence/release record without substituting Phase 4's older identifiers. Task 4 and the first Phase 5 exit gate are complete based on this evidence; tasks 5–7, the second exit gate, and Phase 6 evidence remain pending. The next bounded task is task 5's failure-blocking and concurrency verification. This acceptance update changes only documentation and runs no builds, tests, database scripts, deployments, or remote checks. Phase 5 remains in progress.

## Sources of truth

Use the following files in this order:

1. The user's current request.
2. This [AGENTS.md](AGENTS.md) file.
3. [docs/ci-cd-roadmap.md](docs/ci-cd-roadmap.md) for active CI/CD phase order, tasks, verification, and exit gates.
4. [README.md](README.md) for documented application behavior and setup.
5. [roadmap.md](roadmap.md) and [docs/mongodb-migration.md](docs/mongodb-migration.md) for migration history and architectural references, not CI/CD sequencing.

The actual source, configuration, and observed verification results determine what is implemented. Historical documents contain checkpoint-specific descriptions; the migration report still describes an older hybrid Oracle/MongoDB state that differs from the current API source. Do not reintroduce Oracle or treat historical completion claims as current CI/CD results.

Documentation can describe planned work. Never assume a roadmap checkbox or design section proves that code has been implemented. Inspect the repository and run the relevant verification before reporting a phase as complete. Honor task-specific limits on execution; when verification is prohibited or unavailable, report it as not run. Never invent test results, screenshots, deployment status, contributors, or evidence.

Distinguish proposed infrastructure, user-confirmed results, and independently observed verification. Attribute user-reported host checks explicitly; a provisioned EC2 instance does not prove Docker, either environment, or the pipeline works.

If two project documents conflict:

- follow the user's latest explicit decision;
- preserve the five-collection MongoDB design;
- prefer the more specific document;
- report the conflict before making a change that would materially alter the architecture.

## Required working style

### Guidance-first default

When the user asks a question, requests a review, or asks what to do next:

1. Inspect the relevant code and documentation.
2. Explain the current state.
3. Identify the next bounded task.
4. Provide a short implementation checklist.
5. Explain how the user can verify the result.
6. Stop without editing files.

Examples that authorize guidance and inspection only:

- “What do I implement next?”
- “How should I implement Phase 4?”
- “Is this step complete?”
- “What is wrong with this code?”
- “Review my implementation.”
- “Give me the tasks for this phase.”
- “Explain this workflow.”
- “Show me a snippet.”
- “Help me troubleshoot this deployment.”

Requests for explanations, snippets, reviews, or troubleshooting authorize guidance only. Explain the purpose of proposed changes and provide practical verification commands with expected results. Read-only commands, test runs, builds, status checks, and repository inspection are allowed when needed to answer accurately unless the current request restricts them. Do not silently fix problems discovered during inspection or execute deployments as part of guidance.

### Explicit implementation requirement

Modify the repository only when the user clearly requests a change.

Examples of explicit authorization:

- “Implement Phase 3.”
- “Create the CI workflow.”
- “Fix the failing repository test.”
- “Complete `docker-compose.yml`.”
- “Update `docs/ci-cd-roadmap.md`.”

An implementation request authorizes only the named task and the minimum supporting changes required to make it correct and verifiable. It does not authorize future roadmap phases or unrelated refactoring.

When implementation is explicitly requested:

1. Restate the bounded outcome briefly.
2. Inspect the affected files and working tree.
3. Make the smallest coherent change.
4. Add or update relevant tests.
5. Run focused verification.
6. Run broader verification when the change crosses application boundaries.
7. Report what changed, what passed, and what remains.

Do not ask for a second confirmation when the user's implementation request is already unambiguous.

### One phase at a time

Follow [docs/ci-cd-roadmap.md](docs/ci-cd-roadmap.md) in order for CI/CD work unless the user explicitly changes the order. The numbered phases in `roadmap.md` apply only when the user explicitly requests historical migration work.

If the user requests a numbered phase or task:

- resolve its exact scope from the active CI/CD roadmap, or the historical roadmap when migration work is explicitly requested;
- implement only that scope;
- satisfy its exit gate before calling it complete;
- do not begin the next phase automatically.

If a prerequisite is missing, explain the dependency. Implement it only if it is a small, necessary part of the authorized task; otherwise ask the user how to proceed.

## Locked architectural decisions

Do not change these decisions without explicit user approval.

### Five ESG collections

The application database is `fiap_carbono` and contains exactly these five domain collections. Staging and production may use the same database name because each has its own MongoDB instance and storage:

| Collection | Responsibility |
| --- | --- |
| `empresas` | Company identity, ESG targets, and governance metadata |
| `produtos` | Products and flexible environmental attributes |
| `fornecedores` | Supplier environmental, social, certification, and audit data |
| `fatores_emissao` | Versioned emission factors, sources, units, scopes, and validity |
| `emissoes_carbono` | Auditable emission events with embedded calculation context |

Do not create separate MongoDB collections for:

- production batches;
- supply-chain stages;
- authentication users;
- dashboards or aggregation results.

Authentication users remain in the existing in-memory `AuthService` for this assignment unless the user explicitly changes that decision.

### Mapping from Oracle

| Oracle source | MongoDB target |
| --- | --- |
| `EC_EMPRESAS` | `empresas` |
| `EC_PRODUTOS` | `produtos` |
| `EC_FORNECEDORES` | `fornecedores` |
| `EC_FATORES_EMISSAO` | `fatores_emissao` |
| `EC_EMISSOES_CARBONO` | `emissoes_carbono` |
| `EC_LOTES_PRODUCAO` | Embedded `emissoes_carbono.lote` snapshot |
| `EC_ETAPAS_CADEIA` | Embedded `emissoes_carbono.etapa` snapshot |

### Embedding and referencing

- Products reference companies with `empresaId`.
- Emissions reference company, product, supplier, and factor documents with `ObjectId` values.
- Emissions embed `lote`, `etapa`, `dadosAtividade`, and `fatorAplicado`.
- `fatorAplicado` is an immutable historical snapshot of the factor used in the calculation.
- Never embed an unbounded emissions array inside a company, product, or supplier.
- Do not replace the factor snapshot with a runtime lookup during historical reads.

### Emission calculation

Preserve the business formula:

```text
quantidadeEmitidaKgCO2e = quantidadeAtividade × fatorAplicado.valor
```

Use decimal arithmetic. Do not use `double` for emission factors, activity quantities, or calculated `kgCO2e` values.

The activity quantity must match the selected factor's base unit. Do not silently perform a unit conversion unless a separately designed and tested conversion rule is explicitly added.

### Historical Oracle removal gate

The earlier migration required the following MongoDB parity conditions before removal of Oracle persistence from the API:

- support for the required CRUD operations;
- preservation of the emission-calculation workflow;
- implementation of all required analytics;
- passing MongoDB integration tests;
- passing migration reconciliation or a documented seed-only approach;
- satisfaction of the historical Phase 14 preconditions in [roadmap.md](roadmap.md).

This gate does not sequence new CI/CD work. Source inspection shows MongoDB registrations in the API and Oracle dependencies in the separate migration utility. Keep the previous Oracle implementation recoverable through Git history and preserve the utility and its linked archive files; do not delete them as unrelated CI/CD cleanup.

### CI/CD boundaries

- Use GitHub Actions, Docker Hub, and Docker Compose on one Ubuntu AWS EC2 deployment host. The hosting decision does not introduce CodePipeline, CodeBuild, ECR, ECS, EKS, or a managed database.
- Keep CI/tests and image publication on GitHub-hosted Ubuntu runners. Use the existing EC2 host's deployment-only self-hosted runner for successful `master` releases, selected with `[self-hosted, linux, x64, carbono-deploy]`; do not run PR jobs on it. Execute matching-revision deployment scripts locally, with runtime environment files outside the checkout. Automated deployment does not require AWS credentials or opening SSH to GitHub-hosted runners.
- Keep staging and production in distinct Compose projects, with independent API/MongoDB services, volumes, networks, and untracked environment configuration; proposed API host ports are `8081` and `8082`, respectively.
- Run existing restore/build/tests for pull requests and gate image publication on successful checks for pushes to `master`.
- Build once, tag with the commit SHA, capture the published digest, verify staging, and promote that same digest to production without rebuilding.
- Verify readiness and a database-backed API response in both environments; fail subsequent stages when a prerequisite fails.
- Serialize the full deployment sequence across runs. Do not overlap deployments or cancel a running deployment midway to start another.
- Keep credentials in GitHub Secrets or untracked environment files. Do not bake secrets into images or expose them in logs or evidence.
- Initialize a fresh database safely using the existing collection/index/seed scripts in order. Do not rerun the replacing seed on existing data at every startup or deployment; do not mount CRUD demonstration scripts as startup hooks.
- Do not add Kubernetes, extra hosts, new business features, or framework upgrades to satisfy this assignment.

## MongoDB data rules

Apply these conventions consistently:

- use MongoDB `_id` values of BSON type `ObjectId`;
- expose IDs as strings in API DTOs only where that matches the existing API conventions;
- reject malformed IDs as client validation errors;
- store dates as UTC BSON dates;
- store CNPJ as a string, never as a number;
- use BSON `Decimal128` for factors, quantities, percentages, and emission totals;
- store stable enum values as descriptive strings;
- include `schemaVersion` on documents expected to evolve;
- add `legacyId` to documents migrated from Oracle;
- preserve `criadoEm` and `atualizadoEm` timestamps;
- use unique indexes for business keys such as CNPJ and factor code/version;
- validate cross-collection references in the application service before writes.

### Flexible schema

Use schema flexibility only when it represents a real domain difference.

The main flexible area is `emissoes_carbono.dadosAtividade`, which may vary by activity type:

- `TRANSPORTE`: distance, load, and fuel;
- `ENERGIA`: consumption, source, and renewable percentage;
- `MATERIA_PRIMA`: material, weight, and recycled percentage;
- `RESIDUO`: class, weight, treatment, and destination distance.

Keep shared emission fields stable. Do not scatter unrelated optional fields across the document root merely to demonstrate flexibility.

### Validators

JSON Schema validators should enforce only stable invariants:

- required identity and reference fields;
- core BSON types;
- calculation fields;
- audit timestamps;
- stable enumerations;
- `schemaVersion`.

Do not use an unnecessarily rigid validator that prevents legitimate `dadosAtividade` variants or controlled future fields.

### Indexes

Indexes must correspond to real constraints and access patterns. At minimum preserve:

- unique company CNPJ;
- unique supplier CNPJ;
- unique product code within a company;
- unique factor code and version;
- emission queries by product and date;
- emission queries by company and date;
- emission queries by supplier and date;
- factor lookup from emissions;
- supply-chain category filtering.

Do not add speculative indexes without explaining which query or constraint they support.

## Database scripts

Keep MongoDB scripts under:

```text
database/mongodb/
├── 01-create-collections.js
├── 02-create-indexes.js
├── 03-seed.js
├── 04-crud-demo.js
└── 05-aggregation-queries.js
```

Responsibilities:

| Script | Responsibility |
| --- | --- |
| `01-create-collections.js` | Create the five collections and their validators |
| `02-create-indexes.js` | Create unique and query-supporting indexes |
| `03-seed.js` | Insert a coherent dataset with at least ten persistent documents per collection |
| `04-crud-demo.js` | Demonstrate create, read, update, second read, delete, and final counts for every collection |
| `05-aggregation-queries.js` | Demonstrate product footprint, supplier ranking, and company dashboard pipelines |

Script requirements:

- use deterministic business keys to resolve generated `ObjectId` references;
- keep the seed internally coherent;
- avoid unrelated documents created only to reach the required count;
- make repeated execution predictable or document when a clean development database is required;
- never drop a database during normal application startup;
- never include unrestricted destructive commands in shared scripts;
- clean up only explicitly named `CRUD-TEMP` demonstration records;
- finish the CRUD demo with at least ten persistent documents in every collection.

Recommended execution order:

```bash
mongosh "mongodb://localhost:27017/fiap_carbono" \
  --file database/mongodb/01-create-collections.js

mongosh "mongodb://localhost:27017/fiap_carbono" \
  --file database/mongodb/02-create-indexes.js

mongosh "mongodb://localhost:27017/fiap_carbono" \
  --file database/mongodb/03-seed.js

mongosh "mongodb://localhost:27017/fiap_carbono" \
  --file database/mongodb/04-crud-demo.js

mongosh "mongodb://localhost:27017/fiap_carbono" \
  --file database/mongodb/05-aggregation-queries.js
```

## .NET implementation conventions

### Project organization

Follow the repository's existing conventions where they are coherent. The API and migration utility are under `src/`, while `Web.Fiap.Carbono.Tests/`, the solution, and `Dockerfile` remain at the root. Preserve namespaces and the internal API structure:

```text
src/Web.Fiap.Carbono/
├── Config/MongoDb/
├── Data/MongoDb/
│   └── Repositories/
├── Models/Documents/
│   └── Embedded/
├── Controllers/
├── Services/
├── ViewModel/
└── Program.cs
```

Do not perform a wholesale namespace or directory rename as part of a persistence task.

### MongoDB client and configuration

- use the official `MongoDB.Driver` package;
- bind `MongoDbSettings` through the ASP.NET Core options pattern;
- register one reusable `MongoClient` for the application;
- do not create a new client per request or repository operation;
- expose typed `IMongoCollection<TDocument>` instances through a focused context or provider;
- support `MongoDb__ConnectionString` and `MongoDb__DatabaseName` configuration;
- never commit real credentials or connection strings containing secrets.

### Document models and DTOs

- keep MongoDB document models separate from API request and response DTOs;
- avoid leaking driver-specific types into public API contracts unless intentional;
- use explicit BSON mappings when property names or representations differ;
- use `[BsonIgnoreExtraElements]` where forward-compatible reads are appropriate;
- represent embedded snapshots with dedicated value-object classes;
- keep API validation at the boundary and business validation in services.

### Repositories

Create focused repositories for:

```text
Empresa
Produto
Fornecedor
FatorEmissao
EmissaoCarbono
```

Standard repository behavior may include:

```text
CreateAsync
GetByIdAsync
GetAllAsync
UpdateAsync
DeleteAsync
ExistsAsync
```

Emission-specific repositories also expose explicit query methods for:

- pagination;
- product footprint;
- supplier ranking;
- company dashboard.

Do not reproduce a generic EF Core repository abstraction that hides filters and aggregation pipelines. MongoDB-specific query behavior should remain readable and testable.

### Async code

- use asynchronous MongoDB driver APIs;
- accept and propagate `CancellationToken` where practical;
- avoid `.Result`, `.Wait()`, or other sync-over-async patterns;
- keep controller actions thin;
- keep business rules in services;
- keep query construction in repositories or dedicated query components.

### Naming and language

- preserve established domain names such as `Empresa`, `Produto`, and `EmissaoCarbono`;
- use consistent Portuguese domain terminology rather than mixing synonyms;
- follow existing C# naming conventions;
- write code, identifiers, and repository documentation in the language already established by the surrounding file;
- answer the user in the language used in the current request unless requested otherwise.

## API behavior

Preserve existing analytics routes where practical:

```text
GET /api/emissoes-carbono
GET /api/emissoes-carbono/{id}
GET /api/produtos-carbono/{id}/pegada
GET /api/fornecedores-carbono/ranking
GET /api/dashboard-carbono/empresas/{id}/resumo
```

Preserve the existing resource contracts. Companies, products, suppliers, and factors use these CRUD routes:

```text
POST   /api/{resource}
GET    /api/{resource}
GET    /api/{resource}/{id}
PUT    /api/{resource}/{id}
DELETE /api/{resource}/{id}
```

Emission creation uses `POST /api/emissoes-carbono/calcular`; ObjectId read/update/delete use `/api/emissoes-carbono/mongodb/{id}`. Preserve the legacy-ID read route and the restrictions on temporary emission mutations. Do not replace these contracts merely to regularize routing for CI/CD.

Authorization baseline:

- reads follow the current public behavior or the explicitly selected policy;
- create and update require `ADMIN` or `ANALISTA_ESG`, except factor writes, which currently require `ADMIN`;
- delete requires `ADMIN`;
- factor activation or deactivation requires `ADMIN`.

Maintain consistent error handling:

| Condition | Expected response |
| --- | --- |
| Invalid request or malformed `ObjectId` | `400 Bad Request` |
| Missing referenced document | `404 Not Found` |
| Duplicate indexed business key | `409 Conflict` |
| Inactive, expired, or incompatible emission factor | `422 Unprocessable Entity` |
| Missing or invalid authentication | `401 Unauthorized` |
| Authenticated but forbidden role | `403 Forbidden` |

Use the existing global exception middleware instead of duplicating error-response logic in every controller.

### Emission immutability

The previous migration assignment required an update and delete demonstration for `emissoes_carbono`. Preserve the isolated `CRUD-TEMP` restriction for that evidence and any CI/CD smoke-test mutations.

For normal application behavior, treat calculated emission inputs, result, and `fatorAplicado` as immutable audit data. Corrections should create a replacement, revision, or reversal record with explicit audit metadata rather than silently rewriting history.

## Analytics requirements

Implement analytics with MongoDB aggregation pipelines.

### Product footprint

Filter by `produtoId`, group by `etapa.categoria`, calculate category totals, and calculate the overall product total.

### Supplier ranking

Group by `fornecedorId`, sum `quantidadeEmitidaKgCO2e`, apply deterministic descending ordering, paginate, and resolve supplier display data.

### Company dashboard

Filter by `empresaId` and use `$facet` to produce multiple summaries from the same event set:

- overall total and record count;
- emissions by month;
- emissions by product;
- emissions by supplier;
- emissions by GHG scope.

Use decimal-safe totals and deterministic sorting. Keep API pagination one-based if that is the existing contract, even if MongoDB `$skip` is zero-based internally.

## Testing requirements

### Test environment

Do not use EF Core InMemory as proof of MongoDB behavior.

Persistence and API integration tests must run against an isolated disposable MongoDB instance, preferably with a container-based test fixture. Tests must not depend on a developer's permanent local database or mutate shared data.

### Minimum coverage

Add or preserve tests for:

- CRUD for all five collections;
- unique company and supplier CNPJ constraints;
- unique product code within a company;
- unique factor code and version;
- malformed `ObjectId` handling;
- missing cross-collection references;
- inactive or expired factor rejection;
- correct decimal emission calculation;
- preservation of the applied-factor snapshot;
- product-footprint totals;
- supplier-ranking order and pagination;
- company-dashboard facets;
- authentication and authorization;
- pagination limits and empty results.

### Verification commands

Use focused tests during application changes, then run full solution verification for a baseline, repository reorganization, or changes crossing application boundaries. Also execute the active phase's container/pipeline/environment checks before declaring it complete:

```bash
dotnet restore Web.Fiap.Carbono.sln
dotnet build Web.Fiap.Carbono.sln
dotnet test Web.Fiap.Carbono.sln
```

The existing API fixture uses Linux host networking and port `27019`; verify runner compatibility and avoid conflicting test runs. Ensure clean-checkout tests receive synthetic JWT configuration without relying on an ignored developer configuration file. Oracle is needed only for explicit real-source migration operations, not normal API deployment or synthetic migration tests.

If a command cannot run because MongoDB, Docker, credentials, or another dependency is unavailable, report the exact blocker. Do not claim the phase passes based only on code inspection. Documentation-only work uses diff and path checks; do not run builds, tests, database scripts, or deployments when the request excludes them.

## Historical migration and reconciliation reference

These rules apply when explicitly working on migration records. They do not require rerunning Oracle migration for CI/CD. Preserve [docs/oracle-baseline.md](docs/oracle-baseline.md) and [docs/oracle-mongodb-reconciliation.md](docs/oracle-mongodb-reconciliation.md) as historical evidence.

When migrating existing Oracle records:

1. migrate companies;
2. migrate products and map company IDs;
3. migrate suppliers;
4. migrate emission factors;
5. join emissions with batches, stages, products, companies, suppliers, and factors;
6. build denormalized emission documents;
7. preserve Oracle primary keys as `legacyId`;
8. reconcile counts, totals, and date ranges.

Required reconciliation includes:

- count per entity;
- total `kgCO2e`;
- totals per company;
- totals per product;
- totals per supplier;
- earliest emission date;
- latest emission date.

If the Oracle database has no useful data, document that the project migrates the domain and persistence design and uses `03-seed.js` as the demonstration dataset.

## Documentation and evidence

Keep these artifacts synchronized with the implementation:

- `README.md` — setup, configuration, architecture, commands, API, and tests;
- [docs/ci-cd-roadmap.md](docs/ci-cd-roadmap.md) — active CI/CD phases, exit gates, and submission checklist;
- `docs/ci-cd-baseline.md` — planned record of actual Phase 1 commands and results; create when that phase is executed;
- [roadmap.md](roadmap.md) — preserved migration history, not active CI/CD sequencing;
- [docs/mongodb-migration.md](docs/mongodb-migration.md) — historical migration report and data-model reference;
- `database/mongodb/*.js` — executable database evidence;
- `docs/images/mongodb/` — historical migration screenshot destination;
- `docs/images/ci-cd/` — planned destination for real pipeline and environment screenshots.

Make documentation changes only within the requested scope. Leave baseline results and README changes for their assigned implementation phases. The final README must use the title **Projeto - Cidades ESGInteligentes** and the exact sections and delivery checklist specified in the active roadmap.

Do not mark a roadmap checkbox complete merely because a file exists. Mark it complete only after the acceptance condition has been verified.

### Screenshot rules

- capture actual execution, including successful operations and controlled failures that prove pipeline blocking;
- show the relevant command and result;
- keep the relevant workflow/run, commit, environment, or database/collection name visible;
- use readable zoom and crop irrelevant desktop content;
- do not expose passwords, tokens, connection strings, personal information, or signing keys;
- use the CI/CD evidence filenames in `docs/ci-cd-roadmap.md`; for historical migration evidence, follow `roadmap.md` and `docs/mongodb-migration.md`;
- add a short explanatory caption below each screenshot;
- do not generate fake screenshots or placeholder results.

## Change safety

- inspect `git status` before editing;
- preserve unrelated user changes;
- avoid broad formatting or refactoring outside the requested task;
- preserve the migration utility and linked Oracle archive files during reorganization; do not rewrite or delete historical artifacts as unrelated cleanup;
- do not edit an already-applied Oracle or MongoDB migration artifact without explaining the consequences;
- never commit secrets;
- never add real credentials to examples;
- do not drop a database, delete persistent data, or reset the repository unless the user explicitly requests that exact action;
- prefer temporary `CRUD-TEMP` records for destructive demonstrations;
- do not commit, push, merge, or open a pull request unless explicitly asked;
- do not use destructive Git commands to discard user work.

When the working tree already contains changes, distinguish the user's existing work from changes made for the current task.

## Review behavior

When asked to review code:

1. inspect the implementation and its tests;
2. run relevant read-only verification where possible;
3. lead with concrete findings, ordered by severity;
4. cite affected files and explain the failure mode;
5. identify missing test coverage;
6. do not implement fixes unless explicitly requested.

Review for:

- broken business rules;
- incorrect BSON representations;
- loss of decimal precision;
- invalid reference handling;
- mutable historical snapshots;
- inconsistent API status codes;
- aggregation and pagination errors;
- missing indexes or mismatched query patterns;
- accidental sixth collections;
- unsafe seed or reset behavior;
- insufficient integration tests;
- documentation that claims unexecuted results.

## Completion and handoff format

After an implementation task, report:

1. **Outcome:** what is now working.
2. **Changed files:** only files changed for the task.
3. **Verification:** commands run and their results.
4. **Exit gate:** whether the requested roadmap task is complete.
5. **Remaining work:** the next task or any blocker, without implementing it automatically.

Do not call a phase complete if tests fail, required commands were not run, evidence is missing, or an acceptance criterion remains unmet.

## Definition of done for the CI/CD assignment

The assignment is complete only when the active roadmap's exit gates and mandatory delivery checklist are verified:

- the reorganized .NET 8 solution builds and existing tests pass with disposable MongoDB;
- the root Dockerfile and `docker-compose.yml` run the API and MongoDB with safe initialization, readiness checks, and persistent storage;
- staging and production operate independently on the prepared Ubuntu EC2 host;
- pull requests run CI, successful `master` pushes publish a SHA-tagged Docker Hub image, and serialized deployments promote the same digest through verified staging to verified production;
- failure blocking, database-backed smoke checks, persistence, and isolation have real execution evidence;
- business behavior, JWT authorization, decimal arithmetic, historical snapshots, and the five-collection design remain preserved;
- the source ZIP, README, PDF or PPT with actual contributors, real screenshots, and completed delivery checklist match the delivered implementation;
- no credentials or invented results appear in source, images, logs, or submission artifacts.

The migration's historical definition of done remains in `roadmap.md`; do not mark its outstanding evidence complete on the basis of CI/CD work.
