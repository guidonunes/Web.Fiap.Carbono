# Oracle API Baseline

## Purpose

This baseline records observable behavior from the Oracle-backed Web.Fiap.Carbono API before MongoDB persistence is introduced. Its values provide comparison points for the future MongoDB implementation; this is evidence for an academic migration exercise, not a production audit.

## Environment confirmation

The API was running with Oracle as its active persistence provider, and successful requests executed queries against the Oracle `EC_*` tables. MongoDB had no runtime impact when this baseline was captured, and the application was Oracle-only at that historical checkpoint. The current transitional application state is documented in the [project README](../README.md) and [MongoDB migration report](mongodb-migration.md).

## Build and test results

| Check | Verified result |
| --- | --- |
| Solution restore | Passed |
| Solution build | Passed, with one EF Core dependency-version warning |
| Test suite | Passed: 8/8 tests |
| API against Oracle | Passed; Oracle `EC_*` queries executed successfully |
| Swagger | HTTP 200 |

## Endpoint evidence

| Operation | Verified result |
| --- | --- |
| Login | HTTP 200; Bearer token issued for `ADMIN` |
| Paginated emissions | Post-calculation refresh: HTTP 200; page 1 with page size 10; 9 records in the current dataset |
| Emission by ID | Pre-calculation evidence: HTTP 200; emission ID 1 emitted 180 kgCO2e |
| Emission calculation | HTTP 201 Created; emission ID 41 persisted with a result of 122.55 kgCO2e |
| Product footprint | Post-calculation Oracle inventory: product ID 1 totals 670.20 kgCO2e across 5 emissions |
| Supplier ranking | Post-calculation Oracle inventory: 5 suppliers with emissions |
| Company dashboard | Post-calculation Oracle inventory: company ID 1 totals 670.20 kgCO2e and 5 emissions |
| Validation error | Invalid login returned HTTP 400 Bad Request with the ASP.NET Core validation-error structure |

## Sanitized login response

The successful login response issued an administrator Bearer token. The token is deliberately redacted, and the supplied unredacted login screenshot is not included or linked.

```json
{
  "token": "[REDACTED]",
  "tipo": "Bearer",
  "email": "admin@carbono.com",
  "role": "ADMIN"
}
```

## Successful emission calculation

The Oracle-backed calculation endpoint returned `201 Created`. One Oracle record, emission `41`, was intentionally created as baseline evidence.

```text
idEmissao: 41
idEtapa: 1
tipoEtapa: TRANSPORTE
idFator: 2
fonteFator: Energia eletrica
escopo: ESCOPO_2
unidadeBase: kWh
valorFatorCo2e: 0.0817
fonteEmissao: Energia eletrica
quantidadeAtividade: 1500
quantidadeEmitida: 122.5500
unidade: kgCO2e
metodoCalculo: QuantidadeAtividade * ValorFatorCo2e
```

The returned calculation result is `122.55 kgCO2e`. This documentation update did not call the calculation endpoint or create another Oracle record.

## Validation error evidence

An invalid login request returned `400 Bad Request` with the title `One or more validation errors occurred.` The captured ASP.NET Core response contained the following fields:

| Field | Baseline expectation |
| --- | --- |
| `type` | Present |
| `title` | `One or more validation errors occurred.` |
| `status` | `400` |
| `errors` | Present; contains validation details |
| `traceId` | Present at runtime, but its value is request-specific and is not a fixed expected value |

Together with the status mappings documented in `README.md`, this response is sufficient error evidence for the academic baseline.

## Representative totals

### Post-calculation values

A read-only refresh on 2026-08-31 returned:

```text
Paginated emission records: 9
Emission ID 41 calculation result: 122.55 kgCO2e
```

The refreshed page included emission `41` and returned its calculation fields consistently with the supplied `201 Created` response.

### Read-only Oracle inventory

On 2026-09-04, a temporary read-only EF Core inventory queried the configured Oracle `EC_*` tables directly. Direct Oracle queries were necessary because the product-footprint, supplier-ranking, and company-dashboard API routes had already migrated to MongoDB and could no longer provide Oracle aggregate values. The inventory did not call `SaveChanges`, execute DML, or expose the Oracle connection string.

| Oracle source | Count |
| --- | ---: |
| `EC_EMPRESAS` | 5 |
| `EC_PRODUTOS` | 5 |
| `EC_FORNECEDORES` | 5 |
| `EC_FATORES_EMISSAO` | 5 |
| `EC_LOTES_PRODUCAO` | 5 |
| `EC_ETAPAS_CADEIA` | 5 |
| `EC_EMISSOES_CARBONO` | 9 |
| Complete joined emission graphs | 9 |

The current Oracle emission total is `3085.45 kgCO2e`. The earliest emission date is `2026-06-15T19:59:34`, and the latest is `2026-08-31T17:03:08`. These are Oracle `DATE` values as returned by the provider; no timezone is inferred.

All read-only orphan checks returned zero: products without companies, batches without products, stages without batches, stages without suppliers, emissions without stages, and emissions without factors.

### Post-calculation aggregate values

The direct Oracle refresh produced these current comparison values:

| Aggregate | Current Oracle value |
| --- | ---: |
| Product ID 1 footprint | 670.20 kgCO2e |
| Product ID 1 emission count | 5 |
| Supplier ranking entries | 5 |
| Company ID 1 dashboard total | 670.20 kgCO2e |
| Company ID 1 emission count | 5 |
| Company ID 1 product count | 1 |
| Company ID 1 batch count | 1 |
| Company ID 1 stage count | 1 |
| Company ID 1 average per product | 670.20 kgCO2e |

All five product ID 1 emissions belong to Oracle stage ID 1, whose type is `TRANSPORTE`, and total `670.20 kgCO2e`. Company ID 1 has the following monthly totals:

| Month | Total |
| --- | ---: |
| June 2026 | 425.10 kgCO2e |
| August 2026 | 245.10 kgCO2e |

The supplier ranking ordered by total emissions descending is:

| Position | Supplier legacy ID | Emissions | Total |
| ---: | ---: | ---: | ---: |
| 1 | 5 | 1 | 1000.00 kgCO2e |
| 2 | 3 | 1 | 675.00 kgCO2e |
| 3 | 1 | 5 | 670.20 kgCO2e |
| 4 | 4 | 1 | 536.00 kgCO2e |
| 5 | 2 | 1 | 204.25 kgCO2e |

Company ID 1's highest-emitting product and supplier both have legacy ID 1. The direct `GET /api/emissoes-carbono/41` verification also returned `122.55 kgCO2e`. No POST, PUT, PATCH, or DELETE request was executed during this refresh.

## Screenshot evidence

The two newly supplied screenshots were not available as files in the local execution environment, so no image files or broken links were added. After their contents are checked again, the masked calculation screenshot and validation screenshot still need to be copied manually to:

```text
docs/images/oracle-baseline/07-emission-calculation.png
docs/images/oracle-baseline/08-validation-error-400.png
```

The earlier safe endpoint screenshots also remain unavailable locally. Any screenshot containing an unmasked JWT, password, Oracle credential, connection string, or signing secret must not be added.

## Phase 0 status

Phase 0 is complete for the accepted academic scope. The sanitized reference responses, calculation evidence, current Oracle counts, post-calculation aggregate totals, date range, and reconciliation breakdowns are recorded above. The aggregate refresh came from direct read-only Oracle queries because the corresponding API routes now use MongoDB.

Oracle must remain available and unchanged until the real Phase 12 migration is compared with these recorded metrics.

No JWT, Oracle credential, connection string, or signing secret is included in this report.
