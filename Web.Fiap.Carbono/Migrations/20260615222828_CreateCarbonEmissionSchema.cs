using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Fiap.Carbono.Migrations
{
    /// <inheritdoc />
    public partial class CreateCarbonEmissionSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EC_EMPRESAS",
                columns: table => new
                {
                    id_empresa = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    nm_empresa = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    ds_cnpj = table.Column<string>(type: "NVARCHAR2(18)", maxLength: 18, nullable: false),
                    ds_setor_atuacao = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    nm_cidade = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    nm_estado = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: true),
                    nm_pais = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    dt_cadastro = table.Column<DateTime>(type: "DATE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EC_EMPRESAS_PK", x => x.id_empresa);
                });

            migrationBuilder.CreateTable(
                name: "EC_FATORES_EMISSAO",
                columns: table => new
                {
                    id_fator = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ds_fonte = table.Column<string>(type: "NVARCHAR2(155)", maxLength: 155, nullable: false),
                    tp_escopo = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    ds_unidade_base = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    vl_fator_co2e = table.Column<decimal>(type: "NUMBER(12,6)", nullable: false),
                    ds_referencia = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    st_ativo = table.Column<string>(type: "CHAR(1)", maxLength: 1, nullable: false),
                    dt_cadastro = table.Column<DateTime>(type: "DATE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EC_FATORES_EMISSAO_PK", x => x.id_fator);
                    table.CheckConstraint("CK_EC_FATORES_ATIVO", "\"st_ativo\" IN (''S'', ''N'')");
                    table.CheckConstraint("CK_EC_FATORES_ESCOPO", "\"tp_escopo\" IN (''ESCOPO_1'', ''ESCOPO_2'', ''ESCOPO_3'')");
                    table.CheckConstraint("CK_EC_FATORES_VALOR", "\"vl_fator_co2e\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "EC_FORNECEDORES",
                columns: table => new
                {
                    id_fornecedor = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    nm_fornecedor = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    ds_cnpj = table.Column<string>(type: "NVARCHAR2(18)", maxLength: 18, nullable: false),
                    tp_fornecedor = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    nm_cidade = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    nm_estado = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: true),
                    nm_pais = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    ds_certificacao_esg = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    st_ativo = table.Column<string>(type: "CHAR(1)", maxLength: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EC_FORNECEDORES_PK", x => x.id_fornecedor);
                    table.CheckConstraint("CK_EC_FORNECEDORES_ATIVO", "\"st_ativo\" IN (''S'', ''N'')");
                });

            migrationBuilder.CreateTable(
                name: "EC_PRODUTOS",
                columns: table => new
                {
                    id_produto = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    id_empresa = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    nm_produto = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    ds_descricao = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    tp_categoria = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    ds_unidade_medida = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    st_ativo = table.Column<string>(type: "CHAR(1)", maxLength: 1, nullable: false),
                    dt_cadastro = table.Column<DateTime>(type: "DATE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EC_PRODUTOS_PK", x => x.id_produto);
                    table.CheckConstraint("CK_EC_PRODUTOS_ATIVO", "\"st_ativo\" IN (''S'', ''N'')");
                    table.ForeignKey(
                        name: "FK_EC_PRODUTOS_EMPRESAS",
                        column: x => x.id_empresa,
                        principalTable: "EC_EMPRESAS",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EC_LOTES_PRODUCAO",
                columns: table => new
                {
                    id_lote = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    id_produto = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    cd_codigo_lote = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    qt_quantidade = table.Column<decimal>(type: "NUMBER(10,2)", nullable: false),
                    dt_data_producao = table.Column<DateTime>(type: "DATE", nullable: false),
                    dt_data_validade = table.Column<DateTime>(type: "DATE", nullable: false),
                    st_status_lote = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EC_LOTES_PRODUCAO_PK", x => x.id_lote);
                    table.CheckConstraint("CK_EC_LOTES_DATAS", "\"dt_data_validade\" >= \"dt_data_producao\"");
                    table.CheckConstraint("CK_EC_LOTES_QTDE", "\"qt_quantidade\" > 0");
                    table.ForeignKey(
                        name: "FK_EC_LOTES_PRODUTOS",
                        column: x => x.id_produto,
                        principalTable: "EC_PRODUTOS",
                        principalColumn: "id_produto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EC_ETAPAS_CADEIA",
                columns: table => new
                {
                    id_etapa = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    tp_tipo_etapa = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    ds_descricao = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    ds_origem = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: true),
                    ds_destino = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: true),
                    dt_data_inicio = table.Column<DateTime>(type: "DATE", nullable: false),
                    dt_data_fim = table.Column<DateTime>(type: "DATE", nullable: true),
                    ds_ordem_etapa = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: true),
                    id_fornecedor = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    id_lote = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EC_ETAPAS_CADEIA_PK", x => x.id_etapa);
                    table.CheckConstraint("CK_EC_ETAPAS_DATAS", "\"dt_data_fim\" IS NULL OR \"dt_data_fim\" >= \"dt_data_inicio\"");
                    table.ForeignKey(
                        name: "FK_EC_ETAPAS_FORNECEDORES",
                        column: x => x.id_fornecedor,
                        principalTable: "EC_FORNECEDORES",
                        principalColumn: "id_fornecedor",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EC_ETAPAS_LOTES",
                        column: x => x.id_lote,
                        principalTable: "EC_LOTES_PRODUCAO",
                        principalColumn: "id_lote",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EC_EMISSOES_CARBONO",
                columns: table => new
                {
                    id_emissao = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    id_fator = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    id_etapa = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ds_fonte_emissao = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    qtd_emitida = table.Column<decimal>(type: "NUMBER(12,3)", nullable: false),
                    qt_atividade = table.Column<decimal>(type: "NUMBER(12,3)", nullable: false),
                    tp_unidade = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    ds_metodo_calculo = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    ds_observacao = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    dt_data_registro = table.Column<DateTime>(type: "DATE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EC_EMISSOES_CARBONO_PK", x => x.id_emissao);
                    table.CheckConstraint("CK_EC_EMISSOES_QT_ATIVIDADE", "\"qt_atividade\" > 0");
                    table.CheckConstraint("CK_EC_EMISSOES_QTD_EMITIDA", "\"qtd_emitida\" >= 0");
                    table.ForeignKey(
                        name: "FK_EC_EMISSOES_ETAPAS",
                        column: x => x.id_etapa,
                        principalTable: "EC_ETAPAS_CADEIA",
                        principalColumn: "id_etapa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EC_EMISSOES_FATORES",
                        column: x => x.id_fator,
                        principalTable: "EC_FATORES_EMISSAO",
                        principalColumn: "id_fator",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EC_EMISSOES_CARBONO_id_etapa",
                table: "EC_EMISSOES_CARBONO",
                column: "id_etapa");

            migrationBuilder.CreateIndex(
                name: "IX_EC_EMISSOES_CARBONO_id_fator",
                table: "EC_EMISSOES_CARBONO",
                column: "id_fator");

            migrationBuilder.CreateIndex(
                name: "UK_EC_EMPRESAS_CNPJ",
                table: "EC_EMPRESAS",
                column: "ds_cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EC_ETAPAS_CADEIA_id_fornecedor",
                table: "EC_ETAPAS_CADEIA",
                column: "id_fornecedor");

            migrationBuilder.CreateIndex(
                name: "IX_EC_ETAPAS_CADEIA_id_lote",
                table: "EC_ETAPAS_CADEIA",
                column: "id_lote");

            migrationBuilder.CreateIndex(
                name: "UK_EC_FORNECEDORES_CNPJ",
                table: "EC_FORNECEDORES",
                column: "ds_cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EC_LOTES_PRODUCAO_id_produto",
                table: "EC_LOTES_PRODUCAO",
                column: "id_produto");

            migrationBuilder.CreateIndex(
                name: "IX_EC_PRODUTOS_id_empresa",
                table: "EC_PRODUTOS",
                column: "id_empresa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EC_EMISSOES_CARBONO");

            migrationBuilder.DropTable(
                name: "EC_ETAPAS_CADEIA");

            migrationBuilder.DropTable(
                name: "EC_FATORES_EMISSAO");

            migrationBuilder.DropTable(
                name: "EC_FORNECEDORES");

            migrationBuilder.DropTable(
                name: "EC_LOTES_PRODUCAO");

            migrationBuilder.DropTable(
                name: "EC_PRODUTOS");

            migrationBuilder.DropTable(
                name: "EC_EMPRESAS");
        }
    }
}
