using System.ComponentModel.DataAnnotations;

namespace Web.Fiap.Carbono.Dtos.MongoDb.Common;

public sealed class MetaReducaoDto
{
    [Required]
    [StringLength(50)]
    public string Tipo { get; init; } = string.Empty;

    [Range(1900, 2200)]
    public int AnoBase { get; init; }

    [Range(1900, 2200)]
    public int AnoMeta { get; init; }

    [Range(typeof(decimal), "0", "100")]
    public decimal PercentualReducao { get; init; }
}

public sealed class GovernancaEsgDto
{
    [Required]
    [StringLength(150)]
    public string ResponsavelEsg { get; init; } = string.Empty;

    public bool ComiteEsg { get; init; }

    [Required]
    [StringLength(30)]
    public string FrequenciaAuditoria { get; init; } = string.Empty;

    public bool? RelatorioPublico { get; init; }

    public bool? CanalDenuncias { get; init; }

    public bool? ConselhoSupervisao { get; init; }
}

public sealed class AtributosAmbientaisDto
{
    public List<MaterialProdutoDto> Materiais { get; init; } =
        new();

    [Range(typeof(decimal), "0", "100")]
    public decimal? PercentualReciclavel { get; init; }

    [Range(typeof(decimal), "0", "100")]
    public decimal? PercentualMaterialReciclado { get; init; }

    [StringLength(100)]
    public string? Embalagem { get; init; }

    [StringLength(100)]
    public string? SeloSustentabilidade { get; init; }

    public bool? CompensacaoCarbono { get; init; }

    public bool? OrigemAgriculturaOrganica { get; init; }

    [Range(0, 200)]
    public int? VidaUtilAnos { get; init; }
}

public sealed class MaterialProdutoDto
{
    [Required]
    [StringLength(150)]
    public string Nome { get; init; } = string.Empty;

    [Range(typeof(decimal), "0", "100")]
    public decimal PercentualComposicao { get; init; }

    public bool? OrigemRenovavel { get; init; }

    public bool? OrigemReciclada { get; init; }
}

public sealed class CertificacaoDto
{
    [Required]
    [StringLength(150)]
    public string Nome { get; init; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Emissor { get; init; } = string.Empty;

    public DateTime? ValidaAte { get; init; }
}

public sealed class IndicadoresSociaisDto
{
    [Range(0, int.MaxValue)]
    public int AcidentesUltimos12Meses { get; init; }

    [Range(typeof(decimal), "0", "100")]
    public decimal? PercentualMulheresLideranca { get; init; }

    public bool? PossuiProgramaDiversidade { get; init; }
}

public sealed class ConformidadeAmbientalDto
{
    public bool PossuiLicenca { get; init; }

    [Range(0, int.MaxValue)]
    public int OcorrenciasUltimos12Meses { get; init; }

    public bool? DescarteMonitorado { get; init; }

    [Range(typeof(decimal), "0", "100")]
    public decimal? PercentualMaterialReciclado { get; init; }

    public DateTime? DataUltimaAuditoria { get; init; }
}
