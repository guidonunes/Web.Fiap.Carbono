using Web.Fiap.Carbono.Dtos.MongoDb.Common;

namespace Web.Fiap.Carbono.Dtos.MongoDb.Produtos;

public sealed class ProdutoResponse
{
    public string Id { get; init; } = string.Empty;

    public int? LegacyId { get; init; }

    public string EmpresaId { get; init; } = string.Empty;

    public string Codigo { get; init; } = string.Empty;

    public string Nome { get; init; } = string.Empty;

    public string? Categoria { get; init; }

    public string UnidadeFuncional { get; init; } = string.Empty;

    public bool Ativo { get; init; }

    public AtributosAmbientaisDto? AtributosAmbientais {
        get;
        init;
    }

    public DateTime CriadoEm { get; init; }

    public DateTime AtualizadoEm { get; init; }

    public int SchemaVersion { get; init; }
}
