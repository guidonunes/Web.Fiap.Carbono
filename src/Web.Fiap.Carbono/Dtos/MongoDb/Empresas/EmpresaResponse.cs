using Web.Fiap.Carbono.Dtos.MongoDb.Common;

namespace Web.Fiap.Carbono.Dtos.MongoDb.Empresas;

public sealed class EmpresaResponse
{
    public string Id { get; init; } = string.Empty;

    public int? LegacyId { get; init; }

    public string Codigo { get; init; } = string.Empty;

    public string RazaoSocial { get; init; } = string.Empty;

    public string? NomeFantasia { get; init; }

    public string Cnpj { get; init; } = string.Empty;

    public string? Setor { get; init; }

    public bool Ativa { get; init; }

    public IReadOnlyList<MetaReducaoDto> MetasReducao { get; init; } =
        Array.Empty<MetaReducaoDto>();

    public GovernancaEsgDto? Governanca { get; init; }

    public DateTime CriadoEm { get; init; }

    public DateTime AtualizadoEm { get; init; }

    public int SchemaVersion { get; init; }
}
