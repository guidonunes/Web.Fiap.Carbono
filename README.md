# Web.Fiap.Carbono

Web.Fiap.Carbono is a REST API for measuring and analyzing greenhouse-gas emissions across a product supply chain. It was built as an ESG-focused FIAP academic project and covers the environmental pillar of ESG.

The API connects companies, products, production batches, supply-chain stages, suppliers, emission factors, and carbon-emission records. It can calculate an emission in `kgCO2e` from activity data and an emission factor, then aggregate those records into product footprints, supplier rankings, and company dashboards.

This repository contains a backend API only; it does not include a web or mobile frontend.

## Main features

- Issue JWTs for two demonstration users with `ADMIN` and `ANALISTA_ESG` roles.
- List carbon-emission records with pagination and retrieve an individual record.
- Calculate and persist an emission for a supply-chain stage using an active emission factor.
- Calculate a product's total carbon footprint and break it down by supply-chain stage.
- Rank suppliers using the emissions associated with the stages they operate.
- Produce a company-level ESG summary with totals, counts, monthly emissions, average emissions per product, and the highest-emitting product and supplier.
- Validate request data, IDs, pagination, active emission factors, and database constraints.
- Return consistent JSON errors through global exception handling.
- Expose Swagger/OpenAPI documentation in the Development environment.
- Run integration-style API tests against an isolated EF Core InMemory database.

The emission calculation is:

```text
emitted quantity (kgCO2e) = activity quantity × emission factor value
```

The API stores the result with the unit `kgCO2e`. It does not perform unit conversion, so the activity quantity must match the selected factor's base unit.

## Technology stack

| Area | Technology |
|---|---|
| Runtime and language | .NET 8, C# |
| Web framework | ASP.NET Core Web API |
| Database | Oracle Database (active) and MongoDB 8 (migration target) |
| Data access | Entity Framework Core 8 with `Oracle.EntityFrameworkCore`; `MongoDB.Driver` configured for the transition |
| Schema management | EF Core migrations |
| Object mapping | AutoMapper |
| Authentication | JWT Bearer tokens signed with HMAC-SHA256 |
| API documentation | Swagger/OpenAPI through Swashbuckle |
| Tests | xUnit, `Microsoft.AspNetCore.Mvc.Testing`, EF Core InMemory, Coverlet |
| Packaging | Multi-stage Linux Docker image |

Important package versions are declared in [the API project file](Web.Fiap.Carbono/Web.Fiap.Carbono.csproj). The repository pins the .NET 8 SDK family in [`global.json`](global.json).

## Architecture

The code uses a layered controller-service-repository architecture. The classes under `ViewModel/` are API request and response DTOs; despite the name, this is not a conventional MVVM user-interface application.

```mermaid
flowchart LR
    Client[HTTP client] --> Controller[Controllers]
    Controller --> Service[Services<br/>validation and business rules]
    Service --> Repository[Repositories<br/>queries and persistence]
    Repository --> EF[EF Core DatabaseContext]
    EF --> Oracle[(Oracle Database)]
    Controller --> Mapper[AutoMapper]
    Mapper --> DTO[ViewModels / DTOs]
```

| Directory | Responsibility |
|---|---|
| `Controllers/` | HTTP routes, status codes, and request/response handling |
| `Services/` | Domain validation, business rules, and emission calculation |
| `Data/Repository/` | EF Core queries and persistence |
| `Data/Contexts/` | Oracle table, relationship, index, and constraint mapping |
| `Data/MongoDb/` | MongoDB database and five-collection provider used by the migration |
| `Models/` | Database-backed domain entities |
| `ViewModel/` | API input and output models |
| `Mapping/` | Domain-to-response transformations and aggregate calculations |
| `Config/Security/` | JWT settings |
| `Config/MongoDb/` | MongoDB connection and database-name settings |
| `Middlewares/` | Global exception-to-HTTP-response mapping |
| `Exceptions/` | Domain-specific exception types |
| `Migrations/` | Versioned Oracle schema |

Dependencies are registered with ASP.NET Core's built-in dependency injection container in [`Program.cs`](Web.Fiap.Carbono/Program.cs).

The application is currently in a transitional state. Oracle repositories remain the active persistence implementation, while one reusable MongoDB client, database, and five-collection context are registered for the later migration phases. MongoDB is not yet used by controllers, services, or repositories.

## Database

The active API data provider is **Oracle Database**, accessed through Entity Framework Core and Oracle's EF Core provider. The connection is configured under `ConnectionStrings:OracleConnection`. MongoDB is configured under `MongoDb`, but does not replace Oracle persistence during Phase 6.

The schema is created by the `CreateCarbonEmissionSchema` EF Core migration. All application tables use the `EC_` prefix:

| Table | Purpose |
|---|---|
| `EC_EMPRESAS` | Companies whose products and emissions are tracked |
| `EC_PRODUTOS` | Products owned by a company |
| `EC_LOTES_PRODUCAO` | Production batches for a product |
| `EC_ETAPAS_CADEIA` | Ordered supply-chain stages for a batch, each linked to a supplier |
| `EC_FORNECEDORES` | Suppliers participating in supply-chain stages |
| `EC_FATORES_EMISSAO` | Emission factors, base units, references, and GHG scopes |
| `EC_EMISSOES_CARBONO` | Calculated activity and `kgCO2e` emission records |

### Entity relationships

```mermaid
erDiagram
    EC_EMPRESAS ||--o{ EC_PRODUTOS : owns
    EC_PRODUTOS ||--o{ EC_LOTES_PRODUCAO : contains
    EC_LOTES_PRODUCAO ||--o{ EC_ETAPAS_CADEIA : passes_through
    EC_FORNECEDORES ||--o{ EC_ETAPAS_CADEIA : operates
    EC_ETAPAS_CADEIA ||--o{ EC_EMISSOES_CARBONO : produces
    EC_FATORES_EMISSAO ||--o{ EC_EMISSOES_CARBONO : calculates
```

The schema includes unique CNPJ indexes for companies and suppliers, foreign-key indexes, positive-value checks, date-order checks, active-status checks (`S` or `N`), and emission-scope checks (`ESCOPO_1`, `ESCOPO_2`, or `ESCOPO_3`). Foreign-key deletion is restrictive; related records are not cascade-deleted.

Authentication users are not stored in Oracle. The two demonstration users are currently held in memory by `AuthService`.

## API endpoints

All routes use the `/api` prefix. Except for emission creation, the current read endpoints are public.

| Method | Route | Authentication | Description |
|---|---|---|---|
| `POST` | `/api/auth/login` | Public | Validate a demonstration user and issue a JWT |
| `GET` | `/api/emissoes-carbono?pageNumber=1&pageSize=10` | Public | List emissions, newest first |
| `GET` | `/api/emissoes-carbono/{idEmissao}` | Public | Retrieve one emission and its stage/factor details |
| `POST` | `/api/emissoes-carbono/calcular` | `ADMIN` or `ANALISTA_ESG` | Calculate and persist an emission |
| `GET` | `/api/produtos-carbono/{idProduto}/pegada` | Public | Return a product footprint and totals by stage |
| `GET` | `/api/fornecedores-carbono/ranking?pageNumber=1&pageSize=10` | Public | Return paginated supplier emission summaries |
| `GET` | `/api/dashboard-carbono/empresas/{idEmpresa}/resumo` | Public | Return a company's aggregated carbon dashboard |

Pagination starts at page 1. `pageSize` must be between 1 and 50. A paginated response has this shape:

```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 10,
  "totalItems": 0,
  "totalPages": 0
}
```

The API currently exposes analytics and emission-record operations. It does not expose CRUD endpoints for companies, products, batches, suppliers, stages, or emission factors; those records must already exist in the database before an emission can be calculated.

## Authentication

Use one of the demonstration accounts:

| Role | Email | Password |
|---|---|---|
| `ADMIN` | `admin@carbono.com` | `Carbono@123` |
| `ANALISTA_ESG` | `analista@carbono.com` | `Carbono@123` |

Request a token:

```bash
curl --request POST http://localhost:5269/api/auth/login \
  --header "Content-Type: application/json" \
  --data '{
    "email": "admin@carbono.com",
    "senha": "Carbono@123"
  }'
```

Then send the returned token in the `Authorization` header:

```bash
curl --request POST http://localhost:5269/api/emissoes-carbono/calcular \
  --header "Authorization: Bearer YOUR_TOKEN" \
  --header "Content-Type: application/json" \
  --data '{
    "idEtapa": 1,
    "idFator": 1,
    "quantidadeAtividade": 100,
    "fonteEmissao": "Diesel",
    "observacao": "Monthly transport activity"
  }'
```

A successful calculation returns `201 Created`, including the created emission and a `Location` header for its resource.

> The built-in users and plain-text demonstration passwords are suitable only for this academic sample. A production deployment should use a persistent identity store, hashed passwords, secret management, HTTPS-only token handling, and an appropriate token-revocation strategy.

## Local setup

### Prerequisites

- .NET 8 SDK
- Access to an Oracle Database instance
- MongoDB 8, either locally or through the provided Compose service
- `dotnet-ef` 8.x if you need to apply or create migrations

### 1. Configure Oracle, JWT, and MongoDB

Create `Web.Fiap.Carbono/appsettings.Development.json`. This file is ignored by Git, so real credentials and signing keys are not committed.

```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=YOUR_USER;Password=YOUR_PASSWORD;Data Source=YOUR_DATA_SOURCE"
  },
  "Jwt": {
    "SecretKey": "replace-with-a-random-secret-at-least-32-characters-long",
    "Issuer": "Web.Fiap.Carbono",
    "Audience": "Web.Fiap.Carbono.Users",
    "ExpirationMinutes": 60
  },
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "fiap_carbono"
  }
}
```

ASP.NET Core configuration can also be supplied with environment variables such as `ConnectionStrings__OracleConnection`, `Jwt__SecretKey`, `MongoDb__ConnectionString`, and `MongoDb__DatabaseName`. Safe MongoDB examples are provided in [`.env.example`](.env.example); do not commit credentials.

### 2. Start MongoDB

```bash
docker compose up -d mongodb
docker compose ps
```

### 3. Restore and build

From the repository root:

```bash
dotnet restore Web.Fiap.Carbono.sln
dotnet build Web.Fiap.Carbono.sln
```

### 4. Apply the Oracle schema

```bash
dotnet ef database update \
  --project Web.Fiap.Carbono/Web.Fiap.Carbono.csproj \
  --startup-project Web.Fiap.Carbono/Web.Fiap.Carbono.csproj
```

The migration creates the schema only; it does not seed production data.

### 5. Run the API

```bash
dotnet run --project Web.Fiap.Carbono/Web.Fiap.Carbono.csproj
```

With the checked-in launch profile, the HTTP URL is `http://localhost:5269` and the HTTPS URL is `https://localhost:7090`. Swagger is available only when `ASPNETCORE_ENVIRONMENT=Development`:

```text
http://localhost:5269/swagger
```

Ready-to-run request examples are available in [`Web.Fiap.Carbono.http`](Web.Fiap.Carbono/Web.Fiap.Carbono.http).

## Tests

Run all tests from the repository root:

```bash
dotnet test Web.Fiap.Carbono.sln
```

The tests start the real ASP.NET Core application through `WebApplicationFactory`, replace Oracle with a uniquely named EF Core InMemory database, and seed a controlled company/product/supplier/emission graph. They cover the public analytics endpoints, valid emission reads, invalid pagination, and rejection of unauthenticated emission creation. Phase 6 tests also verify MongoDB settings binding, singleton reuse, startup validation, and the exact five collection names without connecting to or mutating a developer database.

The InMemory provider is useful for deterministic API tests but does not validate Oracle- or MongoDB-specific persistence behavior, types, indexes, or constraints. The Phase 6 configuration tests are not substitutes for the disposable MongoDB integration tests required in a later phase.

## Docker

Build the multi-stage image from the repository root:

```bash
docker build \
  --file Web.Fiap.Carbono/Dockerfile \
  --tag web-fiap-carbono .
```

Supply configuration at runtime rather than baking secrets into the image:

```bash
docker run --rm --publish 8080:8080 \
  --env ConnectionStrings__OracleConnection="YOUR_ORACLE_CONNECTION_STRING" \
  --env Jwt__SecretKey="YOUR_RANDOM_JWT_SECRET" \
  --env Jwt__Issuer="Web.Fiap.Carbono" \
  --env Jwt__Audience="Web.Fiap.Carbono.Users" \
  --env Jwt__ExpirationMinutes="60" \
  web-fiap-carbono
```

Swagger is disabled by default in the container because the image does not set the Development environment.

## Error responses

Unhandled domain exceptions are converted to JSON by the global middleware:

```json
{
  "statusCode": 404,
  "error": "NotFound",
  "message": "Etapa da cadeia não encontrada.",
  "timestamp": "2026-08-26T12:00:00Z"
}
```

The main mappings are `400` for validation/argument errors, `401` for invalid login or missing authentication, `404` for missing domain records, `422` for business-rule violations such as an inactive emission factor, and `500` for unexpected failures.
