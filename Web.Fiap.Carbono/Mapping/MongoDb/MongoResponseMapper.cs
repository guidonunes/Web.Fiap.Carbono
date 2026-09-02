using Web.Fiap.Carbono.Dtos.MongoDb.Common;
using Web.Fiap.Carbono.Dtos.MongoDb.Empresas;
using Web.Fiap.Carbono.Dtos.MongoDb.FatoresEmissao;
using Web.Fiap.Carbono.Dtos.MongoDb.Fornecedores;
using Web.Fiap.Carbono.Dtos.MongoDb.Produtos;
using Web.Fiap.Carbono.Models.Documents;

namespace Web.Fiap.Carbono.Mapping.MongoDb;

public static class MongoResponseMapper
{
    public static EmpresaResponse ToResponse(
        this EmpresaDocument document)
    {
        return new EmpresaResponse
        {
            Id = document.Id.ToString(),
            Codigo = document.Codigo,
            RazaoSocial = document.RazaoSocial,
            NomeFantasia = document.NomeFantasia,
            Cnpj = document.Cnpj,
            Setor = document.Setor,
            Ativa = document.Ativa,

            MetasReducao = document.MetasReducao
                .Select(meta => new MetaReducaoDto
                {
                    Tipo = meta.Tipo,
                    AnoBase = meta.AnoBase,
                    AnoMeta = meta.AnoMeta,
                    PercentualReducao = meta.PercentualReducao
                })
                .ToList(),

            Governanca = document.Governanca is null
                ? null
                : new GovernancaEsgDto
                {
                    ResponsavelEsg =
                        document.Governanca.ResponsavelEsg,

                    ComiteEsg =
                        document.Governanca.ComiteEsg,

                    FrequenciaAuditoria =
                        document.Governanca.FrequenciaAuditoria,
                    RelatorioPublico =
                        document.Governanca.RelatorioPublico,
                    CanalDenuncias =
                        document.Governanca.CanalDenuncias,
                    ConselhoSupervisao =
                        document.Governanca.ConselhoSupervisao
                },

            LegacyId = document.LegacyId,
            CriadoEm = document.CriadoEm,
            AtualizadoEm = document.AtualizadoEm,
            SchemaVersion = document.SchemaVersion
        };
    }

    public static ProdutoResponse ToResponse(
        this ProdutoDocument document)
    {
        return new ProdutoResponse
        {
            Id = document.Id.ToString(),
            EmpresaId = document.EmpresaId.ToString(),
            Codigo = document.Codigo,
            Nome = document.Nome,
            Categoria = document.Categoria,
            UnidadeFuncional = document.UnidadeFuncional,
            Ativo = document.Ativo,

            AtributosAmbientais =
                document.AtributosAmbientais is null
                    ? null
                    : new AtributosAmbientaisDto
                    {
                        PercentualReciclavel =
                            document.AtributosAmbientais
                                .PercentualReciclavel,

                        PercentualMaterialReciclado =
                            document.AtributosAmbientais
                                .PercentualMaterialReciclado,

                        Embalagem =
                            document.AtributosAmbientais.Embalagem,

                        SeloSustentabilidade =
                            document.AtributosAmbientais
                                .SeloSustentabilidade,

                        CompensacaoCarbono =
                            document.AtributosAmbientais
                                .CompensacaoCarbono,

                        OrigemAgriculturaOrganica =
                            document.AtributosAmbientais
                                .OrigemAgriculturaOrganica,

                        VidaUtilAnos =
                            document.AtributosAmbientais
                                .VidaUtilAnos,

                        Materiais = document.AtributosAmbientais
                            .Materiais
                            .Select(material =>
                                new MaterialProdutoDto
                                {
                                    Nome = material.Nome,
                                    PercentualComposicao =
                                        material.PercentualComposicao,
                                    OrigemRenovavel =
                                        material.OrigemRenovavel,
                                    OrigemReciclada =
                                        material.OrigemReciclada
                                })
                            .ToList()
                    },

            LegacyId = document.LegacyId,
            CriadoEm = document.CriadoEm,
            AtualizadoEm = document.AtualizadoEm,
            SchemaVersion = document.SchemaVersion
        };
    }

    public static FornecedorResponse ToResponse(
        this FornecedorDocument document)
    {
        return new FornecedorResponse
        {
            Id = document.Id.ToString(),
            Codigo = document.Codigo,
            RazaoSocial = document.RazaoSocial,
            NomeFantasia = document.NomeFantasia,
            Cnpj = document.Cnpj,
            Ativo = document.Ativo,

            CategoriasAtuacao =
                document.CategoriasAtuacao.ToList(),

            Certificacoes = document.Certificacoes
                .Select(certificacao => new CertificacaoDto
                {
                    Nome = certificacao.Nome,
                    Emissor = certificacao.Emissor,
                    ValidaAte = certificacao.ValidaAte
                })
                .ToList(),

            IndicadoresSociais =
                document.IndicadoresSociais is null
                    ? null
                    : new IndicadoresSociaisDto
                    {
                        AcidentesUltimos12Meses =
                            document.IndicadoresSociais
                                .AcidentesUltimos12Meses,

                        PercentualMulheresLideranca =
                            document.IndicadoresSociais
                                .PercentualMulheresLideranca,

                        PossuiProgramaDiversidade =
                            document.IndicadoresSociais
                                .PossuiProgramaDiversidade
                    },

            ConformidadeAmbiental =
                document.ConformidadeAmbiental is null
                    ? null
                    : new ConformidadeAmbientalDto
                    {
                        PossuiLicenca =
                            document.ConformidadeAmbiental
                                .PossuiLicenca,

                        OcorrenciasUltimos12Meses =
                            document.ConformidadeAmbiental
                                .OcorrenciasUltimos12Meses,

                        DescarteMonitorado =
                            document.ConformidadeAmbiental
                                .DescarteMonitorado,

                        PercentualMaterialReciclado =
                            document.ConformidadeAmbiental
                                .PercentualMaterialReciclado,

                        DataUltimaAuditoria =
                            document.ConformidadeAmbiental
                                .DataUltimaAuditoria
                    },

            StatusAuditoria = document.StatusAuditoria,
            NivelRiscoEsg = document.NivelRiscoEsg,

            LegacyId = document.LegacyId,
            CriadoEm = document.CriadoEm,
            AtualizadoEm = document.AtualizadoEm,
            SchemaVersion = document.SchemaVersion
        };
    }

    public static FatorEmissaoResponse ToResponse(
        this FatorEmissaoDocument document)
    {
        return new FatorEmissaoResponse
        {
            Id = document.Id.ToString(),
            Codigo = document.Codigo,
            Nome = document.Nome,
            Categoria = document.Categoria,
            Valor = document.Valor,
            UnidadeBase = document.UnidadeBase,
            Escopo = document.Escopo,
            Versao = document.Versao,
            FonteReferencia = document.FonteReferencia,
            Metodologia = document.Metodologia,
            ValidoDe = document.ValidoDe,
            ValidoAte = document.ValidoAte,
            Ativo = document.Ativo,
            LegacyId = document.LegacyId,
            CriadoEm = document.CriadoEm,
            AtualizadoEm = document.AtualizadoEm,
            SchemaVersion = document.SchemaVersion
        };
    }
}
