using MongoDB.Bson;
using MongoDB.Driver;

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

        Empresas = database.GetCollection<BsonDocument>(
            EmpresasCollectionName
        );
        Produtos = database.GetCollection<BsonDocument>(
            ProdutosCollectionName
        );
        Fornecedores = database.GetCollection<BsonDocument>(
            FornecedoresCollectionName
        );
        FatoresEmissao = database.GetCollection<BsonDocument>(
            FatoresEmissaoCollectionName
        );
        EmissoesCarbono = database.GetCollection<BsonDocument>(
            EmissoesCarbonoCollectionName
        );
    }

    public IMongoCollection<BsonDocument> Empresas { get; }
    public IMongoCollection<BsonDocument> Produtos { get; }
    public IMongoCollection<BsonDocument> Fornecedores { get; }
    public IMongoCollection<BsonDocument> FatoresEmissao { get; }
    public IMongoCollection<BsonDocument> EmissoesCarbono { get; }
}
