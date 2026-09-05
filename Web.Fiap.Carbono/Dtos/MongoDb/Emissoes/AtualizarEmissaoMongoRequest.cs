using System.ComponentModel.DataAnnotations;

namespace Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;

public sealed class AtualizarEmissaoMongoRequest
{
    [Required]
    [StringLength(1000)]
    public string Observacao { get; init; } = string.Empty;
}
