using MongoDB.Bson;
using MongoDB.Driver;
using static Web.Fiap.Carbono.Migration.MigrationPlanner;

namespace Web.Fiap.Carbono.Migration;

public sealed class MongoMigrationWriter(IMongoDatabase database)
{
    // No schema/index changes at runtime. The existing 01/02 scripts initialize the target.
    public async Task<Dictionary<string, int>> RunAsync(
        Dictionary<string, List<BsonDocument>> plan, bool apply,
        bool acceptReconstructedSnapshots, int batchSize = 100,
        CancellationToken token = default)
    {
        Check(batchSize is >= 1 and <= 1000, "Batch size must be between 1 and 1000.");
        Check(!apply || acceptReconstructedSnapshots,
            "Applying requires acknowledgement of reconstructed factor snapshots.");
        Check(plan.Keys.Order().SequenceEqual(Collections.Order()), "Invalid migration collection set.");
        using var cursor = await database.ListCollectionsAsync(cancellationToken: token);
        var info = await cursor.ToListAsync(token);
        Check(info.Select(x => x["name"].AsString).Where(x => !x.StartsWith("system.", StringComparison.Ordinal))
            .Order().SequenceEqual(Collections.Order()), "Target must contain exactly the five initialized collections.");
        var pending = new Dictionary<string, List<BsonDocument>>();
        foreach (var name in Collections)
        {
            var collection = database.GetCollection<BsonDocument>(name);
            var documents = plan[name];
            var expected = documents.ToDictionary(x => x["_id"].AsObjectId);
            var existing = await collection.Find(FilterDefinition<BsonDocument>.Empty).ToListAsync(token);
            foreach (var saved in existing)
            {
                Check(expected.TryGetValue(saved["_id"].AsObjectId, out var candidate),
                    $"{name}: unrelated target document; use a clean isolated database.");
                Check(saved.Equals(candidate), $"{name}, legacyId={saved.GetValue("legacyId", -1)}: " +
                    "existing document differs; source and policy must remain unchanged on retry.");
            }
            var existingIds = existing.Select(x => x["_id"].AsObjectId).ToHashSet();
            pending[name] = documents.Where(x => !existingIds.Contains(x["_id"].AsObjectId)).ToList();

            using var indexesCursor = await collection.Indexes.ListAsync(token);
            var indexes = await indexesCursor.ToListAsync(token);
            var requiredKeys = name switch
            {
                "empresas" or "fornecedores" => new[] { new BsonDocument("cnpj", 1), new BsonDocument("codigo", 1) },
                "produtos" => [new BsonDocument { ["empresaId"] = 1, ["codigo"] = 1 }],
                "fatores_emissao" => [new BsonDocument { ["codigo"] = 1, ["versao"] = 1 }],
                _ => [new BsonDocument("codigo", 1)]
            };
            foreach (var key in requiredKeys)
                Check(indexes.Any(x => x["key"].Equals(key) && x.GetValue("unique", false).AsBoolean &&
                        (!x.Contains("partialFilterExpression") ||
                         (name == "emissoes_carbono" && x["partialFilterExpression"].Equals(
                             new BsonDocument("codigo", new BsonDocument("$type", "string")))))),
                    $"{name}: required unique index is missing or incompatible.");

            var options = info.Single(x => x["name"] == name)["options"].AsBsonDocument;
            Check(options.GetValue("validationLevel", "strict") == "strict" &&
                  options.GetValue("validationAction", "error") == "error",
                $"{name}: strict/error validation is required.");
            Check(options.TryGetValue("validator", out var validator) &&
                  validator.IsBsonDocument && validator.AsBsonDocument.ElementCount > 0,
                $"{name}: validator is missing.");

            // Collectionless aggregation evaluates the actual validator without an insert.
            // Small batches avoid oversized command documents; output contains IDs only.
            foreach (var batch in documents.Chunk(50))
            {
                var result = await database.RunCommandAsync<BsonDocument>(new BsonDocument
                {
                    ["aggregate"] = 1,
                    ["pipeline"] = new BsonArray
                    {
                        new BsonDocument("$documents", new BsonArray(batch)),
                        new BsonDocument("$match", new BsonDocument("$nor", new BsonArray { validator! })),
                        new BsonDocument("$project", new BsonDocument { ["_id"] = 0, ["legacyId"] = 1 })
                    },
                    ["cursor"] = new BsonDocument("batchSize", 100)
                }, cancellationToken: token);
                var invalid = result["cursor"]["firstBatch"].AsBsonArray;
                Check(invalid.Count == 0, $"{name}: validator rejected legacy IDs " +
                    string.Join(", ", invalid.Select(x => x["legacyId"])));
            }
        }

        // Every collection has now passed preflight. No source/target changes are allowed
        // during apply; run exactly one migration process against this isolated target.
        if (apply)
            foreach (var name in Collections)
                foreach (var batch in pending[name].Chunk(batchSize))
                {
                    try
                    {
                        await database.GetCollection<BsonDocument>(name).InsertManyAsync(batch,
                            new InsertManyOptions { IsOrdered = true }, token);
                    }
                    catch (MongoBulkWriteException<BsonDocument> e)
                    {
                        var details = e.WriteErrors.Select(x =>
                            $"{name}, legacyId={batch[x.Index]["legacyId"]}: MongoDB write error {x.Code}.").ToList();
                        if (e.WriteConcernError is not null)
                            details.Add($"{name}: write concern error {e.WriteConcernError.Code}; outcome may be partial.");
                        details.Add("Stopped after partial failure. Retry the same source and policy; completed inserts are retained.");
                        throw new MigrationValidationException(details);
                    }
                }
        return pending.ToDictionary(x => x.Key, x => x.Value.Count);
    }
}
