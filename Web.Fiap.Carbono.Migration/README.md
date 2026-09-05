# Phase 12 Oracle migration tool

This console application is separate from API startup. It reads the seven
Oracle tables in one serializable transaction, prepares the five MongoDB domain
collections, validates the entire plan, and optionally inserts ordered batches.
Oracle reads use `AsNoTracking` behavior; no Oracle DML or `SaveChanges` is used.

## Commands

Run from the Git root:

```bash
dotnet restore Web.Fiap.Carbono.sln
dotnet build Web.Fiap.Carbono.sln

# Read-only inventory; no MongoDB connection or policy is needed.
dotnet run --no-build --project Web.Fiap.Carbono.Migration -- \
  --inventory --oracle-config Web.Fiap.Carbono/appsettings.Development.json

# Validate mappings and destination without inserting anything.
dotnet run --no-build --project Web.Fiap.Carbono.Migration -- \
  Web.Fiap.Carbono.Migration/migration-policy.json \
  --oracle-config Web.Fiap.Carbono/appsettings.Development.json

# Apply only after reviewing the completed policy and passing the dry run.
dotnet run --no-build --project Web.Fiap.Carbono.Migration -- \
  Web.Fiap.Carbono.Migration/migration-policy.json --apply \
  --oracle-config Web.Fiap.Carbono/appsettings.Development.json

# Compare Oracle with the isolated migrated target.
dotnet run --no-build --project Web.Fiap.Carbono.Migration -- \
  Web.Fiap.Carbono.Migration/migration-policy.json --reconcile \
  --oracle-config Web.Fiap.Carbono/appsettings.Development.json
```

Alternatively, set `ConnectionStrings__OracleConnection` using local secret
configuration and omit `--oracle-config`. Environment configuration takes
precedence. Never commit the source connection string. Driver exception messages
and source documents are not printed because they may expose sensitive data.

Exit codes: `0` success, `1` infrastructure/configuration failure, `2` validation
or partial-write failure, and `130` cancellation. A partial write is reported with
collection, Oracle ID, and server error code. Cancellation or connection loss can
leave completed batches; neither triggers deletion or automatic rollback.

## Reviewed policy decisions

The checked-in policy was completed from the read-only Oracle inventory and
reviewed academic migration assumptions on 2026-09-05:

- `oracleTimeZone` is `America/Sao_Paulo`, matching the application environment
  in which the original code stored local `DateTime.Now` values. Ambiguous or
  invalid daylight-saving times still stop migration. UTC conversion is explicit.
- `migrationTimestampUtc` is fixed and unchanged on retries.
- `companyActiveDefault` is `true` because Oracle has no company active flag.
- `supplierCreatedAtUtc` is the fixed migration timestamp because Oracle has no
  supplier creation timestamp. The document labels this fallback explicitly.
- `factors`: one entry per Oracle ID, with `category`, positive `version`,
  `validFromUtc`, optional `validUntilUtc`, and `decisionNote`. These are target
  decisions, not recovered historical Oracle factor versions/validity periods.
- `stageCategories` maps source-only `AGRICOLA` and `INSUMOS` values to
  `MATERIA_PRIMA`; already supported source categories remain unchanged.
- `acceptReconstructedFactorSnapshots` is `true` after review of the limitation
  below.

Oracle contains no historical factor snapshots or original calculation-user
field. `fatorAplicado` is reconstructed from the currently referenced source
factor, labeled `snapshotReconstruido`, and frozen at import. Matching arithmetic
does not prove that this was the original historical factor. `calculadoPor` uses
the explicit unknown marker `NAO_REGISTRADO_NO_ORACLE`.

The persisted Oracle emission result is preserved as Decimal128. The mapper
checks compatibility against quantity × factor rounded to three decimal places,
matching the existing `NUMBER(12,3)` EF mapping; a mismatch requires historical
review and stops migration. It never silently recalculates or repairs a record.
Historic inactive factors can be imported; this is distinct from authorizing a
new calculation with an inactive factor.

`dadosAtividade` contains the supported type, original quantity, factor unit,
and `detalhamentoDisponivel: false`. Missing distance, load, recycled percentages,
and energy-source measurements are not invented. Oracle stage categories are
preserved unless a policy mapping is supplied. In particular, a transport stage
referencing an electricity factor remains visible as such. This legacy shape
does not replace the richer API activity variants or the flexible-schema demo.

Every root document includes `legacyId` and migration provenance. Batch/stage
IDs are preserved in embedded snapshots. Original source records are retained as
JSON text under migration metadata, including fields outside public DTOs. These
records may include company/supplier identity information: do not paste this
metadata into public screenshots. Creation timestamps are preserved when present;
missing modification timestamps are derived from creation and labeled. Batch
units are derived from the referenced product and labeled.

## Isolated target and retry behavior

The CLI targets only `127.0.0.1:27018/fiap_carbono`. It does not reuse application
MongoDB settings. Prepare a separate container if the previous disposable target
has stopped:

```bash
docker run --detach --rm --name fiap-carbono-phase12-migration \
  --publish 127.0.0.1:27018:27017 mongo:8.0.29-noble

mongosh "mongodb://127.0.0.1:27018/fiap_carbono" \
  --file database/mongodb/01-create-collections.js
mongosh "mongodb://127.0.0.1:27018/fiap_carbono" \
  --file database/mongodb/02-create-indexes.js
```

Check whether that named container already exists before creating it. It is
disposable (`--rm`); stopping it removes the container and its anonymous data
volume. Keep it running until reconciliation/evidence are finished, or recreate
it and rerun the migration. Never seed this target. The application database on
port 27017 remains separate. The repository tests now use an allocated port so
they do not conflict with this migration target.

The mapper derives deterministic ObjectIds from source identifier, collection,
and Oracle primary key, and detects collisions. The same inputs reproduce the
same mapping without a sixth collection. Generated codes use an `ORACLE-` prefix.
Source and policy must remain frozen from the first dry run through reconciliation;
run only one migration process against the isolated target at a time.

Before any insert, the writer checks all destination collections, business-key
indexes, strict/error validators, and every prepared document against the actual
validator using a read-only `$documents` aggregation. Seed/foreign documents or
changed previously imported documents stop the run. An identical retry retains
matching documents and inserts only missing ones, in dependency order and batches
of 100. It never replaces or deletes historical emissions. If source data changed
after a partial run, use a new clean isolated target or resolve the conflict
explicitly; do not overwrite prior evidence to force a retry.

The small academic source is loaded in memory. This is not a streaming production
export, and the executable does not provide distributed locking or an all-or-nothing
transaction across MongoDB collections. The `--reconcile` command compares
counts, Decimal128 totals, totals by legacy company/product/supplier, UTC dates,
references, legacy-ID sets, and every full planned document. Oracle counts below
ten per collection are migration evidence, not fulfillment of the separate FIAP
minimum.

## Verification status

On 2026-09-05 the permanent read-only inventory returned 5 companies, 5 products,
5 suppliers, 5 factors, 5 batches, 5 stages, and 9 emissions, totaling
`3085.45 kgCO2e`. The completed policy and dry run passed. The apply inserted
5/5/5/5/9 documents into the isolated target, and reconciliation passed every
count, Decimal128 total, per-entity total, date, reference, full-document, and
`legacyId` check. An identical retry inserted zero documents. Detailed values
are in the [reconciliation report](../docs/oracle-mongodb-reconciliation.md).

The migration tests use synthetic source records and disposable MongoDB 8.0.29
with the repository's actual validators/indexes. Coverage includes decimal BSON,
typed reads, missing references/policies, source errors, duplicate CNPJ, explicit
timezone conversion, dry-run isolation, idempotency, conflicting documents,
recovery from a real partial batch failure, and reconciliation failures. Run:

Verification on 2026-09-05: restore and build succeeded with zero warnings or
errors, 20/20 focused migration tests passed, and the full suite passed 107/107.

```bash
dotnet test Web.Fiap.Carbono.sln --filter 'FullyQualifiedName~Web.Fiap.Carbono.Tests.Migration'
dotnet test Web.Fiap.Carbono.sln
```
