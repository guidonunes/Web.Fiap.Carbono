# AGENTS.md

## Purpose

This file defines how coding agents must work in the **Web.Fiap.Carbono** repository while the application is migrated from Oracle Database and Entity Framework Core to MongoDB.

The user is completing this project as a learning exercise. Agents must help the user understand and implement the migration incrementally. The default behavior is to inspect, explain, and propose tasks. Do not edit code or files unless the user explicitly asks for implementation.

These instructions apply to the entire repository unless a more specific `AGENTS.md` exists in a subdirectory. Direct instructions from the user take precedence over this file.

## Project context

Web.Fiap.Carbono is a .NET 8 ASP.NET Core Web API that measures and analyzes greenhouse-gas emissions across product supply chains.

The original application uses:

- C# and .NET 8;
- ASP.NET Core Web API;
- Oracle Database;
- Entity Framework Core with `Oracle.EntityFrameworkCore`;
- JWT authentication;
- AutoMapper;
- xUnit integration-style API tests.

The target application replaces Oracle persistence with MongoDB while preserving the important API behavior, emission calculation, validation, analytics, authentication, authorization, and error responses.

The FIAP assignment requires:

- the **Migration** challenge option;
- exactly five MongoDB ESG domain collections;
- at least ten persistent documents per collection;
- complete CRUD operations for every collection;
- a meaningful flexible-schema demonstration;
- creation, index, seed, CRUD, and aggregation commands;
- screenshots proving the executed operations;
- clear technical documentation.

## Sources of truth

Use the following files in this order:

1. The user's current request.
2. This `AGENTS.md` file.
3. `roadmap.md` for phase order, tasks, verification, and exit gates.
4. `mongodb-migration.md` for the target data model and architectural decisions.
5. `README.md` for the current application behavior and setup.
6. The actual source code and passing tests for the implemented state.

Documentation can describe planned work. Never assume a roadmap checkbox or design section proves that code has been implemented. Inspect the repository and run the relevant verification before reporting a phase as complete.

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

Read-only commands, test runs, builds, status checks, and repository inspection are allowed when needed to answer accurately. Do not silently fix problems discovered during inspection.

### Explicit implementation requirement

Modify the repository only when the user clearly requests a change.

Examples of explicit authorization:

- “Implement Phase 3.”
- “Create `03-seed.js`.”
- “Fix the failing repository test.”
- “Replace the Oracle repository with MongoDB.”
- “Update `mongodb-migration.md`.”

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

Follow `roadmap.md` in order unless the user explicitly changes the order.

If the user requests a numbered phase or task:

- resolve its exact scope from `roadmap.md`;
- implement only that scope;
- satisfy its exit gate before calling it complete;
- do not begin the next phase automatically.

If a prerequisite is missing, explain the dependency. Implement it only if it is a small, necessary part of the authorized task; otherwise ask the user how to proceed.

## Locked architectural decisions

Do not change these decisions without explicit user approval.

### Five ESG collections

The target database is `fiap_carbono` and contains exactly these five domain collections:

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

### Oracle removal gate

Do not remove Oracle or Entity Framework Core persistence until the MongoDB implementation:

- supports the required CRUD operations;
- preserves the emission-calculation workflow;
- implements all required analytics;
- passes its MongoDB integration tests;
- passes migration reconciliation or documents the seed-only approach;
- satisfies the Phase 14 preconditions in `roadmap.md`.

Keep the previous Oracle implementation recoverable through Git history.

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

Follow the repository's existing conventions where they are coherent. The target persistence structure may use:

```text
Web.Fiap.Carbono/
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

The five resources must eventually expose CRUD endpoints:

```text
POST   /api/{resource}
GET    /api/{resource}
GET    /api/{resource}/{id}
PUT    /api/{resource}/{id}
DELETE /api/{resource}/{id}
```

Authorization baseline:

- reads follow the current public behavior or the explicitly selected policy;
- create and update require `ADMIN` or `ANALISTA_ESG`;
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

The academic assignment requires an update and delete demonstration for `emissoes_carbono`. Use isolated `CRUD-TEMP` records for that evidence.

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

Use focused tests during implementation, then run the full solution verification before declaring a phase complete:

```bash
dotnet restore Web.Fiap.Carbono.sln
dotnet build Web.Fiap.Carbono.sln
dotnet test Web.Fiap.Carbono.sln
```

If a command cannot run because MongoDB, Docker, Oracle, credentials, or another dependency is unavailable, report the exact blocker. Do not claim the phase passes based only on code inspection.

## Migration and reconciliation

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
- `roadmap.md` — implementation phases and exit gates;
- `mongodb-migration.md` — technical decision and FIAP report;
- `database/mongodb/*.js` — executable database evidence;
- `docs/images/mongodb/` — screenshots of executed operations.

Do not mark a roadmap checkbox complete merely because a file exists. Mark it complete only after the acceptance condition has been verified.

### Screenshot rules

- capture actual successful execution;
- show the relevant command and result;
- keep the database and collection name visible;
- use readable zoom and crop irrelevant desktop content;
- do not expose passwords, tokens, connection strings, personal information, or signing keys;
- use the filenames defined in `roadmap.md` and `mongodb-migration.md`;
- add a short explanatory caption below each screenshot;
- do not generate fake screenshots or placeholder results.

## Change safety

- inspect `git status` before editing;
- preserve unrelated user changes;
- avoid broad formatting or refactoring outside the requested task;
- do not rewrite or delete Oracle files before the removal gate;
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

## Definition of done for the migration

The full migration is complete only when:

- the application uses MongoDB as its active persistence layer;
- exactly five ESG collections are used;
- every collection contains at least ten coherent persistent documents;
- CRUD has been executed and evidenced for every collection;
- flexible activity documents are demonstrated meaningfully;
- validators and indexes are applied;
- emission calculation and historical snapshots are correct;
- all analytics use verified aggregation pipelines;
- integration tests pass against MongoDB;
- Oracle data is reconciled or the seed-only decision is documented;
- Oracle dependencies are removed only after parity;
- README and migration documentation match the committed implementation;
- required screenshots are included without exposing secrets.
