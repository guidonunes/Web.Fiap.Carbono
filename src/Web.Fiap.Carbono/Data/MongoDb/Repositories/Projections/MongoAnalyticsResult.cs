using MongoDB.Bson;

namespace Web.Fiap.Carbono.Data.MongoDb.Repositories.Projections;

public sealed record EmissaoPorEtapaMongoResult(
    string Categoria,
    decimal TotalKgCO2e
);

public sealed record ProdutoPegadaMongoResult(
    ObjectId ProdutoId,
    decimal TotalKgCO2e,
    IReadOnlyList<EmissaoPorEtapaMongoResult> PorEtapa
);

public sealed record FornecedorRankingMongoResult(
    ObjectId FornecedorId,
    string? Codigo,
    string? Nome,
    decimal TotalKgCO2e,
    long QuantidadeEmissoes
);

public sealed record EmissaoMensalMongoResult(
    int Ano,
    int Mes,
    decimal TotalKgCO2e
);

public sealed record EmissaoPorProdutoMongoResult(
    ObjectId ProdutoId,
    decimal TotalKgCO2e
);

public sealed record EmissaoPorFornecedorMongoResult(
    ObjectId FornecedorId,
    decimal TotalKgCO2e
);

public sealed record EmissaoPorEscopoMongoResult(
    string Escopo,
    decimal TotalKgCO2e
);

public sealed record DashboardEmpresaMongoResult(
    ObjectId EmpresaId,
    decimal TotalKgCO2e,
    long QuantidadeEmissoes,
    IReadOnlyList<EmissaoMensalMongoResult> PorMes,
    IReadOnlyList<EmissaoPorProdutoMongoResult> PorProduto,
    IReadOnlyList<EmissaoPorFornecedorMongoResult> PorFornecedor,
    IReadOnlyList<EmissaoPorEscopoMongoResult> PorEscopo
);
