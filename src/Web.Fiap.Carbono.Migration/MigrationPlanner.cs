using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MongoDB.Bson;

namespace Web.Fiap.Carbono.Migration;

public static class MigrationPlanner
{
    public static readonly string[] Collections =
        ["empresas", "produtos", "fornecedores", "fatores_emissao", "emissoes_carbono"];

    public static Dictionary<string, List<BsonDocument>> Build(
        OracleSource source, MigrationPolicy policy)
    {
        Check(!string.IsNullOrWhiteSpace(policy.SourceId), "SourceId is required.");
        Check(policy.MigrationTimestampUtc != default &&
              policy.MigrationTimestampUtc.Kind == DateTimeKind.Utc,
            "MigrationTimestampUtc must be explicitly selected in UTC.");
        Check(!string.IsNullOrWhiteSpace(policy.OracleTimeZone),
            "OracleTimeZone must be explicitly selected.");
        TimeZoneInfo timezone;
        try { timezone = TimeZoneInfo.FindSystemTimeZoneById(policy.OracleTimeZone); }
        catch (Exception e) when (e is TimeZoneNotFoundException or InvalidTimeZoneException)
        { throw new MigrationValidationException(["Invalid OracleTimeZone."]); }

        var errors = new List<string>();
        var plan = Collections.ToDictionary(x => x, _ => new List<BsonDocument>());
        var companies = Index(source.Empresas, x => x.IdEmpresa, "EC_EMPRESAS");
        var products = Index(source.Produtos, x => x.IdProduto, "EC_PRODUTOS");
        var suppliers = Index(source.Fornecedores, x => x.IdFornecedor, "EC_FORNECEDORES");
        var factors = Index(source.Fatores, x => x.IdFator, "EC_FATORES_EMISSAO");
        var batches = Index(source.Lotes, x => x.IdLote, "EC_LOTES_PRODUCAO");
        var stages = Index(source.Etapas, x => x.IdEtapa, "EC_ETAPAS_CADEIA");
        _ = Index(source.Emissoes, x => x.IdEmissao, "EC_EMISSOES_CARBONO");

        ObjectId Id(string collection, int id) => StableId(policy.SourceId, collection, id);
        DateTime Utc(DateTime date)
        {
            Check(date != default, "Required Oracle date is missing.");
            var local = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
            Check(!timezone.IsInvalidTime(local) && !timezone.IsAmbiguousTime(local),
                "Oracle date requires an explicit daylight-saving resolution.");
            return TimeZoneInfo.ConvertTimeToUtc(local, timezone);
        }

        BsonDocument Base(string collection, int id, DateTime created, object original) => new()
        {
            ["_id"] = Id(collection, id), ["legacyId"] = id,
            ["codigo"] = $"ORACLE-{collection}-{id}",
            ["criadoEm"] = new BsonDateTime(created),
            ["atualizadoEm"] = new BsonDateTime(created), ["schemaVersion"] = 1,
            ["migracao"] = new BsonDocument
            {
                ["origem"] = policy.SourceId,
                ["executadaEm"] = new BsonDateTime(policy.MigrationTimestampUtc),
                ["fusoHorarioOracle"] = policy.OracleTimeZone,
                ["atualizadoEmDerivadoDaCriacao"] = true,
                // Keep source fields absent from public DTOs without losing their text/scale.
                ["registroOracleJson"] = JsonSerializer.Serialize(original)
            }
        };

        void Plan(string collection, int id, Func<BsonDocument> map)
        {
            try { plan[collection].Add(map()); }
            catch (MigrationValidationException e)
            { errors.AddRange(e.Errors.Select(x => $"{collection}, legacyId={id}: {x}")); }
            catch (OverflowException)
            { errors.Add($"{collection}, legacyId={id}: decimal overflow."); }
        }

        // Validate even unused batches/stages, so joining cannot silently hide orphans.
        foreach (var batch in source.Lotes)
        {
            if (!products.ContainsKey(batch.IdProduto))
                errors.Add($"EC_LOTES_PRODUCAO, legacyId={batch.IdLote}: missing product {batch.IdProduto}.");
            if (batch.Quantidade <= 0 || batch.DataProducao == default || batch.DataValidade < batch.DataProducao)
                errors.Add($"EC_LOTES_PRODUCAO, legacyId={batch.IdLote}: invalid quantity or dates.");
        }
        foreach (var stage in source.Etapas)
        {
            if (!batches.ContainsKey(stage.IdLote))
                errors.Add($"EC_ETAPAS_CADEIA, legacyId={stage.IdEtapa}: missing batch {stage.IdLote}.");
            if (!suppliers.ContainsKey(stage.IdFornecedor))
                errors.Add($"EC_ETAPAS_CADEIA, legacyId={stage.IdEtapa}: missing supplier {stage.IdFornecedor}.");
            if (stage.DataInicio == default || stage.DataFim < stage.DataInicio)
                errors.Add($"EC_ETAPAS_CADEIA, legacyId={stage.IdEtapa}: invalid dates.");
        }

        foreach (var x in source.Empresas)
            Plan("empresas", x.IdEmpresa, () =>
            {
                Check(policy.CompanyActiveDefault.HasValue, "CompanyActiveDefault must be reviewed.");
                var d = Base("empresas", x.IdEmpresa, Utc(x.DataCadastro), x);
                d["razaoSocial"] = Text(x.NomeEmpresa); d["cnpj"] = Cnpj(x.Cnpj);
                d["ativa"] = policy.CompanyActiveDefault!.Value;
                Optional(d, "setor", x.SetorAtuacao);
                d["migracao"]["ativaDefinidaPorPolitica"] = true;
                return d;
            });

        foreach (var x in source.Produtos)
            Plan("produtos", x.IdProduto, () =>
            {
                Check(companies.ContainsKey(x.IdEmpresa), $"Missing company {x.IdEmpresa}.");
                var d = Base("produtos", x.IdProduto, Utc(x.DataCadastro), x);
                d["empresaId"] = Id("empresas", x.IdEmpresa); d["nome"] = Text(x.NomeProduto);
                d["categoria"] = Text(x.TipoCategoria); d["unidadeFuncional"] = Text(x.UnidadeMedida);
                d["ativo"] = Active(x.Ativo);
                return d;
            });

        foreach (var x in source.Fornecedores)
            Plan("fornecedores", x.IdFornecedor, () =>
            {
                Check(policy.SupplierCreatedAtUtc is { Kind: DateTimeKind.Utc } created && created != default,
                    "SupplierCreatedAtUtc must be explicitly selected in UTC.");
                var d = Base("fornecedores", x.IdFornecedor, policy.SupplierCreatedAtUtc!.Value, x);
                d["razaoSocial"] = Text(x.NomeFornecedor); d["cnpj"] = Cnpj(x.Cnpj);
                d["ativo"] = Active(x.Ativo);
                d["categoriasAtuacao"] = new BsonArray { Text(x.TipoFornecedor) };
                if (!string.IsNullOrWhiteSpace(x.CertificacaoEsg))
                    d["certificacoes"] = new BsonArray { new BsonDocument("nome", x.CertificacaoEsg.Trim()) };
                d["migracao"]["criadoEmDefinidoPorPolitica"] = true;
                return d;
            });

        foreach (var x in source.Fatores)
            Plan("fatores_emissao", x.IdFator, () =>
            {
                Check(policy.Factors.TryGetValue(x.IdFator, out var p), "Missing factor policy.");
                Check(Category(p!.Category) && p.Version > 0, "Invalid factor category/version.");
                Check(p.ValidFromUtc != default && p.ValidFromUtc.Kind == DateTimeKind.Utc,
                    "Factor validity start must be reviewed in UTC.");
                Check(p.ValidUntilUtc is null || (p.ValidUntilUtc.Value.Kind == DateTimeKind.Utc &&
                      p.ValidUntilUtc >= p.ValidFromUtc), "Invalid factor validity end.");
                Check(!string.IsNullOrWhiteSpace(p.DecisionNote), "Factor decision note is required.");
                Check(x.ValorFatorCo2e >= 0, "Negative factor value.");
                Check(x.Escopo is "ESCOPO_1" or "ESCOPO_2" or "ESCOPO_3", "Invalid scope.");
                var d = Base("fatores_emissao", x.IdFator, Utc(x.DataCadastro), x);
                d["nome"] = Text(x.Fonte); d["categoria"] = p.Category;
                d["valor"] = Decimal(x.ValorFatorCo2e); d["unidadeBase"] = Text(x.UnidadeBase);
                d["escopo"] = x.Escopo; d["versao"] = p.Version; d["ativo"] = Active(x.Ativo);
                d["validoDe"] = new BsonDateTime(p.ValidFromUtc);
                if (p.ValidUntilUtc.HasValue) d["validoAte"] = new BsonDateTime(p.ValidUntilUtc.Value);
                Optional(d, "fonteReferencia", x.Referencia);
                d["migracao"]["decisaoFator"] = p.DecisionNote;
                return d;
            });

        var mappedFactors = plan["fatores_emissao"].ToDictionary(x => x["legacyId"].AsInt32);
        foreach (var x in source.Emissoes)
            Plan("emissoes_carbono", x.IdEmissao, () =>
            {
                Check(stages.TryGetValue(x.IdEtapa, out var stage), $"Missing stage {x.IdEtapa}.");
                Check(batches.TryGetValue(stage!.IdLote, out var batch), $"Missing batch {stage.IdLote}.");
                Check(products.TryGetValue(batch!.IdProduto, out var product), $"Missing product {batch.IdProduto}.");
                Check(companies.ContainsKey(product!.IdEmpresa), $"Missing company {product.IdEmpresa}.");
                Check(suppliers.ContainsKey(stage.IdFornecedor), $"Missing supplier {stage.IdFornecedor}.");
                Check(factors.TryGetValue(x.IdFator, out var factor), $"Missing factor {x.IdFator}.");
                Check(mappedFactors.TryGetValue(x.IdFator, out var mapped), $"Factor {x.IdFator} failed mapping.");
                Check(x.QuantidadeAtividade > 0 && x.QuantidadeEmitida >= 0, "Invalid activity/result quantity.");
                Check(x.Unidade == "kgCO2e", "Unsupported result unit.");
                Check(int.TryParse(stage.OrdemEtapa, NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out var order) && order > 0, "Stage order must be a positive integer.");
                // Source EF mapping is NUMBER(12,3). Never replace the persisted result.
                var expected = decimal.Round(checked(x.QuantidadeAtividade * factor!.ValorFatorCo2e),
                    3, MidpointRounding.AwayFromZero);
                Check(expected == x.QuantidadeEmitida,
                    "Result differs from current factor at Oracle scale; historical review required.");
                var category = policy.StageCategories.TryGetValue(stage.TipoEtapa, out var replacement)
                    ? replacement : stage.TipoEtapa.Trim().ToUpperInvariant();
                Check(Category(category), "Stage category requires a reviewed policy mapping.");
                var d = Base("emissoes_carbono", x.IdEmissao, Utc(x.DataRegistro), x);
                d["empresaId"] = Id("empresas", product.IdEmpresa); d["produtoId"] = Id("produtos", product.IdProduto);
                d["fornecedorId"] = Id("fornecedores", stage.IdFornecedor); d["fatorEmissaoId"] = Id("fatores_emissao", x.IdFator);
                d["lote"] = new BsonDocument
                {
                    ["legacyId"] = batch.IdLote, ["codigo"] = Text(batch.CodigoLote),
                    ["quantidadeProduzida"] = Decimal(batch.Quantidade), ["unidade"] = Text(product.UnidadeMedida),
                    ["dataProducao"] = new BsonDateTime(Utc(batch.DataProducao))
                };
                d["etapa"] = new BsonDocument
                {
                    ["legacyId"] = stage.IdEtapa, ["nome"] = string.IsNullOrWhiteSpace(stage.Descricao)
                        ? Text(stage.TipoEtapa) : stage.Descricao.Trim(),
                    ["ordem"] = order, ["categoria"] = category
                };
                d["dadosAtividade"] = new BsonDocument
                {
                    ["tipo"] = category, ["origem"] = "ORACLE", ["detalhamentoDisponivel"] = false,
                    ["quantidadeOriginal"] = Decimal(x.QuantidadeAtividade),
                    ["unidadeBaseFator"] = Text(factor.UnidadeBase)
                };
                var snapshot = new BsonDocument();
                foreach (var field in new[] { "codigo", "nome", "valor", "unidadeBase", "escopo", "versao", "fonteReferencia" })
                    if (mapped!.TryGetValue(field, out var value)) snapshot[field] = value.DeepClone();
                d["fatorAplicado"] = snapshot; d["quantidadeAtividade"] = Decimal(x.QuantidadeAtividade);
                d["quantidadeEmitidaKgCO2e"] = Decimal(x.QuantidadeEmitida);
                d["metodoCalculo"] = Text(x.MetodoCalculo); d["calculadoPor"] = "NAO_REGISTRADO_NO_ORACLE";
                d["dataEmissao"] = new BsonDateTime(Utc(x.DataRegistro));
                Optional(d, "fonteEmissao", x.FonteEmissao); Optional(d, "observacao", x.Observacao);
                d["migracao"]["snapshotReconstruido"] = true;
                d["migracao"]["loteOracleJson"] = JsonSerializer.Serialize(batch);
                d["migracao"]["etapaOracleJson"] = JsonSerializer.Serialize(stage);
                d["migracao"]["unidadeLoteDerivadaDoProduto"] = true;
                return d;
            });

        foreach (var name in new[] { "empresas", "fornecedores" })
            foreach (var group in plan[name].GroupBy(x => x["cnpj"]).Where(x => x.Count() > 1))
                errors.Add($"{name}: duplicate normalized CNPJ in legacy IDs {string.Join(", ", group.Select(x => x["legacyId"]))}.");
        var ids = plan.Values.SelectMany(x => x).Select(x => x["_id"]).ToArray();
        if (ids.Distinct().Count() != ids.Length) errors.Add("Generated ObjectId collision.");
        if (errors.Count > 0) throw new MigrationValidationException(errors);
        return plan;
    }

    public static ObjectId StableId(string source, string collection, int legacyId)
    {
        var identity = JsonSerializer.Serialize(new[] { source, collection, legacyId.ToString(CultureInfo.InvariantCulture) });
        return new ObjectId(SHA256.HashData(Encoding.UTF8.GetBytes(identity))[..12]);
    }

    private static Dictionary<int, T> Index<T>(IEnumerable<T> records, Func<T, int> key, string table)
    {
        var groups = records.GroupBy(key).ToArray();
        var invalid = groups.Where(x => x.Key <= 0 || x.Count() != 1).Select(x => $"{table}: invalid/duplicate legacyId={x.Key}.").ToArray();
        if (invalid.Length > 0) throw new MigrationValidationException(invalid);
        return groups.ToDictionary(x => x.Key, x => x.Single());
    }
    internal static void Check(bool condition, string message)
    { if (!condition) throw new MigrationValidationException([message]); }
    private static string Text(string? text)
    { Check(!string.IsNullOrWhiteSpace(text), "Required source text is missing."); return text!.Trim(); }
    private static bool Active(string text) => text?.Trim() switch
    {
        "S" => true, "N" => false,
        _ => throw new MigrationValidationException(["Invalid Oracle active flag."])
    };
    private static string Cnpj(string? text)
    {
        Check(text is not null && text.All(x => char.IsAsciiDigit(x) || x is '.' or '/' or '-' or ' '),
            "Invalid CNPJ characters.");
        var value = new string(text!.Where(char.IsAsciiDigit).ToArray());
        Check(value.Length == 14, "CNPJ must contain 14 digits."); return value;
    }
    private static bool Category(string value) => value is "TRANSPORTE" or "ENERGIA" or "MATERIA_PRIMA" or "RESIDUO";
    private static BsonDecimal128 Decimal(decimal value) => new(new Decimal128(value));
    private static void Optional(BsonDocument document, string name, string? value)
    { if (!string.IsNullOrWhiteSpace(value)) document[name] = value.Trim(); }
}
