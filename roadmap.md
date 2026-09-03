# Web.Fiap.Carbono — MongoDB Migration Roadmap

## 1. Purpose

This roadmap guides the migration of **Web.Fiap.Carbono** from Oracle Database and Entity Framework Core to MongoDB while preserving the existing REST API's carbon-emission calculation, product-footprint analysis, supplier ranking, company dashboard, authentication, validation, and error-handling behavior.

The project will use the **Migration** option from the FIAP challenge. The final result must demonstrate a genuine document-oriented design, five MongoDB collections, at least ten persistent documents per collection, complete CRUD operations, flexible document structures, aggregation pipelines, technical documentation, and screenshots of the executed operations.

## 2. Working method

Complete the phases in order. Do not remove the Oracle implementation until the MongoDB implementation has reached functional parity and its tests pass.

For every numbered task:

1. Read the goal and acceptance criteria.
2. Implement only that task.
3. Run the listed verification.
4. Correct failures before continuing.
5. Mark the task and its exit gate as complete.

Use a dedicated branch:

```bash
git switch -c feature/mongodb-migration
```

## 3. Scope

### Required

- Replace Oracle persistence with MongoDB.
- Implement exactly five ESG domain collections.
- Store at least ten persistent documents in each collection.
- Demonstrate create, read, update, and delete operations in every collection.
- Demonstrate meaningful schema flexibility.
- Preserve the emission formula:

```text
emitted quantity (kgCO2e) = activity quantity × emission factor value
```

- Preserve or adapt the existing analytics endpoints.
- Implement MongoDB aggregation pipelines for analytics.
- Add database initialization, index, seed, CRUD-demonstration, and aggregation scripts.
- Replace EF Core InMemory persistence tests with MongoDB integration tests.
- Document the migration decisions and collect the required screenshots.

### Not required for this migration

- A web or mobile frontend.
- Replacing JWT authentication with a new identity system.
- Creating a sixth collection for demonstration users.
- Real-time ingestion, messaging, or change streams.
- Production deployment or a MongoDB Atlas account unless required by the professor.

The two demonstration users may remain in the current in-memory `AuthService`. They do not belong to the five ESG collections.

## 4. Target data model

The seven Oracle tables will become five MongoDB collections.

| Oracle structure | MongoDB collection | Strategy |
| --- | --- | --- |
| `EC_EMPRESAS` | `empresas` | One company per document |
| `EC_PRODUTOS` | `produtos` | One product per document with a reference to its company |
| `EC_FORNECEDORES` | `fornecedores` | One supplier per document with flexible ESG and certification data |
| `EC_FATORES_EMISSAO` | `fatores_emissao` | Versioned emission-factor documents |
| `EC_EMISSOES_CARBONO`, `EC_LOTES_PRODUCAO`, and `EC_ETAPAS_CADEIA` | `emissoes_carbono` | One emission event per document with embedded batch, stage, and applied-factor snapshots |

### 4.1 Collection responsibilities

#### `empresas`

Stores company identification, reporting configuration, reduction targets, governance contacts, and audit metadata.

Minimum stable fields:

```text
_id
razaoSocial
nomeFantasia
cnpj
setor
ativa
metasReducao[]
governanca
criadoEm
atualizadoEm
schemaVersion
```

#### `produtos`

Stores products associated with a company and flexible environmental attributes such as composition, recyclability, packaging, and sustainability labels.

Minimum stable fields:

```text
_id
empresaId
codigo
nome
categoria
unidadeFuncional
atributosAmbientais
ativo
criadoEm
atualizadoEm
schemaVersion
```

#### `fornecedores`

Stores supplier identity, operational categories, certifications, labor indicators, environmental compliance, and ESG-audit information.

Minimum stable fields:

```text
_id
razaoSocial
nomeFantasia
cnpj
categoriasAtuacao[]
certificacoes[]
indicadoresSociais
conformidadeAmbiental
statusAuditoria
ativo
criadoEm
atualizadoEm
schemaVersion
```

#### `fatores_emissao`

Stores emission factors and the technical information required to use and audit them.

Minimum stable fields:

```text
_id
codigo
nome
categoria
valor
unidadeBase
escopo
fonteReferencia
metodologia
versao
validoDe
validoAte
ativo
criadoEm
atualizadoEm
schemaVersion
```

#### `emissoes_carbono`

Stores auditable emission events. It references the main entities and embeds the historical context used in the calculation.

Minimum stable fields:

```text
_id
empresaId
produtoId
fornecedorId
fatorEmissaoId
lote
etapa
quantidadeAtividade
dadosAtividade
fatorAplicado
quantidadeEmitidaKgCO2e
fonteEmissao
observacao
metodoCalculo
calculadoPor
dataEmissao
criadoEm
atualizadoEm
schemaVersion
```

### 4.2 Embedding and referencing rules

- Reference companies, products, suppliers, and active emission factors by `ObjectId` because they are independently maintained entities.
- Embed `lote` and `etapa` in each emission because they describe the context of that event and are normally read with it.
- Embed `fatorAplicado` as an immutable snapshot while retaining `fatorEmissaoId` as a reference.
- Never embed an unbounded emission array inside a company or product document.
- Add `legacyId` during Oracle data migration to preserve traceability to the relational source.
- Store dates as UTC BSON dates.
- Store CNPJ as text, never as a numeric value.
- Store calculation values as BSON `Decimal128`, never as floating-point `double`.
- Store descriptive enum values such as `ESCOPO_1`, `ESCOPO_2`, and `ESCOPO_3` as strings.

### 4.3 Flexible-schema demonstration

Use the `dadosAtividade` subdocument to represent genuinely different emission activities.

Transport example:

```javascript
dadosAtividade: {
  tipo: "TRANSPORTE",
  distanciaKm: 350,
  cargaToneladas: 1.2,
  combustivel: "DIESEL"
}
```

Energy example:

```javascript
dadosAtividade: {
  tipo: "ENERGIA",
  consumoKwh: 950,
  fonteEnergia: "SOLAR",
  percentualRenovavel: 100
}
```

Raw-material example:

```javascript
dadosAtividade: {
  tipo: "MATERIA_PRIMA",
  material: "ALUMINIO",
  pesoKg: 800,
  percentualReciclado: 65
}
```

The shared fields remain consistent, while activity-specific details vary naturally.

## 5. Planned project structure

Adapt names to the existing repository conventions when necessary.

```text
Web.Fiap.Carbono/
├── Config/
│   └── MongoDb/
│       └── MongoDbSettings.cs
├── Data/
│   └── MongoDb/
│       ├── MongoDbContext.cs
│       └── Repositories/
├── Models/
│   └── Documents/
│       ├── EmpresaDocument.cs
│       ├── ProdutoDocument.cs
│       ├── FornecedorDocument.cs
│       ├── FatorEmissaoDocument.cs
│       ├── EmissaoCarbonoDocument.cs
│       └── Embedded/
├── Controllers/
├── Services/
├── ViewModel/
└── Program.cs

database/
└── mongodb/
    ├── 01-create-collections.js
    ├── 02-create-indexes.js
    ├── 03-seed.js
    ├── 04-crud-demo.js
    └── 05-aggregation-queries.js

docs/
├── mongodb-migration.md
└── images/
    └── mongodb/
```

---

# Implementation phases

## Phase 0 — Baseline the Oracle application

### Goal

Create a trustworthy behavioral reference before changing persistence.

### Tasks

- [x] Run the complete existing test suite.
- [x] Start the API against Oracle.
- [x] Confirm that Swagger opens in the Development environment.
- [x] Save a successful login response.
- [x] Save a paginated emission-list response.
- [x] Save a single-emission response.
- [x] Save a successful emission-calculation response.
- [x] Save a product-footprint response.
- [x] Save a supplier-ranking response.
- [x] Save a company-dashboard response.
- [x] Record the current status codes and error response shapes.
- [x] Record representative totals so they can be compared after migration.

### Verification

```bash
dotnet restore Web.Fiap.Carbono.sln
dotnet build Web.Fiap.Carbono.sln
dotnet test Web.Fiap.Carbono.sln
```

### Exit gate

- [x] The existing solution builds and tests pass.
- [ ] Reference responses and totals have been saved.
- [x] No MongoDB changes have altered current behavior yet.

### Academic-scope decision

Phase 0 is complete except for a consistent refresh of the post-calculation aggregate totals. The verified responses, the remaining refresh requirement, and the comparison metrics are recorded in the [Oracle baseline](docs/oracle-baseline.md). The unchecked reference-response-and-total exit item remains visible above and must not be misrepresented as completed.

| Requirement | Result |
| --- | --- |
| Restore solution | Passed |
| Build solution | Passed, with one EF Core dependency-version warning |
| Complete test suite | Passed: 8/8 tests |
| Start API against Oracle | Passed; Oracle `EC_*` queries executed successfully |
| Swagger in Development | Passed: HTTP 200 |
| Successful login | Verified: HTTP 200; a sanitized Bearer response is saved with the token redacted |
| Paginated emissions | Post-calculation refresh verified: HTTP 200; page 1 with page size 10 returned 9 records |
| Single emission | Verified: HTTP 200; emission ID 1 emitted 180 kgCO2e |
| Emission calculation | Verified: HTTP 201 Created; emission ID 41 persisted with a result of 122.55 kgCO2e |
| Product footprint | Pre-calculation value: product ID 1 totaled 425.10 kgCO2e; refresh outstanding |
| Supplier ranking | Pre-calculation value: 5 suppliers; refresh outstanding |
| Company dashboard | Pre-calculation value: company ID 1 totaled 425.10 kgCO2e and 3 emissions; refresh outstanding |
| Status and error documentation | Complete for academic scope; README mappings plus a captured ASP.NET Core HTTP 400 validation response |
| Saved representative totals | Calculation and pagination are current; post-calculation aggregate totals remain outstanding |
| MongoDB runtime impact | None; the application remains Oracle-only |

The user has explicitly authorized proceeding to Phase 2 because it only configures an isolated MongoDB environment and does not replace Oracle persistence. Emission `41` was intentionally persisted as calculation baseline evidence. This documentation update used only read-only GET requests and created no additional Oracle record. Oracle must remain unchanged until the remaining aggregate metrics are refreshed and MongoDB behavior can be compared against a consistent baseline.

No JWT, Oracle credential, connection string, or other secret should be added to this documentation.

## Phase 1 — Document the migration decision

### Goal

Explain why the project is being migrated and why MongoDB is suitable for its ESG data.

### Tasks

- [x] Create `docs/mongodb-migration.md`.
- [x] State that the challenge option is **Migration**.
- [x] Summarize the original Oracle model and current API capabilities.
- [x] Add the seven-table-to-five-collection mapping.
- [x] Explain the embedding and referencing decisions.
- [x] Explain why activity details need a flexible schema.
- [x] Explain how snapshots improve historical auditability and governance.
- [x] Explain how aggregation pipelines support footprint, ranking, and dashboard queries.
- [x] Explain how horizontal scaling and replica sets could support future data growth and availability.
- [x] State that these operational capabilities are design benefits and are not necessarily deployed in the local academic environment.

### Exit gate

- [x] The document clearly connects MongoDB features to the ESG problem.
- [x] The document does not claim that MongoDB automatically makes the application sustainable.
- [x] All five collections and their responsibilities are documented.

## Phase 2 — Configure the local MongoDB environment

### Goal

Run a reproducible MongoDB instance locally without modifying the application yet.

### Tasks

- [x] Choose and pin the MongoDB version supported by the course environment.
- [x] Add MongoDB to the existing Docker Compose file or create a dedicated Compose file.
- [x] Configure a named volume for persistent local data.
- [x] Configure a health check.
- [x] Do not commit real credentials.
- [x] Add example environment variables to `.env.example`.
- [x] Start the container.
- [x] Connect with `mongosh`.
- [x] Connect with MongoDB Compass.
- [x] Confirm the target database name is `fiap_carbono`.

Recommended configuration keys:

```text
MongoDb__ConnectionString=mongodb://localhost:27017
MongoDb__DatabaseName=fiap_carbono
```

### Verification

```bash
docker compose up -d
docker compose ps
mongosh "mongodb://localhost:27017/fiap_carbono"
```

Inside `mongosh`:

```javascript
db.runCommand({ ping: 1 })
```

### Exit gate

- [x] MongoDB reports a healthy state.
- [x] Both `mongosh` and Compass can connect.
- [x] Credentials and local configuration files are ignored by Git.

Verified on 2026-08-31 with the course-supported `mongo:8.0.29-noble` image. Docker reported the container as healthy, `mongosh` returned `{ ok: 1 }` in the `fiap_carbono` database context, MongoDB Compass connected through port `27017`, and the local `.env` remained ignored and untracked.

## Phase 3 — Create collections, validators, and indexes

### Goal

Create the five collections explicitly and enforce only the stable domain invariants.

### Tasks

- [x] Create `database/mongodb/01-create-collections.js`.
- [x] Add explicit `db.createCollection(...)` commands for all five collections.
- [x] Add JSON Schema validation to each collection.
- [x] Require stable identifiers, calculation values, and timestamps.
- [x] Allow optional and activity-specific fields where flexibility is valuable.
- [x] Validate nonnegative activity and emission values.
- [x] Restrict emission scopes to `ESCOPO_1`, `ESCOPO_2`, or `ESCOPO_3`.
- [x] Create `database/mongodb/02-create-indexes.js`.
- [x] Add a unique CNPJ index for companies.
- [x] Add a unique CNPJ index for suppliers.
- [x] Add a unique compound index for `{ empresaId, codigo }` on products.
- [x] Add a unique compound index for `{ codigo, versao }` on emission factors.
- [x] Add query indexes for emissions by company, product, supplier, factor, date, and stage category.
- [x] Make both scripts safe to rerun or clearly document that they initialize an empty database.

Recommended emission indexes:

```javascript
db.emissoes_carbono.createIndex({ produtoId: 1, dataEmissao: -1 })
db.emissoes_carbono.createIndex({ empresaId: 1, dataEmissao: -1 })
db.emissoes_carbono.createIndex({ fornecedorId: 1, dataEmissao: -1 })
db.emissoes_carbono.createIndex({ fatorEmissaoId: 1 })
db.emissoes_carbono.createIndex({ "etapa.categoria": 1 })
```

### Verification

Run the scripts and inspect the result:

```bash
mongosh "mongodb://localhost:27017/fiap_carbono" \
  --file database/mongodb/01-create-collections.js

mongosh "mongodb://localhost:27017/fiap_carbono" \
  --file database/mongodb/02-create-indexes.js
```

Inside `mongosh`:

```javascript
show collections
db.empresas.getIndexes()
db.produtos.getIndexes()
db.fornecedores.getIndexes()
db.fatores_emissao.getIndexes()
db.emissoes_carbono.getIndexes()
```

### Exit gate

- [x] Exactly five ESG collections exist.
- [x] Validators accept valid documents and reject an intentionally invalid document.
- [x] Required unique and query indexes exist.
- [x] No collection was created merely as a relational join table.

Verified on 2026-08-31 against the local MongoDB 8.0.29 environment. Both scripts completed successfully on consecutive runs. The database contained exactly the five required collections with strict error-level validators; valid temporary documents were accepted and representative invalid documents were rejected in every collection. All temporary validation documents were removed, leaving the collections empty for Phase 4. The required unique and query indexes were inspected successfully, and no join collection exists.

## Phase 4 — Seed a coherent ESG dataset

### Goal

Populate every collection with at least ten realistic and connected documents.

### Tasks

- [x] Create `database/mongodb/03-seed.js`.
- [x] Make the seed deterministic and safe to run on a clean database.
- [x] Insert at least 10 companies.
- [x] Insert at least 10 products referencing valid companies.
- [x] Insert at least 10 suppliers.
- [x] Insert at least 10 emission factors.
- [x] Insert at least 10 emissions referencing valid domain documents.
- [x] Include emission examples for transport, energy, raw materials, waste, or another meaningful activity type.
- [x] Include all three GHG scopes across the factor and emission data when reasonable.
- [x] Include different ESG attributes across suppliers and products.
- [x] Ensure every emission's stored total equals activity quantity multiplied by the applied factor.
- [x] Avoid random data that makes the analytics impossible to verify.

Recommended minimum final counts:

| Collection | Minimum documents |
| --- | ---: |
| `empresas` | 10 |
| `produtos` | 10 |
| `fornecedores` | 10 |
| `fatores_emissao` | 10 |
| `emissoes_carbono` | 15 |

Using more than ten emissions makes product, supplier, and monthly aggregation results more convincing.

### Verification

```javascript
db.empresas.countDocuments()
db.produtos.countDocuments()
db.fornecedores.countDocuments()
db.fatores_emissao.countDocuments()
db.emissoes_carbono.countDocuments()
```

Also verify that referenced IDs resolve:

```javascript
db.emissoes_carbono.aggregate([
  {
    $lookup: {
      from: "produtos",
      localField: "produtoId",
      foreignField: "_id",
      as: "produto"
    }
  },
  { $match: { produto: { $size: 0 } } },
  { $count: "emissoesSemProduto" }
])
```

Repeat the orphan check for companies, suppliers, and factors.

### Exit gate

- [x] Every collection contains at least ten persistent documents.
- [x] All references point to existing documents.
- [x] At least three distinct `dadosAtividade` structures are visible.
- [x] Seeded emission calculations have been manually verified.

Verified on 2026-09-01 against MongoDB 8.0.29. The final counts were 10 companies, 10 products, 10 suppliers, 10 emission factors, and 15 emissions. Orphan checks returned zero for product-to-company and all four emission references. The dataset demonstrates `ENERGIA`, `MATERIA_PRIMA`, `RESIDUO`, and `TRANSPORTE`, covers all three GHG scopes, and produced zero calculation mismatches. A consecutive seed execution matched all 55 documents with zero modifications and zero upserts. Solution restore and build passed, with the existing EF Core Relational version warning, and all 8 tests passed.

## Phase 5 — Demonstrate CRUD in all five collections

### Goal

Meet the CRUD requirement without deleting the permanent seed data.

### Tasks

- [x] Create `database/mongodb/04-crud-demo.js`.
- [x] For each collection, insert one temporary `CRUD-TEMP` document.
- [x] Read the temporary document and print it.
- [x] Update the temporary document with `$set` and record `atualizadoEm`.
- [x] Read it again to prove the update.
- [x] Delete the temporary document.
- [x] Print the deletion result.
- [x] Print the final collection count.
- [x] Ensure at least ten permanent documents remain after every deletion.
- [x] Add readable section comments to the script so its output can be captured for the report.

Required operation sequence per collection:

```text
insertOne
findOne
updateOne
findOne after update
deleteOne
countDocuments
```

### Verification

```bash
mongosh "mongodb://localhost:27017/fiap_carbono" \
  --file database/mongodb/04-crud-demo.js
```

Confirm that no temporary document remains:

```javascript
db.empresas.countDocuments({ codigo: "CRUD-TEMP-EMP" })
db.produtos.countDocuments({ codigo: "CRUD-TEMP-PROD" })
db.fornecedores.countDocuments({ codigo: "CRUD-TEMP-FOR" })
db.fatores_emissao.countDocuments({ codigo: "CRUD-TEMP-FE" })
db.emissoes_carbono.countDocuments({ codigo: "CRUD-TEMP-EMISSAO" })
```

Each absence query must return `0`.

### Exit gate

- [x] CRUD has been executed successfully against every collection.
- [x] The script output visibly proves each operation.
- [x] Every collection still contains at least ten documents.
- [x] Permanent seed data was not deleted.

Verified on 2026-09-01 against MongoDB 8.0.31. The script completed the insert, first read, `$set` update, second read, delete, absence check, and final count for all five collections. Final counts matched their starting values: 10 companies, 10 products, 10 suppliers, 10 emission factors, and 15 emissions. The temporary emission preserved its calculation inputs and applied-factor snapshot during the audit-only update, and all five `CRUD-TEMP` absence checks returned zero. Solution restore and build passed, with the existing EF Core Relational version warning, and all 8 tests passed. Required screenshot evidence remains pending and must be captured from actual execution before final submission.

## Phase 6 — Add MongoDB to the .NET application

### Goal

Configure the official MongoDB driver while keeping Oracle available temporarily.

### Tasks

- [x] Add the `MongoDB.Driver` package to the API project.
- [x] Add `MongoDbSettings` with connection-string and database-name properties.
- [x] Add MongoDB configuration to the development configuration file without committing secrets.
- [x] Add environment-variable examples to the README or `.env.example`.
- [x] Create a reusable `MongoClient` registration in `Program.cs`.
- [x] Register `IMongoDatabase` once through dependency injection.
- [x] Create `MongoDbContext` or typed collection-provider abstractions.
- [x] Fail application startup with a clear message when required MongoDB settings are missing.
- [x] Ensure configuration values and credentials are not printed in logs.
- [x] Keep Oracle configuration working during the transition.

Package command:

```bash
dotnet add Web.Fiap.Carbono/Web.Fiap.Carbono.csproj package MongoDB.Driver
```

### Verification

```bash
dotnet restore Web.Fiap.Carbono.sln
dotnet build Web.Fiap.Carbono.sln
dotnet run --project Web.Fiap.Carbono/Web.Fiap.Carbono.csproj
```

### Exit gate

- [x] The application starts with MongoDB configured.
- [x] Dependency injection can resolve the database and all five typed collections.
- [x] No credentials appear in console output.
- [x] The solution still builds cleanly.

Verified on 2026-09-01 with `MongoDB.Driver` 3.11.1 and the local MongoDB 8 environment. The API started in Development, Swagger returned HTTP 200, a read-only Oracle-backed emissions request returned HTTP 200, and MongoDB returned a successful ping with exactly the five approved collection names. Dependency-injection tests resolve one shared client, database, and context, verify the lowercase persisted collection names, and prove that missing settings stop startup with fixed messages that do not echo configuration values. The Phase 6 context intentionally uses `BsonDocument`; Phase 7 replaces these transitional handles with the five domain document types. All 13 tests passed. The build succeeded with no new warnings; the accepted pre-existing EF Core Relational version warning remains.

## Phase 7 — Implement BSON document classes

### Goal

Represent the target MongoDB model explicitly in C#.

### Tasks

- [x] Create a document class for each of the five collections.
- [x] Create embedded classes for batch, stage, factor snapshot, reduction target, certification, and ESG audit data.
- [x] Map `_id` using `[BsonId]`.
- [x] Configure string IDs with `[BsonRepresentation(BsonType.ObjectId)]` if the public C# property type is `string`.
- [x] Use explicit `[BsonElement]` names if the persisted naming convention differs from C# property names.
- [x] Use `[BsonIgnoreExtraElements]` where forward-compatible reads are appropriate.
- [x] Configure decimal values to use BSON `Decimal128`.
- [x] Keep API request/response DTOs separate from database documents.
- [x] Do not expose MongoDB-specific attributes in API DTOs.
- [x] Add mapping code between documents and response DTOs.

### Verification

- [x] Serialize one example of every document type to BSON.
- [x] Deserialize the BSON back to the original type.
- [x] Confirm that IDs, UTC dates, decimal values, enum strings, and embedded documents retain their values.
- [x] Build the solution without warnings introduced by nullable reference types.

### Exit gate

- [x] All five document classes represent the approved model.
- [x] Serialization round-trip tests pass.
- [x] Documents and API DTOs remain separate.

Verified on 2026-09-01 after aligning the test project's Entity Framework Core packages with application version 8.0.22. Solution restore completed cleanly, the solution build completed with zero warnings and zero errors, and all 19 tests passed, including BSON round-trip and document-to-response mapping coverage for the five approved MongoDB document types.

## Phase 8 — Implement MongoDB repositories

### Goal

Replace EF Core persistence behavior with repository methods based on `IMongoCollection<T>`.

### Tasks

- [x] Implement `EmpresaRepository`.
- [x] Implement `ProdutoRepository`.
- [x] Implement `FornecedorRepository`.
- [x] Implement `FatorEmissaoRepository`.
- [x] Implement `EmissaoCarbonoRepository`.
- [x] Implement asynchronous create, get-by-ID, list, update, and delete methods for every repository.
- [x] Parse and validate `ObjectId` values before querying.
- [x] Implement pagination with `Skip`, `Limit`, and a stable sort.
- [x] Return domain-appropriate not-found results rather than silently accepting missing documents.
- [x] Catch duplicate-key errors and translate them into the application's domain error format.
- [x] Use projections for queries that do not need complete documents.
- [x] Add cancellation-token support to driver calls.
- [x] Avoid a generic repository that hides filters, projections, and aggregation pipelines.

Expected common operations:

```text
CreateAsync
GetByIdAsync
GetAllAsync
UpdateAsync
DeleteAsync
ExistsAsync
```

Expected emission-specific operations:

```text
GetPaginatedAsync
GetProductFootprintAsync
GetSupplierRankingAsync
GetCompanyDashboardAsync
```

### Verification

- [x] Each repository can create, read, update, and delete an isolated test document.
- [x] Invalid IDs produce a controlled `400` or `404`, according to the established API convention.
- [x] Duplicate CNPJ and compound-key violations produce controlled errors.
- [x] Pagination remains one-based and limits `pageSize` to 1–50.

### Exit gate

- [x] All five repositories are registered through dependency injection.
- [x] CRUD repository tests pass against a real disposable MongoDB instance.
- [x] No new code depends on EF Core for MongoDB operations.

Verified on 2026-09-01 with a disposable `mongo:8.0.29-noble` Testcontainers fixture bound to loopback and removed automatically after the run. Repository integration coverage exercises CRUD for all five collections, malformed `ObjectId` validation, unique CNPJ and compound-key conflicts, one-based stable pagination with the 1–50 page-size limit, decimal-safe product footprint and dashboard totals, and deterministic supplier ranking with display-data resolution. Solution restore and build succeeded with zero warnings, and all 25 tests passed.

## Phase 9 — Add CRUD services and API endpoints

### Goal

Expose complete CRUD operations for the five collections while preserving the existing layering and error format.

### Tasks

- [x] Add service methods containing domain validation and business rules.
- [x] Add CRUD controllers for companies.
- [x] Add CRUD controllers for products.
- [x] Add CRUD controllers for suppliers.
- [x] Add CRUD controllers for emission factors.
- [x] Extend the emission controller with update and delete operations if they are included in the API demonstration. They are not included in the Phase 9 REST demonstration; emission mutations remain service-only and protected as `CRUD-TEMP` operations until the emission API is migrated.
- [x] Keep controllers responsible for HTTP concerns only.
- [x] Validate parent references when creating products and emissions.
- [x] Prevent creating emissions with inactive or out-of-validity factors.
- [x] Apply `ADMIN` or `ANALISTA_ESG` authorization consistently.
- [x] Restrict destructive operations to `ADMIN`.
- [x] Preserve the global JSON error format.
- [x] Add Swagger descriptions and response types.

Suggested endpoints:

```text
POST   /api/empresas
GET    /api/empresas
GET    /api/empresas/{id}
PUT    /api/empresas/{id}
DELETE /api/empresas/{id}

POST   /api/produtos
GET    /api/produtos
GET    /api/produtos/{id}
PUT    /api/produtos/{id}
DELETE /api/produtos/{id}

POST   /api/fornecedores
GET    /api/fornecedores
GET    /api/fornecedores/{id}
PUT    /api/fornecedores/{id}
DELETE /api/fornecedores/{id}

POST   /api/fatores-emissao
GET    /api/fatores-emissao
GET    /api/fatores-emissao/{id}
PUT    /api/fatores-emissao/{id}
DELETE /api/fatores-emissao/{id}
```

### Delete policy

For the academic CRUD demonstration, hard-delete isolated `CRUD-TEMP` documents. For meaningful ESG records, prefer deactivation or reject deletion when referenced by emissions. Document this distinction as a governance decision.

### Exit gate

- [ ] CRUD works through both `mongosh` and the REST API.
- [x] Reference validation and authorization are enforced for the Phase 9 REST resources.
- [x] Swagger documents the new endpoints.
- [x] API errors follow the existing response contract.

Verified on 2026-09-02 with a disposable `mongo:8.0.29-noble` integration fixture. REST CRUD passed for `empresas`, `produtos`, `fornecedores`, and `fatores_emissao`; tests also verified public reads, JWT role enforcement, `ADMIN`-only destructive operations and factor changes, missing product-company references, referenced-record protection, duplicate-key conflicts, malformed `ObjectId` handling, flexible-field preservation, the global JSON error contract, and Swagger descriptions/security metadata. Solution restore succeeded with two `NU1900` vulnerability-audit warnings because `api.nuget.org` access was denied in the execution environment, the solution build succeeded with zero warnings, and all 29 tests passed.

Phase 9 remains visibly incomplete only because full emission CRUD is not yet available through the migrated REST API: MongoDB emission creation and retrieval are implemented, but update and delete remain service-only operations for the isolated `CRUD-TEMP-EMISSAO` demonstration. Phase 10 now verifies product, derived-company, supplier, and factor references and rejects inactive, expired, and not-yet-valid factors before persistence.

## Phase 10 — Migrate the emission-calculation workflow

### Goal

Preserve the main business rule while replacing the relational stage lookup with document-oriented input and snapshots.

### Tasks

- [x] Redesign the calculation request to receive `produtoId`, `fornecedorId`, `fatorEmissaoId`, `lote`, `etapa`, `quantidadeAtividade`, `dadosAtividade`, `fonteEmissao`, and `observacao`.
- [x] Load the product and derive `empresaId` from it.
- [x] Validate that the company, product, supplier, and factor exist.
- [x] Validate that the factor is active.
- [x] Validate the factor's effective date.
- [x] Validate activity quantity and unit compatibility.
- [x] Calculate `kgCO2e` with decimal arithmetic.
- [x] Copy the current factor into `fatorAplicado`.
- [x] Embed `lote` and `etapa` in the emission document.
- [x] Store the authenticated user in `calculadoPor`.
- [x] Store calculation and creation timestamps in UTC.
- [x] Return `201 Created` with a `Location` header.
- [x] Preserve `422` for valid input that violates an emission business rule.

Target flow:

```text
request
→ validate IDs and references
→ validate active factor and units
→ calculate kgCO2e
→ create historical snapshots
→ insert emission document
→ return 201 Created
```

### Verification scenarios

- [x] Valid transport emission.
- [x] Valid energy emission.
- [x] Valid raw-material emission.
- [x] Inactive factor.
- [x] Expired factor.
- [x] Negative or zero activity quantity.
- [x] Missing company, product, supplier, or factor.
- [x] Invalid ObjectId.
- [x] Unauthenticated request.
- [x] Exact decimal calculation.

### Exit gate

- [x] The calculation is correct for every supported activity structure.
- [x] The created document contains an immutable applied-factor snapshot.
- [x] The endpoint maintains the expected status and error behavior.

Verified on 2026-09-03 with a disposable MongoDB 8.0.29 integration environment. API tests cover successful energy, transport, raw-material, and waste calculations; exact `decimal`/BSON `Decimal128` results; variant-specific `dadosAtividade` persistence; product, derived-company, supplier, and factor reference failures; malformed public ObjectIds; zero and negative quantities; inactive, expired, not-yet-valid, and unit-incompatible factors; unauthenticated access; `201 Created` with a resolvable `Location`; and omission of nullable optional BSON fields. Service tests also verify UTC timestamps, authenticated-user attribution, and preservation of the immutable applied-factor snapshot after the source factor changes. Solution restore and build passed with zero warnings, the 19 calculation API scenarios passed, and the complete suite passed 87/87 tests.

## Phase 11 — Implement MongoDB analytics

### Goal

Rebuild the three existing analytics features with aggregation pipelines.

### 11.1 Product footprint

- [ ] Match emissions by `produtoId`.
- [ ] Sum the total `quantidadeEmitidaKgCO2e`.
- [ ] Group totals by stage category.
- [ ] Return the product identity and breakdown.
- [ ] Preserve the existing endpoint when practical:

```text
GET /api/produtos-carbono/{idProduto}/pegada
```

### 11.2 Supplier ranking

- [ ] Group emissions by `fornecedorId`.
- [ ] Sum emissions per supplier.
- [ ] Join supplier display data only when required.
- [ ] Sort by total emissions descending.
- [ ] Apply stable pagination.
- [ ] Preserve the existing endpoint:

```text
GET /api/fornecedores-carbono/ranking?pageNumber=1&pageSize=10
```

### 11.3 Company dashboard

- [ ] Match emissions by `empresaId`.
- [ ] Use `$facet` to calculate multiple summaries in one pipeline.
- [ ] Calculate the overall emission total.
- [ ] Calculate total emission count.
- [ ] Calculate monthly emissions.
- [ ] Calculate average emissions per product.
- [ ] Identify the highest-emitting product.
- [ ] Identify the highest-emitting supplier.
- [ ] Add totals by GHG scope if useful for the report.
- [ ] Preserve the existing endpoint:

```text
GET /api/dashboard-carbono/empresas/{idEmpresa}/resumo
```

### 11.4 Reusable demonstration script

- [ ] Add equivalent raw MongoDB pipelines to `database/mongodb/05-aggregation-queries.js`.
- [ ] Print readable results for screenshots.
- [ ] Comment each pipeline stage in the script.

### Verification

Compare MongoDB results with manual calculations from the deterministic seed:

- [ ] Product total.
- [ ] Breakdown by stage.
- [ ] Supplier ranking order.
- [ ] Company total.
- [ ] Monthly series.
- [ ] Highest-emitting product.
- [ ] Highest-emitting supplier.

### Exit gate

- [ ] All three analytics endpoints return correct results.
- [ ] Raw aggregation scripts produce equivalent results.
- [ ] Results match manual seed calculations.
- [ ] Pagination behavior remains compatible with the current API.

## Phase 12 — Migrate existing Oracle data

### Goal

Transfer existing relational records when a populated Oracle database is available.

If the Oracle instance contains no meaningful data, document that the project performs a structural migration and uses the MongoDB seed as the demonstration dataset.

### Tasks

- [ ] Create a one-off migration console application or migration service separate from normal API startup.
- [ ] Read companies and insert `empresas` documents.
- [ ] Build an Oracle-ID-to-MongoDB-ID mapping.
- [ ] Read products and translate their company references.
- [ ] Read suppliers and factors.
- [ ] Join each emission with its stage, batch, product, company, supplier, and factor.
- [ ] Produce one denormalized `emissoes_carbono` document per original emission.
- [ ] Add `legacyId` to migrated documents.
- [ ] Add a migration timestamp and source identifier if helpful.
- [ ] Use batched inserts.
- [ ] Make partial failures visible and recoverable.
- [ ] Do not silently skip invalid source records.

### Reconciliation checklist

- [ ] Oracle company rows equal migrated company documents.
- [ ] Oracle product rows equal migrated product documents.
- [ ] Oracle supplier rows equal migrated supplier documents.
- [ ] Oracle factor rows equal migrated factor documents.
- [ ] Oracle emission rows equal migrated emission documents.
- [ ] Total `kgCO2e` matches across databases.
- [ ] Totals per company match.
- [ ] Totals per product match.
- [ ] Totals per supplier match.
- [ ] Minimum and maximum emission dates match.
- [ ] No MongoDB references are orphaned.

### Exit gate

- [ ] The migration is repeatable on a clean target database, or its one-time limitations are documented.
- [ ] Counts and financial-style decimal totals reconcile exactly.
- [ ] Every migrated record is traceable through `legacyId`.

## Phase 13 — Replace persistence integration tests

### Goal

Test real MongoDB behavior instead of relying on EF Core InMemory behavior.

### Tasks

- [ ] Keep pure unit tests for validation and calculation logic.
- [ ] Add MongoDB integration tests using a disposable container.
- [ ] Start a clean database per test collection or test suite.
- [ ] Run collection initialization and seed logic for test data.
- [ ] Replace Oracle/EF Core test service registrations with MongoDB test registrations.
- [ ] Test CRUD for every collection.
- [ ] Test unique indexes.
- [ ] Test BSON serialization.
- [ ] Test invalid and missing ObjectIds.
- [ ] Test valid emission calculations.
- [ ] Test inactive and expired factors.
- [ ] Test product-footprint aggregation.
- [ ] Test supplier-ranking order and pagination.
- [ ] Test company-dashboard totals.
- [ ] Test authentication and authorization.
- [ ] Test consistent error responses.
- [ ] Ensure test data is isolated and deterministic.

### Minimum API test matrix

| Area | Minimum cases |
| --- | --- |
| Companies | create, read, update, delete, duplicate CNPJ |
| Products | CRUD, missing company, duplicate company/code pair |
| Suppliers | CRUD, duplicate CNPJ |
| Factors | CRUD, invalid scope, inactive factor |
| Emissions | CRUD demonstration, calculation, invalid references, unauthorized creation |
| Product footprint | correct total and stage breakdown |
| Supplier ranking | correct order and pagination |
| Company dashboard | totals, monthly data, highest emitters |

### Verification

```bash
dotnet build Web.Fiap.Carbono.sln
dotnet test Web.Fiap.Carbono.sln
```

### Exit gate

- [ ] Tests use a real MongoDB engine.
- [ ] Tests are deterministic and independent.
- [ ] All business, CRUD, aggregation, validation, and authorization tests pass.

## Phase 14 — Remove Oracle and EF Core persistence

### Goal

Complete the cutover only after the MongoDB version reaches parity.

### Preconditions

- [ ] MongoDB CRUD endpoints work.
- [ ] Emission calculation works.
- [ ] Analytics results are correct.
- [ ] Integration tests pass.
- [ ] Oracle data has been migrated or the seed-only decision is documented.

### Tasks

- [ ] Remove the Oracle EF Core provider package.
- [ ] Remove unused EF Core packages if no longer needed.
- [ ] Remove `DatabaseContext` and Oracle-specific registrations.
- [ ] Remove or archive EF Core repositories.
- [ ] Remove Oracle migrations from the active application path.
- [ ] Remove `ConnectionStrings:OracleConnection` from active configuration.
- [ ] Remove Oracle-only Docker and environment configuration.
- [ ] Search the repository for stale Oracle and EF references.
- [ ] Update build and deployment files.
- [ ] Preserve historical source through Git rather than leaving dead code in the application.

### Verification

```bash
rg -n "Oracle|OracleConnection|DbContext|EntityFrameworkCore" .
dotnet clean Web.Fiap.Carbono.sln
dotnet restore Web.Fiap.Carbono.sln
dotnet build Web.Fiap.Carbono.sln
dotnet test Web.Fiap.Carbono.sln
```

Review every `rg` result. Some historical documentation references may remain intentionally.

### Exit gate

- [ ] The running API has no Oracle runtime dependency.
- [ ] The active persistence implementation uses the MongoDB driver.
- [ ] The clean build and all tests pass.

## Phase 15 — Update technical documentation

### Goal

Make the repository and FIAP submission independently understandable and reproducible.

### README updates

- [ ] Change the database technology from Oracle to MongoDB.
- [ ] Replace Oracle setup instructions with MongoDB setup instructions.
- [ ] Document the five collections.
- [ ] Document the embedded and referenced relationships.
- [ ] Document configuration variables.
- [ ] Document database scripts and their execution order.
- [ ] Document seed behavior.
- [ ] Document new CRUD endpoints.
- [ ] Document aggregation endpoints.
- [ ] Document how to run integration tests.
- [ ] Update the architecture diagram.
- [ ] Remove obsolete Oracle migration instructions.

### `docs/mongodb-migration.md` final structure

```text
1. Project overview
2. Decision: migration
3. Original relational model
4. Why MongoDB fits the ESG problem
5. Seven-table-to-five-collection mapping
6. Detailed collection descriptions
7. Embedding and referencing decisions
8. Flexible-schema examples
9. Validators and indexes
10. Creation commands
11. Seed data
12. CRUD commands
13. Aggregation pipelines
14. API migration
15. Testing and reconciliation
16. Screenshots
17. Conclusion
```

### Exit gate

- [ ] A new developer can start MongoDB, seed it, run the API, and run the tests using only the README.
- [ ] The migration document directly answers every challenge requirement.
- [ ] Commands shown in the documents match the committed scripts.

## Phase 16 — Capture evidence and assemble the submission

### Goal

Produce unambiguous visual proof of collection creation and CRUD execution.

### Screenshot checklist

- [ ] MongoDB Compass showing database `fiap_carbono` and all five collections.
- [ ] Successful collection-creation script.
- [ ] Indexes visible for the collections.
- [ ] `empresas` insert, find, update, verification, and delete.
- [ ] `produtos` insert, find, update, verification, and delete.
- [ ] `fornecedores` insert, find, update, verification, and delete.
- [ ] `fatores_emissao` insert, find, update, verification, and delete.
- [ ] `emissoes_carbono` insert, find, update, verification, and delete.
- [ ] Final counts proving at least ten documents remain in every collection.
- [ ] Documents showing at least three `dadosAtividade` structures.
- [ ] Product-footprint aggregation and result.
- [ ] Supplier-ranking aggregation and result.
- [ ] Company-dashboard aggregation and result.
- [ ] Optional Swagger screenshots of the CRUD and analytics endpoints.

Store screenshots under:

```text
docs/images/mongodb/
```

Suggested names:

```text
01-collections-created.png
02-indexes.png
03-empresas-crud.png
04-produtos-crud.png
05-fornecedores-crud.png
06-fatores-emissao-crud.png
07-emissoes-carbono-crud.png
08-final-document-counts.png
09-flexible-schema.png
10-product-footprint.png
11-supplier-ranking.png
12-company-dashboard.png
```

### Screenshot quality rules

- Show both the command and its result.
- Keep the database and collection name visible.
- Do not expose passwords, connection-string credentials, or JWT signing keys.
- Use readable zoom and crop irrelevant desktop areas.
- Add a caption and short interpretation below every image in the report.
- Execute the commands rather than presenting only source-code screenshots.

### Exit gate

- [ ] Every required database operation has visual evidence.
- [ ] Screenshots are referenced in the technical document in execution order.
- [ ] No secret or personal data is visible.

---

# Final verification

## Automated verification

```bash
docker compose up -d

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

dotnet restore Web.Fiap.Carbono.sln
dotnet build Web.Fiap.Carbono.sln
dotnet test Web.Fiap.Carbono.sln
```

If the initialization scripts assume an empty database, reset only the explicitly named development database through a documented local reset command before running this sequence. Never place an unrestricted database-drop command in normal application startup.

## Manual API smoke test

- [ ] Obtain a JWT using the existing login endpoint.
- [ ] Create and retrieve one company.
- [ ] Create and retrieve one product.
- [ ] Create and retrieve one supplier.
- [ ] Create and retrieve one emission factor.
- [ ] Calculate one emission.
- [ ] Retrieve that emission.
- [ ] Retrieve its product footprint.
- [ ] Retrieve the supplier ranking.
- [ ] Retrieve the company dashboard.
- [ ] Confirm an unauthenticated calculation is rejected.
- [ ] Confirm an inactive factor is rejected.

# Definition of Done

The migration is complete only when every item below is true.

## ESG and data design

- [ ] The submission identifies the project as an ESG migration.
- [ ] The environmental purpose is clear.
- [ ] Social supplier attributes and governance/audit metadata are represented coherently.
- [ ] The design uses exactly five ESG collections.
- [ ] Embedding and referencing decisions are justified.
- [ ] Flexible documents reflect real activity differences.

## MongoDB implementation

- [ ] The five collections are created explicitly.
- [ ] Appropriate validators and indexes exist.
- [ ] Every collection contains at least ten documents.
- [ ] Every collection has demonstrated create, read, update, and delete operations.
- [ ] At least three meaningful document structures exist within emission activity data.
- [ ] Decimal calculation precision is preserved.
- [ ] Aggregation pipelines generate the required analytics.

## API implementation

- [ ] The application uses `MongoDB.Driver` for persistence.
- [ ] CRUD operations are available and validated.
- [ ] Emission calculation works and stores audit snapshots.
- [ ] Product footprint works.
- [ ] Supplier ranking works.
- [ ] Company dashboard works.
- [ ] Authentication, authorization, pagination, and error responses remain consistent.

## Quality and submission

- [ ] A clean build succeeds.
- [ ] All tests pass against a real MongoDB engine.
- [ ] The README contains reproducible setup instructions.
- [ ] The technical migration document answers every challenge requirement.
- [ ] Commands are included in full.
- [ ] Screenshots demonstrate collection creation and every CRUD category.
- [ ] No secrets are committed or visible in screenshots.
- [ ] Oracle runtime dependencies have been removed after parity is achieved.

# Recommended commit sequence

Keep commits small and aligned with the phases:

```text
docs: describe mongodb migration architecture
chore: add local mongodb environment
feat: add mongodb collection and index scripts
testdata: add coherent esg mongodb seed
docs: add mongodb crud demonstration script
feat: configure mongodb driver and document models
feat: add mongodb repositories
feat: expose collection crud endpoints
feat: migrate carbon emission calculation
feat: implement mongodb analytics pipelines
test: add mongodb integration coverage
refactor: remove oracle persistence
docs: finalize mongodb setup and migration evidence
```

# Suggested execution order summary

```text
Baseline Oracle behavior
→ document the MongoDB model
→ start MongoDB locally
→ create collections and indexes
→ seed at least ten documents each
→ execute CRUD demonstrations
→ configure MongoDB in .NET
→ implement document classes and repositories
→ expose CRUD endpoints
→ migrate emission calculation
→ rebuild analytics with aggregation pipelines
→ migrate existing Oracle data if available
→ replace integration tests
→ remove Oracle
→ update documentation
→ capture screenshots and submit
```
