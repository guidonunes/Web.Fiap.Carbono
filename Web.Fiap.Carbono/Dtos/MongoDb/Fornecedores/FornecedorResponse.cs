using Web.Fiap.Carbono.Dtos.MongoDb.Common;

namespace Web.Fiap.Carbono.Dtos.MongoDb.Fornecedores;

public sealed class FornecedorResponse
{
    public string Id { get; init; } = string.Empty;

    public int? LegacyId { get; init; }

    public string Codigo { get; init; } = string.Empty;

    public string RazaoSocial { get; init; } = string.Empty;

    public string? NomeFantasia { get; init; }

    public string Cnpj { get; init; } = string.Empty;

    public bool Ativo { get; init; }

    public IReadOnlyList<string> CategoriasAtuacao {
        get;
        init;
    } = Array.Empty<string>();

    public IReadOnlyList<CertificacaoDto> Certificacoes {
        get;
        init;
    } = Array.Empty<CertificacaoDto>();

    public IndicadoresSociaisDto? IndicadoresSociais {
        get;
        init;
    }

    public ConformidadeAmbientalDto? ConformidadeAmbiental {
        get;
        init;
    }

    public string? StatusAuditoria { get; init; }

    public string? NivelRiscoEsg { get; init; }

    public DateTime CriadoEm { get; init; }

    public DateTime AtualizadoEm { get; init; }

    public int SchemaVersion { get; init; }
}
