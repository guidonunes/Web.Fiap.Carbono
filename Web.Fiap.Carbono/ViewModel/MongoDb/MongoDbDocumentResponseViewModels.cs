namespace Web.Fiap.Carbono.ViewModel.MongoDb;

// These response models deliberately contain no MongoDB.Driver types or BSON attributes.
public sealed class EmpresaMongoResponseViewModel
{
    public string Id { get; init; } = string.Empty;
    public int? LegacyId { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string RazaoSocial { get; init; } = string.Empty;
    public string? NomeFantasia { get; init; }
    public string Cnpj { get; init; } = string.Empty;
    public string? Setor { get; init; }
    public bool Ativa { get; init; }
    public IReadOnlyList<MetaReducaoResponseViewModel> MetasReducao { get; init; } = [];
    public GovernancaEsgResponseViewModel? Governanca { get; init; }
    public DateTime CriadoEm { get; init; }
    public DateTime AtualizadoEm { get; init; }
    public int SchemaVersion { get; init; }
}

public sealed class ProdutoMongoResponseViewModel
{
    public string Id { get; init; } = string.Empty;
    public int? LegacyId { get; init; }
    public string EmpresaId { get; init; } = string.Empty;
    public string Codigo { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string? Categoria { get; init; }
    public string UnidadeFuncional { get; init; } = string.Empty;
    public bool Ativo { get; init; }
    public AtributosAmbientaisResponseViewModel? AtributosAmbientais { get; init; }
    public DateTime CriadoEm { get; init; }
    public DateTime AtualizadoEm { get; init; }
    public int SchemaVersion { get; init; }
}

public sealed class FornecedorMongoResponseViewModel
{
    public string Id { get; init; } = string.Empty;
    public int? LegacyId { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string RazaoSocial { get; init; } = string.Empty;
    public string? NomeFantasia { get; init; }
    public string Cnpj { get; init; } = string.Empty;
    public bool Ativo { get; init; }
    public IReadOnlyList<string> CategoriasAtuacao { get; init; } = [];
    public IReadOnlyList<CertificacaoResponseViewModel> Certificacoes { get; init; } = [];
    public IndicadoresSociaisResponseViewModel? IndicadoresSociais { get; init; }
    public ConformidadeAmbientalResponseViewModel? ConformidadeAmbiental { get; init; }
    public string? StatusAuditoria { get; init; }
    public string? NivelRiscoEsg { get; init; }
    public DateTime CriadoEm { get; init; }
    public DateTime AtualizadoEm { get; init; }
    public int SchemaVersion { get; init; }
}

public sealed class FatorEmissaoMongoResponseViewModel
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

public sealed class EmissaoCarbonoMongoResponseViewModel
{
    public string Id { get; init; } = string.Empty;
    public int? LegacyId { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string EmpresaId { get; init; } = string.Empty;
    public string ProdutoId { get; init; } = string.Empty;
    public string FornecedorId { get; init; } = string.Empty;
    public string FatorEmissaoId { get; init; } = string.Empty;
    public LoteSnapshotResponseViewModel Lote { get; init; } = new();
    public EtapaSnapshotResponseViewModel Etapa { get; init; } = new();
    public decimal QuantidadeAtividade { get; init; }
    public IReadOnlyDictionary<string, object?> DadosAtividade { get; init; } = new Dictionary<string, object?>();
    public FatorEmissaoSnapshotResponseViewModel FatorAplicado { get; init; } = new();
    public decimal QuantidadeEmitidaKgCO2e { get; init; }
    public string MetodoCalculo { get; init; } = string.Empty;
    public string? FonteEmissao { get; init; }
    public string? Observacao { get; init; }
    public string CalculadoPor { get; init; } = string.Empty;
    public DateTime DataEmissao { get; init; }
    public DateTime? RevisadoEm { get; init; }
    public string? RevisadoPor { get; init; }
    public DateTime CriadoEm { get; init; }
    public DateTime AtualizadoEm { get; init; }
    public int SchemaVersion { get; init; }
}

public sealed class MetaReducaoResponseViewModel
{
    public string Tipo { get; init; } = string.Empty;
    public int AnoBase { get; init; }
    public int AnoMeta { get; init; }
    public decimal PercentualReducao { get; init; }
}

public sealed class GovernancaEsgResponseViewModel
{
    public string ResponsavelEsg { get; init; } = string.Empty;
    public bool ComiteEsg { get; init; }
    public string FrequenciaAuditoria { get; init; } = string.Empty;
    public bool? RelatorioPublico { get; init; }
    public bool? CanalDenuncias { get; init; }
    public bool? ConselhoSupervisao { get; init; }
}

public sealed class AtributosAmbientaisResponseViewModel
{
    public IReadOnlyList<MaterialProdutoResponseViewModel> Materiais { get; init; } = [];
    public decimal? PercentualReciclavel { get; init; }
    public decimal? PercentualMaterialReciclado { get; init; }
    public string? Embalagem { get; init; }
    public string? SeloSustentabilidade { get; init; }
    public bool? CompensacaoCarbono { get; init; }
    public bool? OrigemAgriculturaOrganica { get; init; }
    public int? VidaUtilAnos { get; init; }
    public IReadOnlyDictionary<string, object?> CamposAdicionais { get; init; } = new Dictionary<string, object?>();
}

public sealed class MaterialProdutoResponseViewModel
{
    public string Nome { get; init; } = string.Empty;
    public decimal PercentualComposicao { get; init; }
    public bool? OrigemRenovavel { get; init; }
    public bool? OrigemReciclada { get; init; }
}

public sealed class CertificacaoResponseViewModel
{
    public string Nome { get; init; } = string.Empty;
    public string Emissor { get; init; } = string.Empty;
    public DateTime? ValidaAte { get; init; }
}

public sealed class IndicadoresSociaisResponseViewModel
{
    public int AcidentesUltimos12Meses { get; init; }
    public decimal? PercentualMulheresLideranca { get; init; }
    public bool? PossuiProgramaDiversidade { get; init; }
}

public sealed class ConformidadeAmbientalResponseViewModel
{
    public bool PossuiLicenca { get; init; }
    public int OcorrenciasUltimos12Meses { get; init; }
    public bool? DescarteMonitorado { get; init; }
    public decimal? PercentualMaterialReciclado { get; init; }
    public DateTime? DataUltimaAuditoria { get; init; }
    public IReadOnlyDictionary<string, object?> CamposAdicionais { get; init; } = new Dictionary<string, object?>();
}

public sealed class LoteSnapshotResponseViewModel
{
    public string Codigo { get; init; } = string.Empty;
    public decimal QuantidadeProduzida { get; init; }
    public string Unidade { get; init; } = string.Empty;
    public DateTime DataProducao { get; init; }
}

public sealed class EtapaSnapshotResponseViewModel
{
    public string Nome { get; init; } = string.Empty;
    public int Ordem { get; init; }
    public string Categoria { get; init; } = string.Empty;
    public string? Local { get; init; }
}

public sealed class FatorEmissaoSnapshotResponseViewModel
{
    public string Codigo { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public string UnidadeBase { get; init; } = string.Empty;
    public string Escopo { get; init; } = string.Empty;
    public int Versao { get; init; }
    public string? FonteReferencia { get; init; }
    public string? Metodologia { get; init; }
}
