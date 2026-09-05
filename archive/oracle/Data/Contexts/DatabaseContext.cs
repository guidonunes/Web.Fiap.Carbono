using Microsoft.EntityFrameworkCore;
using Web.Fiap.Carbono.Models;

namespace Web.Fiap.Carbono.Data.Contexts;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    public DbSet<EmpresaModel> Empresas { get; set; }
    public DbSet<ProdutoModel> Produtos { get; set; }
    public DbSet<FornecedorModel> Fornecedores { get; set; }
    public DbSet<LoteProducaoModel> LotesProducao { get; set; }
    public DbSet<EtapaCadeiaModel> EtapasCadeia { get; set; }
    public DbSet<EmissaoCarbonoModel> EmissoesCarbono { get; set; }
    public DbSet<FatorEmissaoModel> FatoresEmissao { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EmpresaModel>(entity =>
        {
            entity.ToTable("EC_EMPRESAS");

            entity.HasKey(e => e.IdEmpresa)
                .HasName("EC_EMPRESAS_PK");

            entity.Property(e => e.IdEmpresa)
                .HasColumnName("ID_EMPRESA")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.NomeEmpresa)
                .HasColumnName("NM_EMPRESA")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Cnpj)
                .HasColumnName("DS_CNPJ")
                .HasMaxLength(18)
                .IsRequired();

            entity.Property(e => e.SetorAtuacao)
                .HasColumnName("DS_SETOR_ATUACAO")
                .HasMaxLength(100);

            entity.Property(e => e.Cidade)
                .HasColumnName("NM_CIDADE")
                .HasMaxLength(100);

            entity.Property(e => e.Estado)
                .HasColumnName("NM_ESTADO")
                .HasMaxLength(2);

            entity.Property(e => e.Pais)
                .HasColumnName("NM_PAIS")
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(e => e.DataCadastro)
                .HasColumnName("DT_CADASTRO")
                .HasColumnType("DATE")
                .IsRequired();

            entity.HasIndex(e => e.Cnpj)
                .IsUnique()
                .HasDatabaseName("UK_EC_EMPRESAS_CNPJ");
        });

        modelBuilder.Entity<ProdutoModel>(entity =>
        {
            entity.ToTable("EC_PRODUTOS", table =>
            {
                table.HasCheckConstraint("CK_EC_PRODUTOS_ATIVO", "\"ST_ATIVO\" IN (''S'', ''N'')");
            });

            entity.HasKey(e => e.IdProduto)
                .HasName("EC_PRODUTOS_PK");

            entity.Property(e => e.IdProduto)
                .HasColumnName("ID_PRODUTO")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.IdEmpresa)
                .HasColumnName("ID_EMPRESA")
                .IsRequired();

            entity.Property(e => e.NomeProduto)
                .HasColumnName("NM_PRODUTO")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Descricao)
                .HasColumnName("DS_DESCRICAO")
                .HasMaxLength(500);

            entity.Property(e => e.TipoCategoria)
                .HasColumnName("TP_CATEGORIA")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.UnidadeMedida)
                .HasColumnName("DS_UNIDADE_MEDIDA")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.Ativo)
                .HasColumnName("ST_ATIVO")
                .HasColumnType("CHAR(1)")
                .HasMaxLength(1)
                .IsRequired();

            entity.Property(e => e.DataCadastro)
                .HasColumnName("DT_CADASTRO")
                .HasColumnType("DATE")
                .IsRequired();

            entity.HasOne(e => e.Empresa)
                .WithMany(e => e.Produtos)
                .HasForeignKey(e => e.IdEmpresa)
                .HasConstraintName("FK_EC_PRODUTOS_EMPRESAS")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FornecedorModel>(entity =>
        {
            entity.ToTable("EC_FORNECEDORES", table =>
            {
                table.HasCheckConstraint("CK_EC_FORNECEDORES_ATIVO", "\"ST_ATIVO\" IN (''S'', ''N'')");
            });

            entity.HasKey(e => e.IdFornecedor)
                .HasName("EC_FORNECEDORES_PK");

            entity.Property(e => e.IdFornecedor)
                .HasColumnName("ID_FORNECEDOR")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.NomeFornecedor)
                .HasColumnName("NM_FORNECEDOR")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Cnpj)
                .HasColumnName("DS_CNPJ")
                .HasMaxLength(18)
                .IsRequired();

            entity.Property(e => e.TipoFornecedor)
                .HasColumnName("TP_FORNECEDOR")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Cidade)
                .HasColumnName("NM_CIDADE")
                .HasMaxLength(100);

            entity.Property(e => e.Estado)
                .HasColumnName("NM_ESTADO")
                .HasMaxLength(2);

            entity.Property(e => e.Pais)
                .HasColumnName("NM_PAIS")
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(e => e.CertificacaoEsg)
                .HasColumnName("DS_CERTIFICACAO_ESG")
                .HasMaxLength(100);

            entity.Property(e => e.Ativo)
                .HasColumnName("ST_ATIVO")
                .HasColumnType("CHAR(1)")
                .HasMaxLength(1)
                .IsRequired();

            entity.HasIndex(e => e.Cnpj)
                .IsUnique()
                .HasDatabaseName("UK_EC_FORNECEDORES_CNPJ");
        });

        modelBuilder.Entity<LoteProducaoModel>(entity =>
        {
            entity.ToTable("EC_LOTES_PRODUCAO", table =>
            {
                table.HasCheckConstraint("CK_EC_LOTES_QTDE", "\"QT_QUANTIDADE\" > 0");
                table.HasCheckConstraint("CK_EC_LOTES_DATAS", "\"DT_DATA_VALIDADE\" >= \"DT_DATA_PRODUCAO\"");
            });

            entity.HasKey(e => e.IdLote)
                .HasName("EC_LOTES_PRODUCAO_PK");

            entity.Property(e => e.IdLote)
                .HasColumnName("ID_LOTE")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.IdProduto)
                .HasColumnName("ID_PRODUTO")
                .IsRequired();

            entity.Property(e => e.CodigoLote)
                .HasColumnName("CD_CODIGO_LOTE")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Quantidade)
                .HasColumnName("QT_QUANTIDADE")
                .HasColumnType("NUMBER(10,2)")
                .IsRequired();

            entity.Property(e => e.DataProducao)
                .HasColumnName("DT_DATA_PRODUCAO")
                .HasColumnType("DATE")
                .IsRequired();

            entity.Property(e => e.DataValidade)
                .HasColumnName("DT_DATA_VALIDADE")
                .HasColumnType("DATE")
                .IsRequired();

            entity.Property(e => e.StatusLote)
                .HasColumnName("ST_STATUS_LOTE")
                .HasMaxLength(30)
                .IsRequired();

            entity.HasOne(e => e.Produto)
                .WithMany(e => e.LotesProducao)
                .HasForeignKey(e => e.IdProduto)
                .HasConstraintName("FK_EC_LOTES_PRODUTOS")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EtapaCadeiaModel>(entity =>
        {
            entity.ToTable("EC_ETAPAS_CADEIA", table =>
            {
                table.HasCheckConstraint("CK_EC_ETAPAS_DATAS", "\"DT_DATA_FIM\" IS NULL OR \"DT_DATA_FIM\" >= \"DT_DATA_INICIO\"");
            });

            entity.HasKey(e => e.IdEtapa)
                .HasName("EC_ETAPAS_CADEIA_PK");

            entity.Property(e => e.IdEtapa)
                .HasColumnName("ID_ETAPA")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.TipoEtapa)
                .HasColumnName("TP_TIPO_ETAPA")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Descricao)
                .HasColumnName("DS_DESCRICAO")
                .HasMaxLength(500);

            entity.Property(e => e.Origem)
                .HasColumnName("DS_ORIGEM")
                .HasMaxLength(150);

            entity.Property(e => e.Destino)
                .HasColumnName("DS_DESTINO")
                .HasMaxLength(150);

            entity.Property(e => e.DataInicio)
                .HasColumnName("DT_DATA_INICIO")
                .HasColumnType("DATE")
                .IsRequired();

            entity.Property(e => e.DataFim)
                .HasColumnName("DT_DATA_FIM")
                .HasColumnType("DATE");

            entity.Property(e => e.OrdemEtapa)
                .HasColumnName("DS_ORDEM_ETAPA")
                .HasMaxLength(30);

            entity.Property(e => e.IdFornecedor)
                .HasColumnName("ID_FORNECEDOR")
                .IsRequired();

            entity.Property(e => e.IdLote)
                .HasColumnName("ID_LOTE")
                .IsRequired();

            entity.HasOne(e => e.Fornecedor)
                .WithMany(e => e.EtapasCadeia)
                .HasForeignKey(e => e.IdFornecedor)
                .HasConstraintName("FK_EC_ETAPAS_FORNECEDORES")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.LoteProducao)
                .WithMany(e => e.EtapasCadeia)
                .HasForeignKey(e => e.IdLote)
                .HasConstraintName("FK_EC_ETAPAS_LOTES")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FatorEmissaoModel>(entity =>
        {
            entity.ToTable("EC_FATORES_EMISSAO", table =>
            {
                table.HasCheckConstraint("CK_EC_FATORES_ATIVO", "\"ST_ATIVO\" IN (''S'', ''N'')");
                table.HasCheckConstraint("CK_EC_FATORES_VALOR", "\"VL_FATOR_CO2E\" > 0");
                table.HasCheckConstraint("CK_EC_FATORES_ESCOPO", "\"TP_ESCOPO\" IN (''ESCOPO_1'', ''ESCOPO_2'', ''ESCOPO_3'')");
            });

            entity.HasKey(e => e.IdFator)
                .HasName("EC_FATORES_EMISSAO_PK");

            entity.Property(e => e.IdFator)
                .HasColumnName("ID_FATOR")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Fonte)
                .HasColumnName("DS_FONTE")
                .HasMaxLength(155)
                .IsRequired();

            entity.Property(e => e.Escopo)
                .HasColumnName("TP_ESCOPO")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.UnidadeBase)
                .HasColumnName("DS_UNIDADE_BASE")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.ValorFatorCo2e)
                .HasColumnName("VL_FATOR_CO2E")
                .HasColumnType("NUMBER(12,6)")
                .IsRequired();

            entity.Property(e => e.Referencia)
                .HasColumnName("DS_REFERENCIA")
                .HasMaxLength(200);

            entity.Property(e => e.Ativo)
                .HasColumnName("ST_ATIVO")
                .HasColumnType("CHAR(1)")
                .HasMaxLength(1)
                .IsRequired();

            entity.Property(e => e.DataCadastro)
                .HasColumnName("DT_CADASTRO")
                .HasColumnType("DATE")
                .IsRequired();
        });

        modelBuilder.Entity<EmissaoCarbonoModel>(entity =>
        {
            entity.ToTable("EC_EMISSOES_CARBONO", table =>
            {
                table.HasCheckConstraint("CK_EC_EMISSOES_QTD_EMITIDA", "\"QTD_EMITIDA\" >= 0");
                table.HasCheckConstraint("CK_EC_EMISSOES_QT_ATIVIDADE", "\"QT_ATIVIDADE\" > 0");
            });

            entity.HasKey(e => e.IdEmissao)
                .HasName("EC_EMISSOES_CARBONO_PK");

            entity.Property(e => e.IdEmissao)
                .HasColumnName("ID_EMISSAO")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.IdFator)
                .HasColumnName("ID_FATOR")
                .IsRequired();

            entity.Property(e => e.IdEtapa)
                .HasColumnName("ID_ETAPA")
                .IsRequired();

            entity.Property(e => e.FonteEmissao)
                .HasColumnName("DS_FONTE_EMISSAO")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.QuantidadeEmitida)
                .HasColumnName("QTD_EMITIDA")
                .HasColumnType("NUMBER(12,3)")
                .IsRequired();

            entity.Property(e => e.QuantidadeAtividade)
                .HasColumnName("QT_ATIVIDADE")
                .HasColumnType("NUMBER(12,3)")
                .IsRequired();

            entity.Property(e => e.Unidade)
                .HasColumnName("TP_UNIDADE")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.MetodoCalculo)
                .HasColumnName("DS_METODO_CALCULO")
                .HasMaxLength(100);

            entity.Property(e => e.Observacao)
                .HasColumnName("DS_OBSERVACAO")
                .HasMaxLength(500);

            entity.Property(e => e.DataRegistro)
                .HasColumnName("DT_DATA_REGISTRO")
                .HasColumnType("DATE")
                .IsRequired();

            entity.HasOne(e => e.EtapaCadeia)
                .WithMany(e => e.EmissoesCarbono)
                .HasForeignKey(e => e.IdEtapa)
                .HasConstraintName("FK_EC_EMISSOES_ETAPAS")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.FatorEmissao)
                .WithMany(e => e.EmissoesCarbono)
                .HasForeignKey(e => e.IdFator)
                .HasConstraintName("FK_EC_EMISSOES_FATORES")
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
