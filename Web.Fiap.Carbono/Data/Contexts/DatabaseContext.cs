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
                .HasColumnName("id_empresa")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.NomeEmpresa)
                .HasColumnName("nm_empresa")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Cnpj)
                .HasColumnName("ds_cnpj")
                .HasMaxLength(18)
                .IsRequired();

            entity.Property(e => e.SetorAtuacao)
                .HasColumnName("ds_setor_atuacao")
                .HasMaxLength(100);

            entity.Property(e => e.Cidade)
                .HasColumnName("nm_cidade")
                .HasMaxLength(100);

            entity.Property(e => e.Estado)
                .HasColumnName("nm_estado")
                .HasMaxLength(2);

            entity.Property(e => e.Pais)
                .HasColumnName("nm_pais")
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(e => e.DataCadastro)
                .HasColumnName("dt_cadastro")
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
                table.HasCheckConstraint("CK_EC_PRODUTOS_ATIVO", "\"st_ativo\" IN (''S'', ''N'')");
            });

            entity.HasKey(e => e.IdProduto)
                .HasName("EC_PRODUTOS_PK");

            entity.Property(e => e.IdProduto)
                .HasColumnName("id_produto")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.IdEmpresa)
                .HasColumnName("id_empresa")
                .IsRequired();

            entity.Property(e => e.NomeProduto)
                .HasColumnName("nm_produto")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Descricao)
                .HasColumnName("ds_descricao")
                .HasMaxLength(500);

            entity.Property(e => e.TipoCategoria)
                .HasColumnName("tp_categoria")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.UnidadeMedida)
                .HasColumnName("ds_unidade_medida")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.Ativo)
                .HasColumnName("st_ativo")
                .HasColumnType("CHAR(1)")
                .HasMaxLength(1)
                .IsRequired();

            entity.Property(e => e.DataCadastro)
                .HasColumnName("dt_cadastro")
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
                table.HasCheckConstraint("CK_EC_FORNECEDORES_ATIVO", "\"st_ativo\" IN (''S'', ''N'')");
            });

            entity.HasKey(e => e.IdFornecedor)
                .HasName("EC_FORNECEDORES_PK");

            entity.Property(e => e.IdFornecedor)
                .HasColumnName("id_fornecedor")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.NomeFornecedor)
                .HasColumnName("nm_fornecedor")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Cnpj)
                .HasColumnName("ds_cnpj")
                .HasMaxLength(18)
                .IsRequired();

            entity.Property(e => e.TipoFornecedor)
                .HasColumnName("tp_fornecedor")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Cidade)
                .HasColumnName("nm_cidade")
                .HasMaxLength(100);

            entity.Property(e => e.Estado)
                .HasColumnName("nm_estado")
                .HasMaxLength(2);

            entity.Property(e => e.Pais)
                .HasColumnName("nm_pais")
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(e => e.CertificacaoEsg)
                .HasColumnName("ds_certificacao_esg")
                .HasMaxLength(100);

            entity.Property(e => e.Ativo)
                .HasColumnName("st_ativo")
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
                table.HasCheckConstraint("CK_EC_LOTES_QTDE", "\"qt_quantidade\" > 0");
                table.HasCheckConstraint("CK_EC_LOTES_DATAS", "\"dt_data_validade\" >= \"dt_data_producao\"");
            });

            entity.HasKey(e => e.IdLote)
                .HasName("EC_LOTES_PRODUCAO_PK");

            entity.Property(e => e.IdLote)
                .HasColumnName("id_lote")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.IdProduto)
                .HasColumnName("id_produto")
                .IsRequired();

            entity.Property(e => e.CodigoLote)
                .HasColumnName("cd_codigo_lote")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Quantidade)
                .HasColumnName("qt_quantidade")
                .HasColumnType("NUMBER(10,2)")
                .IsRequired();

            entity.Property(e => e.DataProducao)
                .HasColumnName("dt_data_producao")
                .HasColumnType("DATE")
                .IsRequired();

            entity.Property(e => e.DataValidade)
                .HasColumnName("dt_data_validade")
                .HasColumnType("DATE")
                .IsRequired();

            entity.Property(e => e.StatusLote)
                .HasColumnName("st_status_lote")
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
                table.HasCheckConstraint("CK_EC_ETAPAS_DATAS", "\"dt_data_fim\" IS NULL OR \"dt_data_fim\" >= \"dt_data_inicio\"");
            });

            entity.HasKey(e => e.IdEtapa)
                .HasName("EC_ETAPAS_CADEIA_PK");

            entity.Property(e => e.IdEtapa)
                .HasColumnName("id_etapa")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.TipoEtapa)
                .HasColumnName("tp_tipo_etapa")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Descricao)
                .HasColumnName("ds_descricao")
                .HasMaxLength(500);

            entity.Property(e => e.Origem)
                .HasColumnName("ds_origem")
                .HasMaxLength(150);

            entity.Property(e => e.Destino)
                .HasColumnName("ds_destino")
                .HasMaxLength(150);

            entity.Property(e => e.DataInicio)
                .HasColumnName("dt_data_inicio")
                .HasColumnType("DATE")
                .IsRequired();

            entity.Property(e => e.DataFim)
                .HasColumnName("dt_data_fim")
                .HasColumnType("DATE");

            entity.Property(e => e.OrdemEtapa)
                .HasColumnName("ds_ordem_etapa")
                .HasMaxLength(30);

            entity.Property(e => e.IdFornecedor)
                .HasColumnName("id_fornecedor")
                .IsRequired();

            entity.Property(e => e.IdLote)
                .HasColumnName("id_lote")
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
                table.HasCheckConstraint("CK_EC_FATORES_ATIVO", "\"st_ativo\" IN (''S'', ''N'')");
                table.HasCheckConstraint("CK_EC_FATORES_VALOR", "\"vl_fator_co2e\" > 0");
                table.HasCheckConstraint("CK_EC_FATORES_ESCOPO", "\"tp_escopo\" IN (''ESCOPO_1'', ''ESCOPO_2'', ''ESCOPO_3'')");
            });

            entity.HasKey(e => e.IdFator)
                .HasName("EC_FATORES_EMISSAO_PK");

            entity.Property(e => e.IdFator)
                .HasColumnName("id_fator")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Fonte)
                .HasColumnName("ds_fonte")
                .HasMaxLength(155)
                .IsRequired();

            entity.Property(e => e.Escopo)
                .HasColumnName("tp_escopo")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.UnidadeBase)
                .HasColumnName("ds_unidade_base")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.ValorFatorCo2e)
                .HasColumnName("vl_fator_co2e")
                .HasColumnType("NUMBER(12,6)")
                .IsRequired();

            entity.Property(e => e.Referencia)
                .HasColumnName("ds_referencia")
                .HasMaxLength(200);

            entity.Property(e => e.Ativo)
                .HasColumnName("st_ativo")
                .HasColumnType("CHAR(1)")
                .HasMaxLength(1)
                .IsRequired();

            entity.Property(e => e.DataCadastro)
                .HasColumnName("dt_cadastro")
                .HasColumnType("DATE")
                .IsRequired();
        });

        modelBuilder.Entity<EmissaoCarbonoModel>(entity =>
        {
            entity.ToTable("EC_EMISSOES_CARBONO", table =>
            {
                table.HasCheckConstraint("CK_EC_EMISSOES_QTD_EMITIDA", "\"qtd_emitida\" >= 0");
                table.HasCheckConstraint("CK_EC_EMISSOES_QT_ATIVIDADE", "\"qt_atividade\" > 0");
            });

            entity.HasKey(e => e.IdEmissao)
                .HasName("EC_EMISSOES_CARBONO_PK");

            entity.Property(e => e.IdEmissao)
                .HasColumnName("id_emissao")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.IdFator)
                .HasColumnName("id_fator")
                .IsRequired();

            entity.Property(e => e.IdEtapa)
                .HasColumnName("id_etapa")
                .IsRequired();

            entity.Property(e => e.FonteEmissao)
                .HasColumnName("ds_fonte_emissao")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.QuantidadeEmitida)
                .HasColumnName("qtd_emitida")
                .HasColumnType("NUMBER(12,3)")
                .IsRequired();

            entity.Property(e => e.QuantidadeAtividade)
                .HasColumnName("qt_atividade")
                .HasColumnType("NUMBER(12,3)")
                .IsRequired();

            entity.Property(e => e.Unidade)
                .HasColumnName("tp_unidade")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.MetodoCalculo)
                .HasColumnName("ds_metodo_calculo")
                .HasMaxLength(100);

            entity.Property(e => e.Observacao)
                .HasColumnName("ds_observacao")
                .HasMaxLength(500);

            entity.Property(e => e.DataRegistro)
                .HasColumnName("dt_data_registro")
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
