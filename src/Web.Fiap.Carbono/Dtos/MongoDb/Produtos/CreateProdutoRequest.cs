using System.ComponentModel.DataAnnotations;
using Web.Fiap.Carbono.Dtos.MongoDb.Common;

namespace Web.Fiap.Carbono.Dtos.MongoDb.Produtos;

public sealed class CreateProdutoRequest
{
    [Required]
    public string EmpresaId { get; init; } = string.Empty;

    [Required]
    [StringLength(50)]
    [RegularExpression("^[A-Za-z0-9-]+$")]
    public string Codigo { get; init; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Nome { get; init; } = string.Empty;

    [StringLength(100)]
    public string? Categoria { get; init; }

    [Required]
    [StringLength(100)]
    public string UnidadeFuncional { get; init; } = string.Empty;

    public bool Ativo { get; init; } = true;

    public AtributosAmbientaisDto? AtributosAmbientais {
        get;
        init;
    }
}
