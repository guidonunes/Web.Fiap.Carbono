using AutoMapper;
using Web.Fiap.Carbono.Models;
using Web.Fiap.Carbono.ViewModel;

namespace Web.Fiap.Carbono.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<EmissaoCarbonoModel, EmissaoCarbonoViewModel>()
            .ForMember(dest => dest.TipoEtapa,
                opt => opt.MapFrom(src => src.EtapaCadeia != null ? src.EtapaCadeia.TipoEtapa : string.Empty))
            .ForMember(dest => dest.FonteFator,
                opt => opt.MapFrom(src => src.FatorEmissao != null ? src.FatorEmissao.Fonte : string.Empty))
            .ForMember(dest => dest.Escopo,
                opt => opt.MapFrom(src => src.FatorEmissao != null ? src.FatorEmissao.Escopo : string.Empty))
            .ForMember(dest => dest.UnidadeBase,
                opt => opt.MapFrom(src => src.FatorEmissao != null ? src.FatorEmissao.UnidadeBase : string.Empty))
            .ForMember(dest => dest.ValorFatorCo2e,
                opt => opt.MapFrom(src => src.FatorEmissao != null ? src.FatorEmissao.ValorFatorCo2e : 0));

        CreateMap<ProdutoModel, ProdutoPegadaCarbonoViewModel>()
            .ForMember(dest => dest.NomeEmpresa,
                opt => opt.MapFrom(src => src.Empresa != null ? src.Empresa.NomeEmpresa : string.Empty))
            .ForMember(dest => dest.TotalCo2e,
                opt => opt.MapFrom(src =>
                    src.LotesProducao
                        .SelectMany(l => l.EtapasCadeia)
                        .SelectMany(e => e.EmissoesCarbono)
                        .Sum(ec => ec.QuantidadeEmitida)))
            .ForMember(dest => dest.EmissoesPorEtapa,
                opt => opt.MapFrom(src =>
                    src.LotesProducao
                        .SelectMany(l => l.EtapasCadeia)
                        .GroupBy(e => new { e.IdEtapa, e.TipoEtapa })
                        .Select(g => new EmissaoPorEtapaViewModel
                        {
                            IdEtapa = g.Key.IdEtapa,
                            TipoEtapa = g.Key.TipoEtapa,
                            TotalCo2e = g.SelectMany(e => e.EmissoesCarbono)
                                .Sum(ec => ec.QuantidadeEmitida)
                        })));

        CreateMap<FornecedorModel, FornecedorRankingCarbonoViewModel>()
            .ForMember(dest => dest.TotalCo2e,
                opt => opt.MapFrom(src =>
                    src.EtapasCadeia
                        .SelectMany(e => e.EmissoesCarbono)
                        .Sum(ec => ec.QuantidadeEmitida)))
            .ForMember(dest => dest.QuantidadeEtapas,
                opt => opt.MapFrom(src => src.EtapasCadeia.Count))
            .ForMember(dest => dest.QuantidadeEmissoes,
                opt => opt.MapFrom(src =>
                    src.EtapasCadeia
                        .SelectMany(e => e.EmissoesCarbono)
                        .Count()));

        CreateMap<EmpresaModel, DashboardCarbonoViewModel>()
            .ForMember(dest => dest.NomeEmpresa,
                opt => opt.MapFrom(src => src.NomeEmpresa))
            .ForMember(dest => dest.TotalCo2e,
                opt => opt.MapFrom(src =>
                    src.Produtos
                        .SelectMany(p => p.LotesProducao)
                        .SelectMany(l => l.EtapasCadeia)
                        .SelectMany(e => e.EmissoesCarbono)
                        .Sum(ec => ec.QuantidadeEmitida)))
            .ForMember(dest => dest.QuantidadeProdutos,
                opt => opt.MapFrom(src => src.Produtos.Count))
            .ForMember(dest => dest.QuantidadeLotes,
                opt => opt.MapFrom(src =>
                    src.Produtos
                        .SelectMany(p => p.LotesProducao)
                        .Count()))
            .ForMember(dest => dest.QuantidadeEtapas,
                opt => opt.MapFrom(src =>
                    src.Produtos
                        .SelectMany(p => p.LotesProducao)
                        .SelectMany(l => l.EtapasCadeia)
                        .Count()))
            .ForMember(dest => dest.QuantidadeEmissoes,
                opt => opt.MapFrom(src =>
                    src.Produtos
                        .SelectMany(p => p.LotesProducao)
                        .SelectMany(l => l.EtapasCadeia)
                        .SelectMany(e => e.EmissoesCarbono)
                        .Count()))
            .ForMember(dest => dest.MediaEmissaoPorProduto,
                opt => opt.MapFrom(src =>
                    src.Produtos.Any()
                        ? src.Produtos
                            .SelectMany(p => p.LotesProducao)
                            .SelectMany(l => l.EtapasCadeia)
                            .SelectMany(e => e.EmissoesCarbono)
                            .Sum(ec => ec.QuantidadeEmitida) / src.Produtos.Count
                        : 0))
            .ForMember(dest => dest.ProdutoMaisEmissor,
                opt => opt.MapFrom(src =>
                    src.Produtos
                        .Select(p => new
                        {
                            NomeProduto = p.NomeProduto,
                            Total = p.LotesProducao
                                .SelectMany(l => l.EtapasCadeia)
                                .SelectMany(e => e.EmissoesCarbono)
                                .Sum(ec => ec.QuantidadeEmitida)
                        })
                        .OrderByDescending(p => p.Total)
                        .Select(p => p.NomeProduto)
                        .FirstOrDefault()))
            .ForMember(dest => dest.FornecedorMaisEmissor,
                opt => opt.MapFrom(src =>
                    src.Produtos
                        .SelectMany(p => p.LotesProducao)
                        .SelectMany(l => l.EtapasCadeia)
                        .Where(e => e.Fornecedor != null)
                        .GroupBy(e => e.Fornecedor!.NomeFornecedor)
                        .Select(g => new
                        {
                            NomeFornecedor = g.Key,
                            Total = g.SelectMany(e => e.EmissoesCarbono)
                                .Sum(ec => ec.QuantidadeEmitida)
                        })
                        .OrderByDescending(f => f.Total)
                        .Select(f => f.NomeFornecedor)
                        .FirstOrDefault()))
            .ForMember(dest => dest.EmissoesPorMes,
                opt => opt.MapFrom(src =>
                    src.Produtos
                        .SelectMany(p => p.LotesProducao)
                        .SelectMany(l => l.EtapasCadeia)
                        .SelectMany(e => e.EmissoesCarbono)
                        .GroupBy(ec => ec.DataRegistro.ToString("yyyy-MM"))
                        .Select(g => new EmissaoPorMesViewModel
                        {
                            Mes = g.Key,
                            TotalCo2e = g.Sum(ec => ec.QuantidadeEmitida)
                        })
                        .OrderBy(e => e.Mes)));
    }
}