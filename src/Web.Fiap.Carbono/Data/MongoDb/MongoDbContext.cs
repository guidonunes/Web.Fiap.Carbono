using MongoDB.Driver;
using Web.Fiap.Carbono.Models.Documents;

namespace Web.Fiap.Carbono.Data.MongoDb;

public sealed class MongoDbContext
{
    public const string EmpresasCollectionName = "empresas";
    public const string ProdutosCollectionName = "produtos";
    public const string FornecedoresCollectionName = "fornecedores";
    public const string FatoresEmissaoCollectionName = "fatores_emissao";
    public const string EmissoesCarbonoCollectionName = "emissoes_carbono";

    public MongoDbContext(IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);

        Empresas = database.GetCollection<EmpresaDocument>(
            EmpresasCollectionName
        );
        Produtos = database.GetCollection<ProdutoDocument>(
            ProdutosCollectionName
        );
        Fornecedores = database.GetCollection<FornecedorDocument>(
            FornecedoresCollectionName
        );
        FatoresEmissao = database.GetCollection<FatorEmissaoDocument>(
            FatoresEmissaoCollectionName
        );
        EmissoesCarbono = database.GetCollection<EmissaoCarbonoDocument>(
            EmissoesCarbonoCollectionName
        );
    }

    public IMongoCollection<EmpresaDocument> Empresas { get; }
    public IMongoCollection<ProdutoDocument> Produtos { get; }
    public IMongoCollection<FornecedorDocument> Fornecedores { get; }
    public IMongoCollection<FatorEmissaoDocument> FatoresEmissao { get; }
    public IMongoCollection<EmissaoCarbonoDocument> EmissoesCarbono { get; }
}
