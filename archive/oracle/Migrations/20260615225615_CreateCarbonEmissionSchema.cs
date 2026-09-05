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
                    ID_EMPRESA = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NM_EMPRESA = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    DS_CNPJ = table.Column<string>(type: "NVARCHAR2(18)", maxLength: 18, nullable: false),
                    DS_SETOR_ATUACAO = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    NM_CIDADE = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    NM_ESTADO = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: true),
                    NM_PAIS = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    DT_CADASTRO = table.Column<DateTime>(type: "DATE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EC_EMPRESAS_PK", x => x.ID_EMPRESA);
                });

            migrationBuilder.CreateTable(
                name: "EC_FATORES_EMISSAO",
                columns: table => new
                {
                    ID_FATOR = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    DS_FONTE = table.Column<string>(type: "NVARCHAR2(155)", maxLength: 155, nullable: false),
                    TP_ESCOPO = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    DS_UNIDADE_BASE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    VL_FATOR_CO2E = table.Column<decimal>(type: "NUMBER(12,6)", nullable: false),
                    DS_REFERENCIA = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    ST_ATIVO = table.Column<string>(type: "CHAR(1)", maxLength: 1, nullable: false),
                    DT_CADASTRO = table.Column<DateTime>(type: "DATE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EC_FATORES_EMISSAO_PK", x => x.ID_FATOR);
                    table.CheckConstraint("CK_EC_FATORES_ATIVO", "\"ST_ATIVO\" IN (''S'', ''N'')");
                    table.CheckConstraint("CK_EC_FATORES_ESCOPO", "\"TP_ESCOPO\" IN (''ESCOPO_1'', ''ESCOPO_2'', ''ESCOPO_3'')");
                    table.CheckConstraint("CK_EC_FATORES_VALOR", "\"VL_FATOR_CO2E\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "EC_FORNECEDORES",
                columns: table => new
                {
                    ID_FORNECEDOR = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NM_FORNECEDOR = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    DS_CNPJ = table.Column<string>(type: "NVARCHAR2(18)", maxLength: 18, nullable: false),
                    TP_FORNECEDOR = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    NM_CIDADE = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    NM_ESTADO = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: true),
                    NM_PAIS = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    DS_CERTIFICACAO_ESG = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    ST_ATIVO = table.Column<string>(type: "CHAR(1)", maxLength: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EC_FORNECEDORES_PK", x => x.ID_FORNECEDOR);
                    table.CheckConstraint("CK_EC_FORNECEDORES_ATIVO", "\"ST_ATIVO\" IN (''S'', ''N'')");
                });

            migrationBuilder.CreateTable(
                name: "EC_PRODUTOS",
                columns: table => new
                {
                    ID_PRODUTO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ID_EMPRESA = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NM_PRODUTO = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    DS_DESCRICAO = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    TP_CATEGORIA = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    DS_UNIDADE_MEDIDA = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    ST_ATIVO = table.Column<string>(type: "CHAR(1)", maxLength: 1, nullable: false),
                    DT_CADASTRO = table.Column<DateTime>(type: "DATE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EC_PRODUTOS_PK", x => x.ID_PRODUTO);
                    table.CheckConstraint("CK_EC_PRODUTOS_ATIVO", "\"ST_ATIVO\" IN (''S'', ''N'')");
                    table.ForeignKey(
                        name: "FK_EC_PRODUTOS_EMPRESAS",
                        column: x => x.ID_EMPRESA,
                        principalTable: "EC_EMPRESAS",
                        principalColumn: "ID_EMPRESA",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EC_LOTES_PRODUCAO",
                columns: table => new
                {
                    ID_LOTE = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ID_PRODUTO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CD_CODIGO_LOTE = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    QT_QUANTIDADE = table.Column<decimal>(type: "NUMBER(10,2)", nullable: false),
                    DT_DATA_PRODUCAO = table.Column<DateTime>(type: "DATE", nullable: false),
                    DT_DATA_VALIDADE = table.Column<DateTime>(type: "DATE", nullable: false),
                    ST_STATUS_LOTE = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EC_LOTES_PRODUCAO_PK", x => x.ID_LOTE);
                    table.CheckConstraint("CK_EC_LOTES_DATAS", "\"DT_DATA_VALIDADE\" >= \"DT_DATA_PRODUCAO\"");
                    table.CheckConstraint("CK_EC_LOTES_QTDE", "\"QT_QUANTIDADE\" > 0");
                    table.ForeignKey(
                        name: "FK_EC_LOTES_PRODUTOS",
                        column: x => x.ID_PRODUTO,
                        principalTable: "EC_PRODUTOS",
                        principalColumn: "ID_PRODUTO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EC_ETAPAS_CADEIA",
                columns: table => new
                {
                    ID_ETAPA = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TP_TIPO_ETAPA = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    DS_DESCRICAO = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    DS_ORIGEM = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: true),
                    DS_DESTINO = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: true),
                    DT_DATA_INICIO = table.Column<DateTime>(type: "DATE", nullable: false),
                    DT_DATA_FIM = table.Column<DateTime>(type: "DATE", nullable: true),
                    DS_ORDEM_ETAPA = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: true),
                    ID_FORNECEDOR = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ID_LOTE = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EC_ETAPAS_CADEIA_PK", x => x.ID_ETAPA);
                    table.CheckConstraint("CK_EC_ETAPAS_DATAS", "\"DT_DATA_FIM\" IS NULL OR \"DT_DATA_FIM\" >= \"DT_DATA_INICIO\"");
                    table.ForeignKey(
                        name: "FK_EC_ETAPAS_FORNECEDORES",
                        column: x => x.ID_FORNECEDOR,
                        principalTable: "EC_FORNECEDORES",
                        principalColumn: "ID_FORNECEDOR",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EC_ETAPAS_LOTES",
                        column: x => x.ID_LOTE,
                        principalTable: "EC_LOTES_PRODUCAO",
                        principalColumn: "ID_LOTE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EC_EMISSOES_CARBONO",
                columns: table => new
                {
                    ID_EMISSAO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ID_FATOR = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ID_ETAPA = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DS_FONTE_EMISSAO = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    QTD_EMITIDA = table.Column<decimal>(type: "NUMBER(12,3)", nullable: false),
                    QT_ATIVIDADE = table.Column<decimal>(type: "NUMBER(12,3)", nullable: false),
                    TP_UNIDADE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    DS_METODO_CALCULO = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    DS_OBSERVACAO = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    DT_DATA_REGISTRO = table.Column<DateTime>(type: "DATE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EC_EMISSOES_CARBONO_PK", x => x.ID_EMISSAO);
                    table.CheckConstraint("CK_EC_EMISSOES_QT_ATIVIDADE", "\"QT_ATIVIDADE\" > 0");
                    table.CheckConstraint("CK_EC_EMISSOES_QTD_EMITIDA", "\"QTD_EMITIDA\" >= 0");
                    table.ForeignKey(
                        name: "FK_EC_EMISSOES_ETAPAS",
                        column: x => x.ID_ETAPA,
                        principalTable: "EC_ETAPAS_CADEIA",
                        principalColumn: "ID_ETAPA",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EC_EMISSOES_FATORES",
                        column: x => x.ID_FATOR,
                        principalTable: "EC_FATORES_EMISSAO",
                        principalColumn: "ID_FATOR",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EC_EMISSOES_CARBONO_ID_ETAPA",
                table: "EC_EMISSOES_CARBONO",
                column: "ID_ETAPA");

            migrationBuilder.CreateIndex(
                name: "IX_EC_EMISSOES_CARBONO_ID_FATOR",
                table: "EC_EMISSOES_CARBONO",
                column: "ID_FATOR");

            migrationBuilder.CreateIndex(
                name: "UK_EC_EMPRESAS_CNPJ",
                table: "EC_EMPRESAS",
                column: "DS_CNPJ",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EC_ETAPAS_CADEIA_ID_FORNECEDOR",
                table: "EC_ETAPAS_CADEIA",
                column: "ID_FORNECEDOR");

            migrationBuilder.CreateIndex(
                name: "IX_EC_ETAPAS_CADEIA_ID_LOTE",
                table: "EC_ETAPAS_CADEIA",
                column: "ID_LOTE");

            migrationBuilder.CreateIndex(
                name: "UK_EC_FORNECEDORES_CNPJ",
                table: "EC_FORNECEDORES",
                column: "DS_CNPJ",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EC_LOTES_PRODUCAO_ID_PRODUTO",
                table: "EC_LOTES_PRODUCAO",
                column: "ID_PRODUTO");

            migrationBuilder.CreateIndex(
                name: "IX_EC_PRODUTOS_ID_EMPRESA",
                table: "EC_PRODUTOS",
                column: "ID_EMPRESA");
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
