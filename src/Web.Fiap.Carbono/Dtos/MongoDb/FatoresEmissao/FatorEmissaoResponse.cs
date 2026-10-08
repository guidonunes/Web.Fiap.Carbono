namespace Web.Fiap.Carbono.Dtos.MongoDb.FatoresEmissao;

public sealed class FatorEmissaoResponse
{
    public string Id { get; init; } = string.Empty;

    public int? LegacyId { get; init; }

    public string Codigo { get; init; } = string.Empty;

    public string Nome { get; init; } = string.Empty;

    public string Categoria { get; init; } = string.Empty;

    public decimal Valor { get; init; }

    public string UnidadeBase { get; init; } = string.Empty;

    public string Escopo { get; init; } = string.Empty;

    public int Versao { get; init; }

    public string? FonteReferencia { get; init; }

    public string? Metodologia { get; init; }

    public DateTime ValidoDe { get; init; }

    public DateTime? ValidoAte { get; init; }

    public bool Ativo { get; init; }

    public DateTime CriadoEm { get; init; }

    public DateTime AtualizadoEm { get; init; }

    public int SchemaVersion { get; init; }
}
