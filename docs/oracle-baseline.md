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
| Product footprint | Pre-calculation evidence: HTTP 200; product ID 1 totaled 425.10 kgCO2e |
| Supplier ranking | Pre-calculation evidence: HTTP 200; 5 suppliers returned |
| Company dashboard | Pre-calculation evidence: HTTP 200; company ID 1 totaled 425.10 kgCO2e and 3 emissions |
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

### Pre-calculation aggregate values

The API became unavailable after the paginated refresh. The direct emission, product-footprint, supplier-ranking, and company-dashboard GET requests could not be completed, so the following aggregate values remain explicitly labeled as pre-calculation evidence:

```text
Emission ID 1: 180 kgCO2e
Product ID 1 footprint: 425.10 kgCO2e
Supplier ranking entries: 5
Company ID 1 dashboard total: 425.10 kgCO2e
Company ID 1 emission count: 3
```

The pre-calculation company dashboard also reported one product, one production batch, one supply-chain stage, and a monthly total of 425.10 kgCO2e for June 2026.

Refreshing the product footprint, supplier ranking, and company dashboard after emission `41` remains outstanding. Their current totals must be copied from actual API responses and must not be inferred from the calculation result.

## Screenshot evidence

The two newly supplied screenshots were not available as files in the local execution environment, so no image files or broken links were added. After their contents are checked again, the masked calculation screenshot and validation screenshot still need to be copied manually to:

```text
docs/images/oracle-baseline/07-emission-calculation.png
docs/images/oracle-baseline/08-validation-error-400.png
```

The earlier safe endpoint screenshots also remain unavailable locally. Any screenshot containing an unmasked JWT, password, Oracle credential, connection string, or signing secret must not be added.

## Phase 0 status

The calculation response and academic error-response evidence are now complete. Phase 0 is complete except for a consistent refresh of the post-calculation aggregate totals. Its reference-response-and-total exit-gate item remains open until those read-only aggregate responses are captured.

This is sufficient to proceed to Phase 2 because Phase 2 only configures an isolated MongoDB environment and does not replace Oracle persistence. Oracle must remain unchanged until MongoDB behavior can be compared with a consistent set of recorded metrics.

No JWT, Oracle credential, connection string, or signing secret is included in this report.
