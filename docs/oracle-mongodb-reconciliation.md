# Oracle-to-MongoDB reconciliation

## Purpose and environment

This report records the Phase 12 comparison between the authoritative Oracle
`EC_*` snapshot and the one-off migration target
`mongodb://127.0.0.1:27018/fiap_carbono`. The target was initialized with only
the repository's collection and index scripts before import. The application's
MongoDB database on port `27017` was not used as the migration target.

The migration ran on 2026-09-05 with the reviewed, checked-in
`Web.Fiap.Carbono.Migration/migration-policy.json`. Oracle access was read-only.
The tool converted Oracle `DATE` values from the reviewed
`America/Sao_Paulo` source timezone to UTC BSON dates.

## Reconciliation result

All comparisons passed exactly:

| Check | Oracle | MongoDB | Result |
| --- | ---: | ---: | --- |
| Companies | 5 | 5 | Passed |
| Products | 5 | 5 | Passed |
| Suppliers | 5 | 5 | Passed |
| Emission factors | 5 | 5 | Passed |
| Emissions | 9 | 9 | Passed |
| Total emissions | 3085.45 kgCO2e | 3085.45 kgCO2e | Passed |
| Earliest emission | `2026-06-15T19:59:34` Oracle time | `2026-06-15T22:59:34Z` | Passed after policy conversion |
| Latest emission | `2026-08-31T17:03:08` Oracle time | `2026-08-31T20:03:08Z` | Passed after policy conversion |

Decimal totals were read from MongoDB as BSON `Decimal128`; no floating-point
conversion was used.

### Totals by Oracle legacy ID

| Legacy ID | Company total | Product total | Supplier total |
| ---: | ---: | ---: | ---: |
| 1 | 670.20 | 670.20 | 670.20 |
| 2 | 204.25 | 204.25 | 204.25 |
| 3 | 675.00 | 675.00 | 675.00 |
| 4 | 1000.00 | 1000.00 | 536.00 |
| 5 | 536.00 | 536.00 | 1000.00 |

Each Oracle and MongoDB grouped total matched exactly.

## References and traceability

The following orphan counts were all zero:

- product to company;
- emission to company;
- emission to product;
- emission to supplier;
- emission to emission factor.

Every migrated root document has one unique `legacyId`, and each collection's
legacy-ID set exactly matches its Oracle source primary-key set. Emissions also
retain batch and stage Oracle IDs in their embedded snapshots. The verifier
matched every target document to the deterministic source plan, including its
migration origin and timestamp.

## Repeatability and limitations

The first apply inserted 5 companies, 5 products, 5 suppliers, 5 factors, and 9
emissions. An immediate retry with the unchanged Oracle source and policy
inserted zero documents in every collection and changed none, proving restart
and retry idempotency for this snapshot. A fresh clean target produces the same
ObjectIds because IDs are derived from the source identifier, collection, and
`legacyId`.

The tool intentionally has no distributed lock or cross-collection transaction.
It exposes partial failures and safely resumes only when the source and policy
remain unchanged. If either changes after a partial import, use a new isolated
target or investigate the conflict; the tool does not overwrite or delete
existing evidence. Reconstructed factor snapshots and unavailable source fields
remain explicitly labelled migration limitations.

## Commands used

```bash
dotnet run --no-build --project Web.Fiap.Carbono.Migration -- \
  Web.Fiap.Carbono.Migration/migration-policy.json --apply \
  --oracle-config Web.Fiap.Carbono/appsettings.Development.json

dotnet run --no-build --project Web.Fiap.Carbono.Migration -- \
  Web.Fiap.Carbono.Migration/migration-policy.json --reconcile \
  --oracle-config Web.Fiap.Carbono/appsettings.Development.json
```

The local Oracle configuration path is shown, but no connection string,
credential, token, or signing secret is recorded here.

## Conclusion

Phase 12 reconciliation is complete for the reviewed Oracle snapshot. The
isolated migration is repeatable, all required counts and Decimal128 totals
match exactly, no MongoDB references are orphaned, and every migrated record is
traceable through `legacyId`. Oracle remains available as the recoverable source
until the later parity and removal gates are satisfied.
