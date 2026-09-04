"use strict";

const targetDb = db.getSiblingDB("fiap_carbono");

const pageNumber = 1;
const pageSize = 10;
const skip = (pageNumber - 1) * pageSize;

function requireDocument(collectionName, filter, description) {
    const document = targetDb
        .getCollection(collectionName)
        .findOne(filter);

    if (document === null) {
        throw new Error(
            `Documento obrigatório não encontrado: ${description}`
        );
    }

    return document;
}

function printResult(title, result) {
    print("");
    print(`=== ${title} ===`);
    print(EJSON.stringify(result, null, 2));
}

/*
 * Product footprint
 *
 * Equivalent endpoint:
 * GET /api/produtos-carbono/{idProduto}/pegada
 */

const produtoPegada = requireDocument(
    "produtos",
    { codigo: "PRO-001" },
    "produto PRO-001"
);

const empresaProduto = requireDocument(
    "empresas",
    { _id: produtoPegada.empresaId },
    "empresa do produto PRO-001"
);

const productFootprintPipeline = [
    // Select only emissions belonging to the requested product.
    {
        $match: {
            produtoId: produtoPegada._id
        }
    },

    // Produce the total and category breakdown from one event set.
    {
        $facet: {
            total: [
                {
                    $group: {
                        _id: null,
                        totalKgCO2e: {
                            $sum: "$quantidadeEmitidaKgCO2e"
                        }
                    }
                }
            ],

            porEtapa: [
                // Group emissions by supply-chain category.
                {
                    $group: {
                        _id: "$etapa.categoria",
                        totalKgCO2e: {
                            $sum: "$quantidadeEmitidaKgCO2e"
                        }
                    }
                },

                // Keep the category breakdown deterministic.
                {
                    $sort: {
                        _id: 1
                    }
                }
            ]
        }
    }
];

const productAggregation = targetDb.emissoes_carbono
    .aggregate(productFootprintPipeline)
    .toArray()[0] ?? {
        total: [],
        porEtapa: []
    };

const productFootprintResult = {
    idProduto: produtoPegada._id,
    nomeProduto: produtoPegada.nome,
    nomeEmpresa:
        empresaProduto.nomeFantasia ??
        empresaProduto.razaoSocial,
    totalCo2e:
        productAggregation.total.length === 0
            ? NumberDecimal("0")
            : productAggregation.total[0].totalKgCO2e,
    unidade: "kgCO2e",
    emissoesPorEtapa: productAggregation.porEtapa.map(item => ({
        tipoEtapa: item._id,
        totalCo2e: item.totalKgCO2e
    }))
};

printResult("Product footprint — PRO-001", productFootprintResult);

/*
 * Supplier ranking
 *
 * Equivalent endpoint:
 * GET /api/fornecedores-carbono/ranking?pageNumber=1&pageSize=10
 */

const supplierRankingPipeline = [
    // Calculate the total and event count for each supplier.
    {
        $group: {
            _id: "$fornecedorId",
            totalKgCO2e: {
                $sum: "$quantidadeEmitidaKgCO2e"
            },
            quantidadeEmissoes: {
                $sum: 1
            }
        }
    },

    // Resolve supplier display information.
    {
        $lookup: {
            from: "fornecedores",
            localField: "_id",
            foreignField: "_id",
            as: "fornecedor"
        }
    },

    // Extract display fields without duplicating ranking rows.
    {
        $set: {
            fornecedorCodigo: {
                $arrayElemAt: [
                    "$fornecedor.codigo",
                    0
                ]
            },
            fornecedorNomeFantasia: {
                $arrayElemAt: [
                    "$fornecedor.nomeFantasia",
                    0
                ]
            },
            fornecedorRazaoSocial: {
                $arrayElemAt: [
                    "$fornecedor.razaoSocial",
                    0
                ]
            }
        }
    },

    // Order totals descending and use ObjectId as a stable tie-breaker.
    {
        $sort: {
            totalKgCO2e: -1,
            _id: 1
        }
    },

    // Return the requested page and total count together.
    {
        $facet: {
            items: [
                {
                    $skip: skip
                },
                {
                    $limit: pageSize
                },
                {
                    $project: {
                        _id: 0,
                        idFornecedor: "$_id",
                        codigoFornecedor: "$fornecedorCodigo",
                        nomeFornecedor: {
                            $ifNull: [
                                "$fornecedorNomeFantasia",
                                "$fornecedorRazaoSocial"
                            ]
                        },
                        totalCo2e: "$totalKgCO2e",
                        quantidadeEmissoes: 1
                    }
                }
            ],

            total: [
                {
                    $count: "totalItems"
                }
            ]
        }
    }
];

const supplierAggregation = targetDb.emissoes_carbono
    .aggregate(supplierRankingPipeline)
    .toArray()[0] ?? {
        items: [],
        total: []
    };

const supplierTotalItems =
    supplierAggregation.total.length === 0
        ? 0
        : supplierAggregation.total[0].totalItems;

const supplierRankingResult = {
    items: supplierAggregation.items,
    totalItems: supplierTotalItems,
    pageNumber,
    pageSize,
    totalPages:
        supplierTotalItems === 0
            ? 0
            : Math.ceil(supplierTotalItems / pageSize)
};

printResult("Supplier ranking — page 1", supplierRankingResult);

/*
 * Company dashboard
 *
 * Equivalent endpoint:
 * GET /api/dashboard-carbono/empresas/{idEmpresa}/resumo
 */

const empresaDashboard = requireDocument(
    "empresas",
    { codigo: "EMP-001" },
    "empresa EMP-001"
);

const companyDashboardPipeline = [
    // Restrict every dashboard facet to the selected company.
    {
        $match: {
            empresaId: empresaDashboard._id
        }
    },

    // Produce every dashboard summary from the same event set.
    {
        $facet: {
            resumo: [
                // Calculate total, count, and distinct products.
                {
                    $group: {
                        _id: null,
                        totalKgCO2e: {
                            $sum: "$quantidadeEmitidaKgCO2e"
                        },
                        quantidadeEmissoes: {
                            $sum: 1
                        },
                        produtos: {
                            $addToSet: "$produtoId"
                        }
                    }
                },

                // Calculate the average across products with emissions.
                {
                    $project: {
                        _id: 0,
                        totalKgCO2e: 1,
                        quantidadeEmissoes: 1,
                        quantidadeProdutos: {
                            $size: "$produtos"
                        },
                        mediaEmissaoPorProduto: {
                            $cond: [
                                {
                                    $eq: [
                                        { $size: "$produtos" },
                                        0
                                    ]
                                },
                                NumberDecimal("0"),
                                {
                                    $divide: [
                                        "$totalKgCO2e",
                                        { $size: "$produtos" }
                                    ]
                                }
                            ]
                        }
                    }
                }
            ],

            porMes: [
                // Group emissions into UTC calendar months.
                {
                    $group: {
                        _id: {
                            ano: {
                                $year: "$dataEmissao"
                            },
                            mes: {
                                $month: "$dataEmissao"
                            }
                        },
                        totalKgCO2e: {
                            $sum: "$quantidadeEmitidaKgCO2e"
                        }
                    }
                },

                // Return months chronologically.
                {
                    $sort: {
                        "_id.ano": 1,
                        "_id.mes": 1
                    }
                }
            ],

            porProduto: [
                // Calculate totals for each product.
                {
                    $group: {
                        _id: "$produtoId",
                        totalKgCO2e: {
                            $sum: "$quantidadeEmitidaKgCO2e"
                        }
                    }
                },

                // The first result is the highest-emitting product.
                {
                    $sort: {
                        totalKgCO2e: -1,
                        _id: 1
                    }
                }
            ],

            porFornecedor: [
                // Calculate totals for each supplier.
                {
                    $group: {
                        _id: "$fornecedorId",
                        totalKgCO2e: {
                            $sum: "$quantidadeEmitidaKgCO2e"
                        }
                    }
                },

                // The first result is the highest-emitting supplier.
                {
                    $sort: {
                        totalKgCO2e: -1,
                        _id: 1
                    }
                }
            ],

            porEscopo: [
                // Group totals using the immutable factor snapshot.
                {
                    $group: {
                        _id: "$fatorAplicado.escopo",
                        totalKgCO2e: {
                            $sum: "$quantidadeEmitidaKgCO2e"
                        }
                    }
                },

                // Keep scope output deterministic.
                {
                    $sort: {
                        _id: 1
                    }
                }
            ]
        }
    }
];

const companyAggregation = targetDb.emissoes_carbono
    .aggregate(companyDashboardPipeline)
    .toArray()[0] ?? {
        resumo: [],
        porMes: [],
        porProduto: [],
        porFornecedor: [],
        porEscopo: []
    };

const resumoEmpresa =
    companyAggregation.resumo.length === 0
        ? {
            totalKgCO2e: NumberDecimal("0"),
            quantidadeEmissoes: 0,
            quantidadeProdutos: 0,
            mediaEmissaoPorProduto: NumberDecimal("0")
        }
        : companyAggregation.resumo[0];

const produtoMaisEmissor =
    companyAggregation.porProduto.length === 0
        ? null
        : targetDb.produtos.findOne({
            _id: companyAggregation.porProduto[0]._id
        });

const fornecedorMaisEmissor =
    companyAggregation.porFornecedor.length === 0
        ? null
        : targetDb.fornecedores.findOne({
            _id: companyAggregation.porFornecedor[0]._id
        });

const companyDashboardResult = {
    idEmpresa: empresaDashboard._id,
    nomeEmpresa:
        empresaDashboard.nomeFantasia ??
        empresaDashboard.razaoSocial,
    totalCo2e: resumoEmpresa.totalKgCO2e,
    unidade: "kgCO2e",
    quantidadeProdutos: resumoEmpresa.quantidadeProdutos,
    quantidadeEmissoes: resumoEmpresa.quantidadeEmissoes,
    mediaEmissaoPorProduto:
        resumoEmpresa.mediaEmissaoPorProduto,
    produtoMaisEmissor:
        produtoMaisEmissor?.nome ?? null,
    fornecedorMaisEmissor:
        fornecedorMaisEmissor === null
            ? null
            : fornecedorMaisEmissor.nomeFantasia ??
              fornecedorMaisEmissor.razaoSocial,
    emissoesPorMes: companyAggregation.porMes.map(item => ({
        ano: item._id.ano,
        mes: item._id.mes,
        periodo:
            `${item._id.ano}-` +
            `${String(item._id.mes).padStart(2, "0")}`,
        totalCo2e: item.totalKgCO2e
    })),
    emissoesPorEscopo: companyAggregation.porEscopo.map(item => ({
        escopo: item._id,
        totalCo2e: item.totalKgCO2e
    }))
};

printResult(
    "Company dashboard — EMP-001",
    companyDashboardResult
);

/*
 * Manual comparison targets for an otherwise clean execution
 * of the deterministic 03-seed.js dataset.
 *
 * Extra API-created emissions can legitimately change these values.
 */

const cleanSeedExpectedValues = {
    productPRO001: {
        totalKgCO2e: "172.9500",
        porCategoria: {
            ENERGIA: "122.5500",
            TRANSPORTE: "50.4000"
        }
    },

    supplierRankingOrder: [
        "FOR-005",
        "FOR-009",
        "FOR-003",
        "FOR-002",
        "FOR-008",
        "FOR-001"
    ],

    companyEMP001: {
        totalKgCO2e: "172.9500",
        quantidadeEmissoes: 2,
        quantidadeProdutos: 1,
        mediaEmissaoPorProduto: "172.9500",
        produtoMaisEmissor: "PRO-001",
        fornecedorMaisEmissor: "FOR-002",
        monthlyTotals: {
            "2026-01": "172.9500"
        }
    }
};

printResult(
    "Clean deterministic seed — manual comparison targets",
    cleanSeedExpectedValues
);

print("");
print("Aggregation demonstrations completed without modifying data.");
