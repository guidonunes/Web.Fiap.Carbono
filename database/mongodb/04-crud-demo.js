const targetDb = db.getSiblingDB("fiap_carbono");

const COLLECTIONS = [
    "empresas",
    "produtos",
    "fornecedores",
    "fatores_emissao",
    "emissoes_carbono"
];

const TEMP_CODES = {
    empresa: "CRUD-TEMP-EMP",
    produto: "CRUD-TEMP-PROD",
    fornecedor: "CRUD-TEMP-FOR",
    fator: "CRUD-TEMP-FE",
    emissao: "CRUD-TEMP-EMISSAO"
};

function assertCondition(condition, message) {
    if (!condition) {
        throw new Error(`ASSERTION FAILED: ${message}`);
    }
}

function printSection(title) {
    print("");
    print("==================================================");
    print(title);
    print("==================================================");
}

function assertInsert(result, description) {
    assertCondition(
        result.acknowledged === true,
        `${description}: insert was not acknowledged`
    );

    assertCondition(
        result.insertedId !== undefined,
        `${description}: insertedId was not returned`
    );
}

function assertUpdate(result, description) {
    assertCondition(
        result.acknowledged === true,
        `${description}: update was not acknowledged`
    );

    assertCondition(
        result.matchedCount === 1,
        `${description}: expected matchedCount=1, got ${result.matchedCount}`
    );

    assertCondition(
        result.modifiedCount === 1,
        `${description}: expected modifiedCount=1, got ${result.modifiedCount}`
    );
}

function assertDelete(result, description) {
    assertCondition(
        result.acknowledged === true,
        `${description}: delete was not acknowledged`
    );

    assertCondition(
        result.deletedCount === 1,
        `${description}: expected deletedCount=1, got ${result.deletedCount}`
    );
}

function multiplyDecimals(firstValue, secondValue) {
    const cursor = targetDb.aggregate([
        {
            $documents: [
                {
                    firstValue,
                    secondValue
                }
            ]
        },
        {
            $project: {
                _id: 0,
                result: {
                    $multiply: [
                        "$firstValue",
                        "$secondValue"
                    ]
                }
            }
        }
    ]);

    return cursor.next().result;
}

printSection("CHECKING REQUIRED COLLECTIONS");

const existingCollections = new Set(
    targetDb.getCollectionNames()
);

COLLECTIONS.forEach(collectionName => {
    assertCondition(
        existingCollections.has(collectionName),
        `Collection does not exist: ${collectionName}`
    );

    print(`OK: ${collectionName}`);
});

printSection("CLEANING INTERRUPTED TEMPORARY RECORDS");

targetDb.empresas.deleteOne({
    codigo: TEMP_CODES.empresa
});

targetDb.produtos.deleteOne({
    codigo: TEMP_CODES.produto
});

targetDb.fornecedores.deleteOne({
    codigo: TEMP_CODES.fornecedor
});

targetDb.fatores_emissao.deleteOne({
    codigo: TEMP_CODES.fator
});

targetDb.emissoes_carbono.deleteOne({
    codigo: TEMP_CODES.emissao
});

print("Temporary cleanup completed.");

printSection("CAPTURING PERMANENT BASELINE COUNTS");

const baselineCounts = {};

COLLECTIONS.forEach(collectionName => {
    const count = targetDb
        .getCollection(collectionName)
        .countDocuments();

    baselineCounts[collectionName] = count;

    print(`${collectionName}: ${count}`);
});

assertCondition(
    baselineCounts.empresas >= 10,
    "Baseline requires at least 10 empresas"
);

assertCondition(
    baselineCounts.produtos >= 10,
    "Baseline requires at least 10 produtos"
);

assertCondition(
    baselineCounts.fornecedores >= 10,
    "Baseline requires at least 10 fornecedores"
);

assertCondition(
    baselineCounts.fatores_emissao >= 10,
    "Baseline requires at least 10 fatores_emissao"
);

assertCondition(
    baselineCounts.emissoes_carbono >= 15,
    "Baseline requires at least 15 emissoes_carbono"
);

print("Baseline counts captured successfully.");

printSection("CRUD — EMPRESAS");

const empresaTemporaria = {
    codigo: TEMP_CODES.empresa,
    razaoSocial: "Empresa Temporária ESG Ltda.",
    nomeFantasia: "Empresa CRUD ESG",
    cnpj: "90000000000101",
    setor: "TECNOLOGIA_AMBIENTAL",
    ativa: true,

    metasReducao: [
        {
            tipo: "EMISSOES_GEE",
            anoBase: 2026,
            anoMeta: 2030,
            percentualReducao: NumberDecimal("25")
        }
    ],

    governanca: {
        responsavelEsg: "Responsável Temporário",
        comiteEsg: false,
        frequenciaAuditoria: "ANUAL"
    },

    criadoEm: ISODate("2026-09-01T10:00:00Z"),
    atualizadoEm: ISODate("2026-09-01T10:00:00Z"),
    schemaVersion: NumberInt(1)
};

// INSERT
const insertEmpresaResult =
    targetDb.empresas.insertOne(empresaTemporaria);

assertInsert(
    insertEmpresaResult,
    "empresas insertOne"
);

printjson(insertEmpresaResult);

// READ
const empresaInserida =
    targetDb.empresas.findOne({
        codigo: TEMP_CODES.empresa
    });

assertCondition(
    empresaInserida !== null,
    "Inserted company could not be read"
);

printjson(empresaInserida);

// UPDATE
const updateEmpresaResult =
    targetDb.empresas.updateOne(
        {
            codigo: TEMP_CODES.empresa
        },
        {
            $set: {
                razaoSocial: "Empresa Temporária ESG Atualizada Ltda.",
                "governanca.comiteEsg": true,
                atualizadoEm: ISODate("2026-09-01T11:00:00Z")
            }
        }
    );

assertUpdate(
    updateEmpresaResult,
    "empresas updateOne"
);

printjson(updateEmpresaResult);

// SECOND READ
const empresaAtualizada =
    targetDb.empresas.findOne({
        codigo: TEMP_CODES.empresa
    });

assertCondition(
    empresaAtualizada !== null,
    "Updated company could not be read"
);

assertCondition(
    empresaAtualizada.razaoSocial ===
    "Empresa Temporária ESG Atualizada Ltda.",
    "Company did not contain the updated value"
);

printjson(empresaAtualizada);

// DELETE
const deleteEmpresaResult =
    targetDb.empresas.deleteOne({
        codigo: TEMP_CODES.empresa
    });

assertDelete(
    deleteEmpresaResult,
    "empresas deleteOne"
);

printjson(deleteEmpresaResult);

// ABSENCE CHECK
const empresaRemanescente =
    targetDb.empresas.findOne({
        codigo: TEMP_CODES.empresa
    });

assertCondition(
    empresaRemanescente === null,
    "Temporary company remains after delete"
);

print("Final absence check: null");

// COUNT
const empresasCount =
    targetDb.empresas.countDocuments();

print(`Permanent empresas count: ${empresasCount}`);

assertCondition(
    empresasCount >= 10,
    `Expected at least 10 empresas, got ${empresasCount}`
);


printSection("CRUD — PRODUTOS");

// Resolve the permanent EMP-001 company.
const empresaProdutoCrud =
    targetDb.empresas.findOne(
        {
            codigo: "EMP-001"
        },
        {
            _id: 1,
            codigo: 1,
            razaoSocial: 1
        }
    );

assertCondition(
    empresaProdutoCrud !== null,
    "Permanent company EMP-001 was not found"
);

assertCondition(
    empresaProdutoCrud._id !== undefined,
    "EMP-001 does not have a valid _id"
);

print("Resolved permanent company:");
printjson(empresaProdutoCrud);


// 1. INSERT
const produtoTemporario = {
    empresaId: empresaProdutoCrud._id,
    codigo: TEMP_CODES.produto,
    nome: "Produto Temporário Sustentável",
    unidadeFuncional: "unidade",
    ativo: true,

    atributosAmbientais: {
        percentualReciclavel: NumberDecimal("75"),
        percentualMaterialReciclado: NumberDecimal("40"),
        embalagem: "PAPEL_RECICLADO",
        seloSustentabilidade: "PRODUTO_DEMONSTRATIVO"
    },

    materiais: [
        {
            nome: "Material reciclado",
            percentualComposicao: NumberDecimal("40"),
            origemReciclada: true
        },
        {
            nome: "Material renovável",
            percentualComposicao: NumberDecimal("60"),
            origemRenovavel: true
        }
    ],

    criadoEm: ISODate("2026-09-01T10:00:00Z"),
    atualizadoEm: ISODate("2026-09-01T10:00:00Z"),
    schemaVersion: NumberInt(1)
};

const insertProdutoResult =
    targetDb.produtos.insertOne(produtoTemporario);

assertInsert(
    insertProdutoResult,
    "produtos insertOne"
);

print("Insert result:");
printjson(insertProdutoResult);


// 2. FIRST FIND
const produtoInserido =
    targetDb.produtos.findOne({
        empresaId: empresaProdutoCrud._id,
        codigo: TEMP_CODES.produto
    });

assertCondition(
    produtoInserido !== null,
    "Inserted temporary product could not be read"
);

assertCondition(
    produtoInserido.empresaId.equals(
        empresaProdutoCrud._id
    ),
    "Temporary product has an incorrect empresaId"
);

print("Inserted product:");
printjson(produtoInserido);


// 3. UPDATE
const updateProdutoResult =
    targetDb.produtos.updateOne(
        {
            empresaId: empresaProdutoCrud._id,
            codigo: TEMP_CODES.produto
        },
        {
            $set: {
                nome: "Produto Temporário Sustentável Atualizado",
                "atributosAmbientais.percentualReciclavel":
                    NumberDecimal("90"),
                "atributosAmbientais.embalagem":
                    "PAPELAO_REUTILIZAVEL",
                atualizadoEm:
                    ISODate("2026-09-01T11:00:00Z")
            }
        }
    );

assertUpdate(
    updateProdutoResult,
    "produtos updateOne"
);

print("Update result:");
printjson(updateProdutoResult);


// 4. SECOND FIND
const produtoAtualizado =
    targetDb.produtos.findOne({
        empresaId: empresaProdutoCrud._id,
        codigo: TEMP_CODES.produto
    });

assertCondition(
    produtoAtualizado !== null,
    "Updated temporary product could not be read"
);

assertCondition(
    produtoAtualizado.nome ===
    "Produto Temporário Sustentável Atualizado",
    "Product name was not updated"
);

assertCondition(
    produtoAtualizado
        .atributosAmbientais
        .embalagem === "PAPELAO_REUTILIZAVEL",
    "Product packaging was not updated"
);

assertCondition(
    produtoAtualizado
        .atributosAmbientais
        .percentualReciclavel
        .toString() === "90",
    "Product recyclable percentage was not updated"
);

print("Updated product:");
printjson(produtoAtualizado);


// 5. DELETE
const deleteProdutoResult =
    targetDb.produtos.deleteOne({
        empresaId: empresaProdutoCrud._id,
        codigo: TEMP_CODES.produto
    });

assertDelete(
    deleteProdutoResult,
    "produtos deleteOne"
);

print("Delete result:");
printjson(deleteProdutoResult);


// 6. FINAL ABSENCE CHECK
const produtoRemanescente =
    targetDb.produtos.findOne({
        empresaId: empresaProdutoCrud._id,
        codigo: TEMP_CODES.produto
    });

assertCondition(
    produtoRemanescente === null,
    "Temporary product remains after delete"
);

print("Final absence check: null");


// 7. FINAL COUNT
const produtosCount =
    targetDb.produtos.countDocuments();

print(`Permanent produtos count: ${produtosCount}`);

assertCondition(
    produtosCount >= 10,
    `Expected at least 10 produtos, got ${produtosCount}`
);

printSection("CRUD — FORNECEDORES");


// 1. INSERT
const fornecedorTemporario = {
    codigo: TEMP_CODES.fornecedor,
    razaoSocial: "Fornecedor Temporário ESG Ltda.",
    nomeFantasia: "Fornecedor CRUD ESG",
    cnpj: "90000000000292",
    ativo: true,

    categoriasAtuacao: [
        "LOGISTICA",
        "MATERIAIS_RECICLADOS"
    ],

    certificacoes: [
        {
            nome: "ISO 14001",
            emissor: "Organismo Certificador Demonstrativo",
            validaAte: ISODate("2028-12-31T00:00:00Z")
        }
    ],

    indicadoresSociais: {
        acidentesUltimos12Meses: 1,
        percentualMulheresLideranca: NumberDecimal("35"),
        possuiProgramaDiversidade: true
    },

    conformidadeAmbiental: {
        possuiLicenca: true,
        ocorrenciasUltimos12Meses: 1,
        descarteMonitorado: true,
        dataUltimaAuditoria:
            ISODate("2026-08-01T00:00:00Z")
    },

    statusAuditoria: "PENDENTE_ADEQUACAO",
    nivelRiscoEsg: "MEDIO",

    criadoEm: ISODate("2026-09-01T10:00:00Z"),
    atualizadoEm: ISODate("2026-09-01T10:00:00Z"),
    schemaVersion: NumberInt(1)
};

const insertFornecedorResult =
    targetDb.fornecedores.insertOne(
        fornecedorTemporario
    );

assertInsert(
    insertFornecedorResult,
    "fornecedores insertOne"
);

print("Insert result:");
printjson(insertFornecedorResult);


// 2. FIRST FIND
const fornecedorInserido =
    targetDb.fornecedores.findOne({
        codigo: TEMP_CODES.fornecedor
    });

assertCondition(
    fornecedorInserido !== null,
    "Inserted temporary supplier could not be read"
);

assertCondition(
    fornecedorInserido.cnpj === "90000000000292",
    "Temporary supplier has an unexpected CNPJ"
);

print("Inserted supplier:");
printjson(fornecedorInserido);


// 3. UPDATE
const updateFornecedorResult =
    targetDb.fornecedores.updateOne(
        {
            codigo: TEMP_CODES.fornecedor
        },
        {
            $set: {
                razaoSocial:
                    "Fornecedor Temporário ESG Atualizado Ltda.",

                "indicadoresSociais.acidentesUltimos12Meses": 0,

                "indicadoresSociais.percentualMulheresLideranca":
                    NumberDecimal("45"),

                "conformidadeAmbiental.ocorrenciasUltimos12Meses":
                    0,

                statusAuditoria: "APROVADO",

                nivelRiscoEsg: "BAIXO",

                "conformidadeAmbiental.dataUltimaAuditoria":
                    ISODate("2026-09-01T00:00:00Z"),

                atualizadoEm:
                    ISODate("2026-09-01T11:00:00Z")
            }
        }
    );

assertUpdate(
    updateFornecedorResult,
    "fornecedores updateOne"
);

print("Update result:");
printjson(updateFornecedorResult);


// 4. SECOND FIND
const fornecedorAtualizado =
    targetDb.fornecedores.findOne({
        codigo: TEMP_CODES.fornecedor
    });

assertCondition(
    fornecedorAtualizado !== null,
    "Updated temporary supplier could not be read"
);

assertCondition(
    fornecedorAtualizado.razaoSocial ===
    "Fornecedor Temporário ESG Atualizado Ltda.",
    "Supplier business name was not updated"
);

assertCondition(
    fornecedorAtualizado.statusAuditoria ===
    "APROVADO",
    "Supplier audit status was not updated"
);

assertCondition(
    fornecedorAtualizado.nivelRiscoEsg ===
    "BAIXO",
    "Supplier ESG risk level was not updated"
);

assertCondition(
    fornecedorAtualizado
        .conformidadeAmbiental
        .ocorrenciasUltimos12Meses === 0,
    "Supplier environmental occurrences were not updated"
);

assertCondition(
    fornecedorAtualizado
        .indicadoresSociais
        .percentualMulheresLideranca
        .toString() === "45",
    "Supplier social indicator was not updated"
);

print("Updated supplier:");
printjson(fornecedorAtualizado);


// 5. DELETE
const deleteFornecedorResult =
    targetDb.fornecedores.deleteOne({
        codigo: TEMP_CODES.fornecedor
    });

assertDelete(
    deleteFornecedorResult,
    "fornecedores deleteOne"
);

print("Delete result:");
printjson(deleteFornecedorResult);


// 6. FINAL ABSENCE CHECK
const fornecedorRemanescente =
    targetDb.fornecedores.findOne({
        codigo: TEMP_CODES.fornecedor
    });

assertCondition(
    fornecedorRemanescente === null,
    "Temporary supplier remains after delete"
);

print("Final absence check: null");


// 7. FINAL COUNT
const fornecedoresCount =
    targetDb.fornecedores.countDocuments();

print(
    `Permanent fornecedores count: ${fornecedoresCount}`
);

assertCondition(
    fornecedoresCount >= 10,
    `Expected at least 10 fornecedores, got ${fornecedoresCount}`
);

printSection("CRUD — FATORES DE EMISSÃO");


// 1. INSERT
const fatorTemporario = {
    codigo: TEMP_CODES.fator,
    nome: "Transporte temporário demonstrativo",
    categoria: "TRANSPORTE",
    valor: NumberDecimal("0.1500"),
    unidadeBase: "ton_km",
    escopo: "ESCOPO_3",
    versao: NumberInt(1),

    fonteReferencia:
        "Dataset demonstrativo FIAP",

    metodologia:
        "Carga transportada multiplicada pela distância",

    validoDe:
        ISODate("2026-01-01T00:00:00Z"),

    validoAte:
        ISODate("2026-12-31T23:59:59Z"),

    ativo: true,

    criadoEm:
        ISODate("2026-09-01T10:00:00Z"),

    atualizadoEm:
        ISODate("2026-09-01T10:00:00Z"),

    schemaVersion: NumberInt(1)
};

const insertFatorResult =
    targetDb.fatores_emissao.insertOne(
        fatorTemporario
    );

assertInsert(
    insertFatorResult,
    "fatores_emissao insertOne"
);

print("Insert result:");
printjson(insertFatorResult);


// 2. FIRST FIND
const fatorInserido =
    targetDb.fatores_emissao.findOne({
        codigo: TEMP_CODES.fator,
        versao: NumberInt(1)
    });

assertCondition(
    fatorInserido !== null,
    "Inserted temporary emission factor could not be read"
);

assertCondition(
    fatorInserido.valor.toString() === "0.1500",
    "Temporary factor has an unexpected value"
);

assertCondition(
    fatorInserido.escopo === "ESCOPO_3",
    "Temporary factor has an unexpected scope"
);

print("Inserted emission factor:");
printjson(fatorInserido);


// 3. UPDATE
const updateFatorResult =
    targetDb.fatores_emissao.updateOne(
        {
            codigo: TEMP_CODES.fator,
            versao: NumberInt(1)
        },
        {
            $set: {
                nome:
                    "Transporte temporário demonstrativo atualizado",

                valor:
                    NumberDecimal("0.1350"),

                fonteReferencia:
                    "Dataset demonstrativo FIAP revisado",

                metodologia:
                    "Toneladas-quilômetro multiplicadas pelo fator revisado",

                atualizadoEm:
                    ISODate("2026-09-01T11:00:00Z")
            }
        }
    );

assertUpdate(
    updateFatorResult,
    "fatores_emissao updateOne"
);

print("Update result:");
printjson(updateFatorResult);


// 4. SECOND FIND
const fatorAtualizado =
    targetDb.fatores_emissao.findOne({
        codigo: TEMP_CODES.fator,
        versao: NumberInt(1)
    });

assertCondition(
    fatorAtualizado !== null,
    "Updated temporary emission factor could not be read"
);

assertCondition(
    fatorAtualizado.nome ===
    "Transporte temporário demonstrativo atualizado",
    "Emission factor name was not updated"
);

assertCondition(
    fatorAtualizado.valor.toString() === "0.1350",
    "Emission factor Decimal128 value was not updated"
);

assertCondition(
    fatorAtualizado.fonteReferencia ===
    "Dataset demonstrativo FIAP revisado",
    "Emission factor reference source was not updated"
);

assertCondition(
    fatorAtualizado.versao === 1,
    "Emission factor version changed unexpectedly"
);

assertCondition(
    fatorAtualizado.escopo === "ESCOPO_3",
    "Emission factor scope changed unexpectedly"
);

assertCondition(
    fatorAtualizado.categoria === "TRANSPORTE",
    "Emission factor category changed unexpectedly"
);

print("Updated emission factor:");
printjson(fatorAtualizado);


// 5. DELETE
const deleteFatorResult =
    targetDb.fatores_emissao.deleteOne({
        codigo: TEMP_CODES.fator,
        versao: NumberInt(1)
    });

assertDelete(
    deleteFatorResult,
    "fatores_emissao deleteOne"
);

print("Delete result:");
printjson(deleteFatorResult);


// 6. FINAL ABSENCE CHECK
const fatorRemanescente =
    targetDb.fatores_emissao.findOne({
        codigo: TEMP_CODES.fator,
        versao: NumberInt(1)
    });

assertCondition(
    fatorRemanescente === null,
    "Temporary emission factor remains after delete"
);

print("Final absence check: null");


// 7. FINAL COUNT
const fatoresEmissaoCount =
    targetDb.fatores_emissao.countDocuments();

print(
    `Permanent fatores_emissao count: ${fatoresEmissaoCount}`
);

assertCondition(
    fatoresEmissaoCount >= 10,
    `Expected at least 10 fatores_emissao, got ${fatoresEmissaoCount}`
);

printSection("CRUD — EMISSÕES DE CARBONO");


// Resolve permanent company EMP-001.
const empresaEmissaoCrud =
    targetDb.empresas.findOne({
        codigo: "EMP-001"
    });

assertCondition(
    empresaEmissaoCrud !== null,
    "Permanent company EMP-001 was not found"
);


// Resolve permanent product PRO-001 belonging to EMP-001.
const produtoEmissaoCrud =
    targetDb.produtos.findOne({
        empresaId: empresaEmissaoCrud._id,
        codigo: "PRO-001"
    });

assertCondition(
    produtoEmissaoCrud !== null,
    "Permanent product PRO-001 was not found for EMP-001"
);


// Resolve permanent supplier FOR-001.
const fornecedorEmissaoCrud =
    targetDb.fornecedores.findOne({
        codigo: "FOR-001"
    });

assertCondition(
    fornecedorEmissaoCrud !== null,
    "Permanent supplier FOR-001 was not found"
);


// Resolve permanent emission factor.
const fatorEmissaoCrud =
    targetDb.fatores_emissao.findOne({
        codigo: "FE-TRANSPORTE-001",
        versao: NumberInt(1)
    });

assertCondition(
    fatorEmissaoCrud !== null,
    "Factor FE-TRANSPORTE-001 version 1 was not found"
);

assertCondition(
    fatorEmissaoCrud.ativo === true,
    "Factor FE-TRANSPORTE-001 version 1 is inactive"
);

assertCondition(
    fatorEmissaoCrud.valor.toString() === "0.1200",
    "Expected factor value 0.1200"
);

assertCondition(
    fatorEmissaoCrud.unidadeBase === "ton_km",
    "Expected factor base unit ton_km"
);

assertCondition(
    fatorEmissaoCrud.escopo === "ESCOPO_3",
    "Expected factor scope ESCOPO_3"
);

print("Resolved company:");
printjson(empresaEmissaoCrud);

print("Resolved product:");
printjson(produtoEmissaoCrud);

print("Resolved supplier:");
printjson(fornecedorEmissaoCrud);

print("Resolved factor:");
printjson(fatorEmissaoCrud);


// Calculate with MongoDB Decimal128 arithmetic:
//
// 10 ton_km × 0.1200 kgCO2e/ton_km
// = 1.2000 kgCO2e

const quantidadeAtividadeCrud =
    NumberDecimal("10");

const quantidadeEmitidaCrud =
    multiplyDecimals(
        quantidadeAtividadeCrud,
        fatorEmissaoCrud.valor
    );

assertCondition(
    quantidadeEmitidaCrud.toString() === "1.2000",
    `Expected calculated emission 1.2000, got ${
        quantidadeEmitidaCrud.toString()
    }`
);

print(
    `Calculated emission: ${
        quantidadeEmitidaCrud.toString()
    } kgCO2e`
);


// 1. INSERT
const emissaoTemporaria = {
    codigo: TEMP_CODES.emissao,

    empresaId: empresaEmissaoCrud._id,
    produtoId: produtoEmissaoCrud._id,
    fornecedorId: fornecedorEmissaoCrud._id,
    fatorEmissaoId: fatorEmissaoCrud._id,

    lote: {
        codigo: "LOTE-CRUD-TEMP",
        quantidadeProduzida: NumberDecimal("100"),
        unidade: "unidades",
        dataProducao:
            ISODate("2026-09-01T00:00:00Z")
    },

    etapa: {
        nome: "Transporte temporário demonstrativo",
        ordem: NumberInt(1),
        categoria: "TRANSPORTE",
        local: "São Paulo"
    },

    quantidadeAtividade:
    quantidadeAtividadeCrud,

    dadosAtividade: {
        tipo: "TRANSPORTE",
        distanciaKm: NumberDecimal("10"),
        cargaToneladas: NumberDecimal("1"),
        combustivel: "DIESEL",
        modal: "RODOVIARIO"
    },

    // Immutable snapshot of the factor used.
    fatorAplicado: {
        codigo: fatorEmissaoCrud.codigo,
        nome: fatorEmissaoCrud.nome,
        valor: fatorEmissaoCrud.valor,
        unidadeBase: fatorEmissaoCrud.unidadeBase,
        escopo: fatorEmissaoCrud.escopo,
        versao: fatorEmissaoCrud.versao,
        fonteReferencia:
        fatorEmissaoCrud.fonteReferencia,
        metodologia:
        fatorEmissaoCrud.metodologia
    },

    quantidadeEmitidaKgCO2e:
    quantidadeEmitidaCrud,

    metodoCalculo:
        "quantidadeAtividade × fatorAplicado.valor",

    fonteEmissao:
        "Transporte rodoviário temporário",

    observacao:
        "Registro temporário para demonstração CRUD",

    calculadoPor:
        "admin@carbono.com",

    dataEmissao:
        ISODate("2026-09-01T10:00:00Z"),

    criadoEm:
        ISODate("2026-09-01T10:00:00Z"),

    atualizadoEm:
        ISODate("2026-09-01T10:00:00Z"),

    schemaVersion: NumberInt(1)
};

const insertEmissaoResult =
    targetDb.emissoes_carbono.insertOne(
        emissaoTemporaria
    );

assertInsert(
    insertEmissaoResult,
    "emissoes_carbono insertOne"
);

print("Insert result:");
printjson(insertEmissaoResult);


// 2. FIRST FIND
const emissaoInserida =
    targetDb.emissoes_carbono.findOne({
        codigo: TEMP_CODES.emissao
    });

assertCondition(
    emissaoInserida !== null,
    "Inserted temporary emission could not be read"
);

assertCondition(
    emissaoInserida.empresaId.equals(
        empresaEmissaoCrud._id
    ),
    "Temporary emission has an incorrect empresaId"
);

assertCondition(
    emissaoInserida.produtoId.equals(
        produtoEmissaoCrud._id
    ),
    "Temporary emission has an incorrect produtoId"
);

assertCondition(
    emissaoInserida.fornecedorId.equals(
        fornecedorEmissaoCrud._id
    ),
    "Temporary emission has an incorrect fornecedorId"
);

assertCondition(
    emissaoInserida.fatorEmissaoId.equals(
        fatorEmissaoCrud._id
    ),
    "Temporary emission has an incorrect fatorEmissaoId"
);

assertCondition(
    emissaoInserida
        .quantidadeEmitidaKgCO2e
        .toString() === "1.2000",
    "Stored emission result is not 1.2000"
);

print("Inserted emission:");
printjson(emissaoInserida);


// Preserve the calculated values before the audit update.
const quantidadeAntesUpdate =
    emissaoInserida.quantidadeAtividade.toString();

const totalAntesUpdate =
    emissaoInserida
        .quantidadeEmitidaKgCO2e
        .toString();

const fatorValorAntesUpdate =
    emissaoInserida
        .fatorAplicado
        .valor
        .toString();

const fatorCodigoAntesUpdate =
    emissaoInserida.fatorAplicado.codigo;

const fatorVersaoAntesUpdate =
    emissaoInserida
        .fatorAplicado
        .versao
        .toString();


// 3. UPDATE ONLY AUDIT METADATA
const updateEmissaoResult =
    targetDb.emissoes_carbono.updateOne(
        {
            codigo: TEMP_CODES.emissao
        },
        {
            $set: {
                observacao:
                    "Registro temporário revisado durante o CRUD",

                revisadoEm:
                    ISODate("2026-09-01T11:00:00Z"),

                revisadoPor:
                    "auditoria@carbono.com",

                atualizadoEm:
                    ISODate("2026-09-01T11:00:00Z")
            }
        }
    );

assertUpdate(
    updateEmissaoResult,
    "emissoes_carbono updateOne"
);

print("Update result:");
printjson(updateEmissaoResult);


// 4. SECOND FIND
const emissaoAtualizada =
    targetDb.emissoes_carbono.findOne({
        codigo: TEMP_CODES.emissao
    });

assertCondition(
    emissaoAtualizada !== null,
    "Updated temporary emission could not be read"
);

assertCondition(
    emissaoAtualizada.observacao ===
    "Registro temporário revisado durante o CRUD",
    "Emission observation was not updated"
);

assertCondition(
    emissaoAtualizada.revisadoPor ===
    "auditoria@carbono.com",
    "Emission reviewer was not updated"
);

assertCondition(
    emissaoAtualizada.revisadoEm !== undefined,
    "Emission revision timestamp was not stored"
);


// Confirm calculation inputs were not changed.
assertCondition(
    emissaoAtualizada
        .quantidadeAtividade
        .toString() === quantidadeAntesUpdate,
    "quantidadeAtividade changed during audit update"
);

assertCondition(
    emissaoAtualizada
        .quantidadeEmitidaKgCO2e
        .toString() === totalAntesUpdate,
    "Calculated emission result changed during audit update"
);


// Confirm the applied-factor snapshot was not changed.
assertCondition(
    emissaoAtualizada
        .fatorAplicado
        .valor
        .toString() === fatorValorAntesUpdate,
    "Applied factor value changed during audit update"
);

assertCondition(
    emissaoAtualizada.fatorAplicado.codigo ===
    fatorCodigoAntesUpdate,
    "Applied factor code changed during audit update"
);

assertCondition(
    emissaoAtualizada
        .fatorAplicado
        .versao
        .toString() === fatorVersaoAntesUpdate,
    "Applied factor version changed during audit update"
);

print("Updated emission:");
printjson(emissaoAtualizada);


// 5. DELETE
const deleteEmissaoResult =
    targetDb.emissoes_carbono.deleteOne({
        codigo: TEMP_CODES.emissao
    });

assertDelete(
    deleteEmissaoResult,
    "emissoes_carbono deleteOne"
);

print("Delete result:");
printjson(deleteEmissaoResult);


// 6. FINAL ABSENCE CHECK
const emissaoRemanescente =
    targetDb.emissoes_carbono.findOne({
        codigo: TEMP_CODES.emissao
    });

assertCondition(
    emissaoRemanescente === null,
    "Temporary emission remains after delete"
);

print("Final absence check: null");


// 7. FINAL COUNT
const emissoesCarbonoCount =
    targetDb.emissoes_carbono.countDocuments();

print(
    `Permanent emissoes_carbono count: ${
        emissoesCarbonoCount
    }`
);

assertCondition(
    emissoesCarbonoCount >= 15,
    `Expected at least 15 emissoes_carbono, got ${
        emissoesCarbonoCount
    }`
);

printSection("FINAL DOCUMENT COUNTS");

const minimumCounts = {
    empresas: 10,
    produtos: 10,
    fornecedores: 10,
    fatores_emissao: 10,
    emissoes_carbono: 15
};

const finalCounts = {};

COLLECTIONS.forEach(collectionName => {
    const finalCount = targetDb
        .getCollection(collectionName)
        .countDocuments();

    finalCounts[collectionName] = finalCount;

    print(
        `${collectionName}: ` +
        `baseline=${baselineCounts[collectionName]}, ` +
        `final=${finalCount}, ` +
        `minimum=${minimumCounts[collectionName]}`
    );

    assertCondition(
        finalCount >= minimumCounts[collectionName],
        `${collectionName}: expected at least ` +
        `${minimumCounts[collectionName]}, got ${finalCount}`
    );

    assertCondition(
        finalCount === baselineCounts[collectionName],
        `${collectionName}: permanent count changed from ` +
        `${baselineCounts[collectionName]} to ${finalCount}`
    );
});

print("");
print("Final count summary:");

printjson(
    COLLECTIONS.map(collectionName => ({
        collection: collectionName,
        baseline: baselineCounts[collectionName],
        final: finalCounts[collectionName],
        minimum: minimumCounts[collectionName],
        unchanged:
            baselineCounts[collectionName] ===
            finalCounts[collectionName]
    }))
);

print("All permanent collection counts remain unchanged.");

printSection("FINAL TEMPORARY ABSENCE ASSERTIONS");

const temporaryRecords = [
    {
        collection: "empresas",
        identifier: TEMP_CODES.empresa,
        filter: {
            codigo: TEMP_CODES.empresa
        }
    },

    {
        collection: "produtos",
        identifier: TEMP_CODES.produto,
        filter: {
            codigo: TEMP_CODES.produto
        }
    },

    {
        collection: "fornecedores",
        identifier: TEMP_CODES.fornecedor,
        filter: {
            codigo: TEMP_CODES.fornecedor
        }
    },

    {
        collection: "fatores_emissao",
        identifier: TEMP_CODES.fator,
        filter: {
            codigo: TEMP_CODES.fator
        }
    },

    {
        collection: "emissoes_carbono",
        identifier: TEMP_CODES.emissao,
        filter: {
            codigo: TEMP_CODES.emissao
        }
    }
];

temporaryRecords.forEach(temporaryRecord => {
    const remainingCount = targetDb
        .getCollection(temporaryRecord.collection)
        .countDocuments(temporaryRecord.filter);

    assertCondition(
        remainingCount === 0,
        `${temporaryRecord.collection}: temporary record ` +
        `${temporaryRecord.identifier} remains in the database`
    );

    print(
        `ABSENT: ${temporaryRecord.collection} / ` +
        `${temporaryRecord.identifier}`
    );
});

print("");
print("All temporary records were removed successfully.");
print("All permanent document counts remain unchanged.");
print("CRUD demonstration completed successfully.");
