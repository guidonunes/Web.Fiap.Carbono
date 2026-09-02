using MongoDB.Bson;
using MongoDB.Driver;
using Web.Fiap.Carbono.Data.MongoDb;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Exceptions;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Interfaces;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Projections;
using Web.Fiap.Carbono.Models.Documents;
using MongoDuplicateKeyException = Web.Fiap.Carbono.Data.MongoDb.Repositories.Exceptions.MongoDuplicateKeyException;

namespace Web.Fiap.Carbono.Data.MongoDb.Repositories;

public sealed class MongoEmissaoCarbonoRepository
    : IMongoEmissaoCarbonoRepository
{
    private readonly IMongoCollection<EmissaoCarbonoDocument>
        _emissoes;

    public MongoEmissaoCarbonoRepository(
        MongoDbContext context
    )
    {
        _emissoes = context.EmissoesCarbono;
    }

    public async Task<EmissaoCarbonoDocument> CreateAsync(
        EmissaoCarbonoDocument document,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _emissoes.InsertOneAsync(
                document,
                cancellationToken: cancellationToken
            );
        }
        catch (MongoWriteException exception)
            when (MongoRepositoryRules.IsDuplicateKey(exception))
        {
            throw new MongoDuplicateKeyException(
                "Já existe uma emissão com o mesmo código."
            );
        }

        return document;
    }

    public async Task<EmissaoCarbonoDocument?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(id, nameof(id));

        var emissao = await _emissoes
            .Find(item => item.Id == objectId)
            .FirstOrDefaultAsync(cancellationToken);

        return emissao;
    }

    public async Task<EmissaoCarbonoDocument?> GetByCodigoAsync(
        string codigo,
        CancellationToken cancellationToken
    )
    {
        var emissao = await _emissoes
            .Find(emissao => emissao.Codigo == codigo)
            .FirstOrDefaultAsync(cancellationToken);

        return emissao;
    }

    public Task<List<EmissaoCarbonoDocument>> GetAllAsync(
        CancellationToken cancellationToken
    )
    {
        return _emissoes
            .Find(Builders<EmissaoCarbonoDocument>.Filter.Empty)
            .Sort(StableEmissionSort())
            .ToListAsync(cancellationToken);
    }

    async Task<IReadOnlyList<EmissaoCarbonoDocument>>
        IMongoEmissaoCarbonoRepository.GetAllAsync(
            CancellationToken cancellationToken
        )
    {
        return await GetAllAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        string id,
        EmissaoCarbonoDocument document,
        CancellationToken cancellationToken
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(id, nameof(id));
        EnsureMatchingId(objectId, document.Id);

        try
        {
            var result = await _emissoes.ReplaceOneAsync(
                emissao => emissao.Id == objectId,
                document,
                new ReplaceOptions { IsUpsert = false },
                cancellationToken
            );

            return result.IsAcknowledged &&
                result.MatchedCount == 1;
        }
        catch (MongoWriteException exception)
            when (MongoRepositoryRules.IsDuplicateKey(exception))
        {
            throw new MongoDuplicateKeyException(
                "Já existe uma emissão com o mesmo código."
            );
        }
    }

    public async Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(id, nameof(id));

        var result = await _emissoes.DeleteOneAsync(
            emissao => emissao.Id == objectId,
            cancellationToken
        );

        return result.IsAcknowledged &&
            result.DeletedCount == 1;
    }

    public async Task<bool> ExistsAsync(
        string id,
        CancellationToken cancellationToken
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(id, nameof(id));

        var count = await _emissoes.CountDocumentsAsync(
            emissao => emissao.Id == objectId,
            new CountOptions { Limit = 1 },
            cancellationToken
        );

        return count == 1;
    }

    public Task<bool> ExistsByEmpresaIdAsync(
        string empresaId,
        CancellationToken cancellationToken = default
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(
            empresaId,
            nameof(empresaId)
        );

        return ExistsAsync(
            Builders<EmissaoCarbonoDocument>.Filter.Eq(
                emissao => emissao.EmpresaId,
                objectId
            ),
            cancellationToken
        );
    }

    public Task<bool> ExistsByProdutoIdAsync(
        string produtoId,
        CancellationToken cancellationToken = default
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(
            produtoId,
            nameof(produtoId)
        );

        return ExistsAsync(
            Builders<EmissaoCarbonoDocument>.Filter.Eq(
                emissao => emissao.ProdutoId,
                objectId
            ),
            cancellationToken
        );
    }

    public Task<bool> ExistsByFornecedorIdAsync(
        string fornecedorId,
        CancellationToken cancellationToken = default
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(
            fornecedorId,
            nameof(fornecedorId)
        );

        return ExistsAsync(
            Builders<EmissaoCarbonoDocument>.Filter.Eq(
                emissao => emissao.FornecedorId,
                objectId
            ),
            cancellationToken
        );
    }

    public Task<bool> ExistsByFatorEmissaoIdAsync(
        string fatorEmissaoId,
        CancellationToken cancellationToken = default
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(
            fatorEmissaoId,
            nameof(fatorEmissaoId)
        );

        return ExistsAsync(
            Builders<EmissaoCarbonoDocument>.Filter.Eq(
                emissao => emissao.FatorEmissaoId,
                objectId
            ),
            cancellationToken
        );
    }

    public async Task<MongoPagedResult<EmissaoCarbonoDocument>>
        GetPaginatedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken
        )
    {
        MongoRepositoryRules.ValidatePagination(
            pageNumber,
            pageSize
        );

        var totalItems = await _emissoes.CountDocumentsAsync(
            Builders<EmissaoCarbonoDocument>.Filter.Empty,
            cancellationToken: cancellationToken
        );

        var skip = (pageNumber - 1) * pageSize;

        var items = await _emissoes
            .Find(Builders<EmissaoCarbonoDocument>.Filter.Empty)
            .Sort(StableEmissionSort())
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return new MongoPagedResult<EmissaoCarbonoDocument>(
            items,
            totalItems,
            pageNumber,
            pageSize
        );
    }

    public async Task<ProdutoPegadaMongoResult?>
        GetProductFootprintAsync(
            string produtoId,
            CancellationToken cancellationToken
        )
    {
        var produtoObjectId = MongoRepositoryRules.ParseObjectId(
            produtoId,
            nameof(produtoId)
        );

        var stages = new BsonDocument[]
        {
            new("$match", new BsonDocument(
                "produtoId",
                produtoObjectId
            )),

            new("$facet", new BsonDocument
            {
                {
                    "total",
                    new BsonArray
                    {
                        new BsonDocument(
                            "$group",
                            new BsonDocument
                            {
                                { "_id", BsonNull.Value },
                                {
                                    "totalKgCO2e",
                                    new BsonDocument(
                                        "$sum",
                                        "$quantidadeEmitidaKgCO2e"
                                    )
                                }
                            }
                        )
                    }
                },
                {
                    "porEtapa",
                    new BsonArray
                    {
                        new BsonDocument(
                            "$group",
                            new BsonDocument
                            {
                                {
                                    "_id",
                                    "$etapa.categoria"
                                },
                                {
                                    "totalKgCO2e",
                                    new BsonDocument(
                                        "$sum",
                                        "$quantidadeEmitidaKgCO2e"
                                    )
                                }
                            }
                        ),
                        new BsonDocument(
                            "$sort",
                            new BsonDocument("_id", 1)
                        )
                    }
                }
            })
        };

        var result = await AggregateOneAsync(
            stages,
            cancellationToken
        );

        if (result is null)
        {
            return null;
        }

        var totalDocuments =
            result["total"].AsBsonArray;

        if (totalDocuments.Count == 0)
        {
            return null;
        }

        var total = ReadDecimal(
            totalDocuments[0]
                .AsBsonDocument["totalKgCO2e"]
        );

        var porEtapa = result["porEtapa"]
            .AsBsonArray
            .Select(item =>
            {
                var document = item.AsBsonDocument;

                return new EmissaoPorEtapaMongoResult(
                    document["_id"].AsString,
                    ReadDecimal(
                        document["totalKgCO2e"]
                    )
                );
            })
            .ToList();

        return new ProdutoPegadaMongoResult(
            produtoObjectId,
            total,
            porEtapa
        );
    }

    public async Task<
        MongoPagedResult<FornecedorRankingMongoResult>
    > GetSupplierRankingAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken
    )
    {
        MongoRepositoryRules.ValidatePagination(
            pageNumber,
            pageSize
        );

        var skip = (pageNumber - 1) * pageSize;

        var stages = new BsonDocument[]
        {
            new("$group", new BsonDocument
            {
                { "_id", "$fornecedorId" },
                {
                    "totalKgCO2e",
                    new BsonDocument(
                        "$sum",
                        "$quantidadeEmitidaKgCO2e"
                    )
                },
                {
                    "quantidadeEmissoes",
                    new BsonDocument("$sum", 1)
                }
            }),

            new("$lookup", new BsonDocument
            {
                {
                    "from",
                    MongoDbContext.FornecedoresCollectionName
                },
                { "localField", "_id" },
                { "foreignField", "_id" },
                { "as", "fornecedor" }
            }),

            new("$set", new BsonDocument
            {
                {
                    "fornecedorCodigo",
                    new BsonDocument(
                        "$arrayElemAt",
                        new BsonArray
                        {
                            "$fornecedor.codigo",
                            0
                        }
                    )
                },
                {
                    "fornecedorNome",
                    new BsonDocument(
                        "$arrayElemAt",
                        new BsonArray
                        {
                            "$fornecedor.nomeFantasia",
                            0
                        }
                    )
                }
            }),

            new("$sort", new BsonDocument
            {
                { "totalKgCO2e", -1 },
                { "_id", 1 }
            }),

            new("$facet", new BsonDocument
            {
                {
                    "items",
                    new BsonArray
                    {
                        new BsonDocument(
                            "$skip",
                            skip
                        ),
                        new BsonDocument(
                            "$limit",
                            pageSize
                        )
                    }
                },
                {
                    "total",
                    new BsonArray
                    {
                        new BsonDocument(
                            "$count",
                            "totalItems"
                        )
                    }
                }
            })
        };

        var result = await AggregateOneAsync(
            stages,
            cancellationToken
        );

        if (result is null)
        {
            return new MongoPagedResult<
                FornecedorRankingMongoResult
            >(
                Array.Empty<FornecedorRankingMongoResult>(),
                0,
                pageNumber,
                pageSize
            );
        }

        var items = result["items"]
            .AsBsonArray
            .Select(item =>
            {
                var document = item.AsBsonDocument;

                return new FornecedorRankingMongoResult(
                    document["_id"].AsObjectId,
                    ReadOptionalString(
                        document,
                        "fornecedorCodigo"
                    ),
                    ReadOptionalString(
                        document,
                        "fornecedorNome"
                    ),
                    ReadDecimal(
                        document["totalKgCO2e"]
                    ),
                    ReadLong(
                        document["quantidadeEmissoes"]
                    )
                );
            })
            .ToList();

        var totalDocuments =
            result["total"].AsBsonArray;

        var totalItems = totalDocuments.Count == 0
            ? 0
            : ReadLong(
                totalDocuments[0]
                    .AsBsonDocument["totalItems"]
            );

        return new MongoPagedResult<
            FornecedorRankingMongoResult
        >(
            items,
            totalItems,
            pageNumber,
            pageSize
        );
    }

    public async Task<DashboardEmpresaMongoResult?>
        GetCompanyDashboardAsync(
            string empresaId,
            CancellationToken cancellationToken
        )
    {
        var empresaObjectId = MongoRepositoryRules.ParseObjectId(
            empresaId,
            nameof(empresaId)
        );

        var stages = new BsonDocument[]
        {
            new("$match", new BsonDocument(
                "empresaId",
                empresaObjectId
            )),

            new("$facet", new BsonDocument
            {
                {
                    "resumo",
                    new BsonArray
                    {
                        new BsonDocument(
                            "$group",
                            new BsonDocument
                            {
                                { "_id", BsonNull.Value },
                                {
                                    "totalKgCO2e",
                                    new BsonDocument(
                                        "$sum",
                                        "$quantidadeEmitidaKgCO2e"
                                    )
                                },
                                {
                                    "quantidadeEmissoes",
                                    new BsonDocument("$sum", 1)
                                }
                            }
                        )
                    }
                },
                {
                    "porMes",
                    new BsonArray
                    {
                        new BsonDocument(
                            "$group",
                            new BsonDocument
                            {
                                {
                                    "_id",
                                    new BsonDocument
                                    {
                                        {
                                            "ano",
                                            new BsonDocument(
                                                "$year",
                                                "$dataEmissao"
                                            )
                                        },
                                        {
                                            "mes",
                                            new BsonDocument(
                                                "$month",
                                                "$dataEmissao"
                                            )
                                        }
                                    }
                                },
                                {
                                    "totalKgCO2e",
                                    new BsonDocument(
                                        "$sum",
                                        "$quantidadeEmitidaKgCO2e"
                                    )
                                }
                            }
                        ),
                        new BsonDocument(
                            "$sort",
                            new BsonDocument
                            {
                                { "_id.ano", 1 },
                                { "_id.mes", 1 }
                            }
                        )
                    }
                },
                {
                    "porProduto",
                    new BsonArray
                    {
                        new BsonDocument(
                            "$group",
                            new BsonDocument
                            {
                                { "_id", "$produtoId" },
                                {
                                    "totalKgCO2e",
                                    new BsonDocument(
                                        "$sum",
                                        "$quantidadeEmitidaKgCO2e"
                                    )
                                }
                            }
                        ),
                        new BsonDocument(
                            "$sort",
                            new BsonDocument
                            {
                                { "totalKgCO2e", -1 },
                                { "_id", 1 }
                            }
                        )
                    }
                },
                {
                    "porFornecedor",
                    new BsonArray
                    {
                        new BsonDocument(
                            "$group",
                            new BsonDocument
                            {
                                { "_id", "$fornecedorId" },
                                {
                                    "totalKgCO2e",
                                    new BsonDocument(
                                        "$sum",
                                        "$quantidadeEmitidaKgCO2e"
                                    )
                                }
                            }
                        ),
                        new BsonDocument(
                            "$sort",
                            new BsonDocument
                            {
                                { "totalKgCO2e", -1 },
                                { "_id", 1 }
                            }
                        )
                    }
                },
                {
                    "porEscopo",
                    new BsonArray
                    {
                        new BsonDocument(
                            "$group",
                            new BsonDocument
                            {
                                {
                                    "_id",
                                    "$fatorAplicado.escopo"
                                },
                                {
                                    "totalKgCO2e",
                                    new BsonDocument(
                                        "$sum",
                                        "$quantidadeEmitidaKgCO2e"
                                    )
                                }
                            }
                        ),
                        new BsonDocument(
                            "$sort",
                            new BsonDocument("_id", 1)
                        )
                    }
                }
            })
        };

        var result = await AggregateOneAsync(
            stages,
            cancellationToken
        );

        if (result is null ||
            result["resumo"].AsBsonArray.Count == 0)
        {
            return null;
        }

        var resumo = result["resumo"]
            .AsBsonArray[0]
            .AsBsonDocument;

        var porMes = result["porMes"]
            .AsBsonArray
            .Select(item =>
            {
                var document = item.AsBsonDocument;
                var id = document["_id"].AsBsonDocument;

                return new EmissaoMensalMongoResult(
                    id["ano"].ToInt32(),
                    id["mes"].ToInt32(),
                    ReadDecimal(
                        document["totalKgCO2e"]
                    )
                );
            })
            .ToList();

        var porProduto = result["porProduto"]
            .AsBsonArray
            .Select(item =>
            {
                var document = item.AsBsonDocument;

                return new EmissaoPorProdutoMongoResult(
                    document["_id"].AsObjectId,
                    ReadDecimal(
                        document["totalKgCO2e"]
                    )
                );
            })
            .ToList();

        var porFornecedor = result["porFornecedor"]
            .AsBsonArray
            .Select(item =>
            {
                var document = item.AsBsonDocument;

                return new EmissaoPorFornecedorMongoResult(
                    document["_id"].AsObjectId,
                    ReadDecimal(
                        document["totalKgCO2e"]
                    )
                );
            })
            .ToList();

        var porEscopo = result["porEscopo"]
            .AsBsonArray
            .Select(item =>
            {
                var document = item.AsBsonDocument;

                return new EmissaoPorEscopoMongoResult(
                    document["_id"].AsString,
                    ReadDecimal(
                        document["totalKgCO2e"]
                    )
                );
            })
            .ToList();

        return new DashboardEmpresaMongoResult(
            empresaObjectId,
            ReadDecimal(resumo["totalKgCO2e"]),
            ReadLong(resumo["quantidadeEmissoes"]),
            porMes,
            porProduto,
            porFornecedor,
            porEscopo
        );
    }

    private async Task<BsonDocument?> AggregateOneAsync(
        IEnumerable<BsonDocument> stages,
        CancellationToken cancellationToken
    )
    {
        var pipeline =
            PipelineDefinition<
                EmissaoCarbonoDocument,
                BsonDocument
            >.Create(stages);

        var result = await _emissoes
            .Aggregate<BsonDocument>(pipeline)
            .FirstOrDefaultAsync(cancellationToken);

        return result;
    }

    private async Task<bool> ExistsAsync(
        FilterDefinition<EmissaoCarbonoDocument> filter,
        CancellationToken cancellationToken
    )
    {
        var count = await _emissoes.CountDocumentsAsync(
            filter,
            new CountOptions { Limit = 1 },
            cancellationToken
        );

        return count == 1;
    }

    private static SortDefinition<EmissaoCarbonoDocument>
        StableEmissionSort()
    {
        return Builders<EmissaoCarbonoDocument>
            .Sort
            .Combine(
                Builders<EmissaoCarbonoDocument>
                    .Sort
                    .Descending(
                        emissao => emissao.DataEmissao
                    ),
                Builders<EmissaoCarbonoDocument>
                    .Sort
                    .Ascending(
                        emissao => emissao.Id
                    )
            );
    }

    private static decimal ReadDecimal(BsonValue value)
    {
        return value.BsonType switch
        {
            BsonType.Decimal128 => Decimal128.ToDecimal(
                value.AsDecimal128
            ),
            BsonType.Int32 => value.AsInt32,
            BsonType.Int64 => value.AsInt64,
            BsonType.Double => checked((decimal)value.AsDouble),
            _ => throw new FormatException(
                $"Valor BSON {value.BsonType} não é decimal."
            )
        };
    }

    private static long ReadLong(BsonValue value)
    {
        return value.BsonType switch
        {
            BsonType.Int32 => value.AsInt32,
            BsonType.Int64 => value.AsInt64,
            BsonType.Decimal128 => checked(
                (long)Decimal128.ToDecimal(value.AsDecimal128)
            ),
            BsonType.Double => checked((long)value.AsDouble),
            _ => throw new FormatException(
                $"Valor BSON {value.BsonType} não é inteiro."
            )
        };
    }

    private static string? ReadOptionalString(
        BsonDocument document,
        string fieldName
    )
    {
        return document.TryGetValue(fieldName, out var value) &&
            value.IsString
                ? value.AsString
                : null;
    }

    private static void EnsureMatchingId(
        ObjectId requestedId,
        ObjectId documentId
    )
    {
        if (requestedId != documentId)
        {
            throw new ArgumentException(
                "O Id do documento deve corresponder ao Id da atualização."
            );
        }
    }
}
