const targetDb = db.getSiblingDB("fiap_carbono");

const SEED_CREATED_AT = ISODate("2026-01-01T00:00:00Z");
const SEED_UPDATED_AT = ISODate("2026-01-01T00:00:00Z");
const SCHEMA_VERSION = NumberInt(1);
const REQUIRED_COLLECTIONS = [
    "empresas",
    "produtos",
    "fornecedores",
    "fatores_emissao",
    "emissoes_carbono"
];

const existingCollections = targetDb
    .getCollectionNames()
    .filter(collectionName => !collectionName.startsWith("system."))
    .sort();
const missingCollections = REQUIRED_COLLECTIONS.filter(
    collectionName => !existingCollections.includes(collectionName)
);
const unexpectedCollections = existingCollections.filter(
    collectionName => !REQUIRED_COLLECTIONS.includes(collectionName)
);

if (missingCollections.length > 0) {
    throw new Error(
        "Run 01-create-collections.js before the seed. " +
        `Missing collections: ${missingCollections.join(", ")}`
    );
}

if (unexpectedCollections.length > 0) {
    throw new Error(
        "The fiap_carbono database must contain exactly the five ESG " +
        `collections. Unexpected collections: ${unexpectedCollections.join(", ")}`
    );
}

function upsertDocument(collectionName, filter, document) {
    const collection = targetDb.getCollection(collectionName);
    const existingDocument = collection.findOne(
        filter,
        { projection: { _id: 1, criadoEm: 1 } }
    );
    const replacement = {
        _id: existingDocument?._id ?? new ObjectId(),
        ...document,
        criadoEm: existingDocument?.criadoEm ?? SEED_CREATED_AT,
        atualizadoEm: SEED_UPDATED_AT,
        schemaVersion: SCHEMA_VERSION
    };
    const result = collection.replaceOne(
        filter,
        replacement,
        { upsert: true }
    );

    print(
        `${collectionName}: matched=${result.matchedCount}, ` +
        `modified=${result.modifiedCount}, ` +
        `upserted=${result.upsertedCount}`
    );
}

function requireDocument(collectionName, filter, description) {
    const document = targetDb
        .getCollection(collectionName)
        .findOne(filter);

    if (!document) {
        throw new Error(`Missing required ${description}`);
    }

    return document;
}

function requireSeedCondition(condition, message) {
    if (!condition) {
        throw new Error(`Seed verification failed: ${message}`);
    }
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

function decimalsAreEqual(firstValue, secondValue) {
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
                    $eq: ["$firstValue", "$secondValue"]
                }
            }
        }
    ]);

    return cursor.next().result;
}

function activityQuantityFromDetails(emissionInput, factor) {
    const activityData = emissionInput.dadosAtividade;

    switch (factor.unidadeBase.toLowerCase()) {
        case "kwh":
            return activityData.consumoKwh;
        case "litro":
            return activityData.consumoLitros;
        case "ton_km":
            return multiplyDecimals(
                activityData.distanciaKm,
                activityData.cargaToneladas
            );
        case "m3":
            return activityData.consumoM3;
        case "kg":
            return activityData.pesoKg ?? activityData.consumoKg;
        default:
            throw new Error(
                `Unsupported factor unit ${factor.unidadeBase} in ${emissionInput.codigo}`
            );
    }
}

const empresas = [
    {
        codigo: "EMP-001",
        razaoSocial: "EcoFoods Brasil S.A.",
        nomeFantasia: "EcoFoods",
        cnpj: "10000000000101",
        setor: "ALIMENTOS",
        ativa: true,
        metasReducao: [
            {
                tipo: "EMISSOES_GEE",
                anoBase: 2025,
                anoMeta: 2030,
                percentualReducao: NumberDecimal("30")
            }
        ],
        governanca: {
            responsavelEsg: "Mariana Silva",
            comiteEsg: true,
            frequenciaAuditoria: "ANUAL",
            relatorioPublico: true,
            canalDenuncias: true
        }
    },

    {
        codigo: "EMP-002",
        razaoSocial: "Verde Têxtil Indústria Ltda.",
        nomeFantasia: "Verde Têxtil",
        cnpj: "10000000000292",
        setor: "TEXTIL",
        ativa: true,
        metasReducao: [
            {
                tipo: "EMISSOES_GEE",
                anoBase: 2024,
                anoMeta: 2032,
                percentualReducao: NumberDecimal("40")
            },
            {
                tipo: "CONSUMO_AGUA",
                anoBase: 2024,
                anoMeta: 2029,
                percentualReducao: NumberDecimal("20")
            }
        ],
        governanca: {
            responsavelEsg: "Carlos Mendes",
            comiteEsg: true,
            frequenciaAuditoria: "SEMESTRAL",
            relatorioPublico: true,
            canalDenuncias: true
        }
    },

    {
        codigo: "EMP-003",
        razaoSocial: "SolarTech Equipamentos S.A.",
        nomeFantasia: "SolarTech",
        cnpj: "10000000000373",
        setor: "ENERGIA_RENOVAVEL",
        ativa: true,
        metasReducao: [
            {
                tipo: "EMISSOES_GEE",
                anoBase: 2025,
                anoMeta: 2035,
                percentualReducao: NumberDecimal("55")
            },
            {
                tipo: "ENERGIA_RENOVAVEL",
                anoBase: 2025,
                anoMeta: 2030,
                percentualReducao: NumberDecimal("100")
            }
        ],
        governanca: {
            responsavelEsg: "Fernanda Rocha",
            comiteEsg: true,
            frequenciaAuditoria: "ANUAL",
            relatorioPublico: true,
            canalDenuncias: true,
            conselhoSupervisao: true
        }
    },

    {
        codigo: "EMP-004",
        razaoSocial: "CircularPack Embalagens Ltda.",
        nomeFantasia: "CircularPack",
        cnpj: "10000000000454",
        setor: "EMBALAGENS",
        ativa: true,
        metasReducao: [
            {
                tipo: "EMISSOES_GEE",
                anoBase: 2023,
                anoMeta: 2030,
                percentualReducao: NumberDecimal("35")
            },
            {
                tipo: "RESIDUOS",
                anoBase: 2023,
                anoMeta: 2028,
                percentualReducao: NumberDecimal("50")
            }
        ],
        governanca: {
            responsavelEsg: "João Ferreira",
            comiteEsg: true,
            frequenciaAuditoria: "ANUAL",
            relatorioPublico: false,
            canalDenuncias: true
        }
    },

    {
        codigo: "EMP-005",
        razaoSocial: "Água Pura Bebidas S.A.",
        nomeFantasia: "Água Pura",
        cnpj: "10000000000535",
        setor: "BEBIDAS",
        ativa: true,
        metasReducao: [
            {
                tipo: "EMISSOES_GEE",
                anoBase: 2024,
                anoMeta: 2029,
                percentualReducao: NumberDecimal("25")
            },
            {
                tipo: "CONSUMO_AGUA",
                anoBase: 2024,
                anoMeta: 2028,
                percentualReducao: NumberDecimal("30")
            }
        ],
        governanca: {
            responsavelEsg: "Patrícia Almeida",
            comiteEsg: true,
            frequenciaAuditoria: "SEMESTRAL",
            relatorioPublico: true,
            canalDenuncias: true
        }
    },

    {
        codigo: "EMP-006",
        razaoSocial: "BioConstrução Materiais Sustentáveis Ltda.",
        nomeFantasia: "BioConstrução",
        cnpj: "10000000000616",
        setor: "CONSTRUCAO_CIVIL",
        ativa: true,
        metasReducao: [
            {
                tipo: "EMISSOES_GEE",
                anoBase: 2025,
                anoMeta: 2035,
                percentualReducao: NumberDecimal("45")
            },
            {
                tipo: "MATERIA_PRIMA_RECICLADA",
                anoBase: 2025,
                anoMeta: 2030,
                percentualReducao: NumberDecimal("60")
            }
        ],
        governanca: {
            responsavelEsg: "Ricardo Nascimento",
            comiteEsg: false,
            frequenciaAuditoria: "BIENAL",
            relatorioPublico: false,
            canalDenuncias: true
        }
    },

    {
        codigo: "EMP-007",
        razaoSocial: "Mobilidade Limpa Transportes S.A.",
        nomeFantasia: "Mobilidade Limpa",
        cnpj: "10000000000705",
        setor: "TRANSPORTE",
        ativa: true,
        metasReducao: [
            {
                tipo: "EMISSOES_GEE",
                anoBase: 2024,
                anoMeta: 2033,
                percentualReducao: NumberDecimal("50")
            },
            {
                tipo: "FROTA_BAIXO_CARBONO",
                anoBase: 2024,
                anoMeta: 2030,
                percentualReducao: NumberDecimal("70")
            }
        ],
        governanca: {
            responsavelEsg: "Luciana Costa",
            comiteEsg: true,
            frequenciaAuditoria: "TRIMESTRAL",
            relatorioPublico: true,
            canalDenuncias: true,
            conselhoSupervisao: true
        }
    },

    {
        codigo: "EMP-008",
        razaoSocial: "AgroRaiz Produção Orgânica Ltda.",
        nomeFantasia: "AgroRaiz",
        cnpj: "10000000000896",
        setor: "AGRONEGOCIO",
        ativa: true,
        metasReducao: [
            {
                tipo: "EMISSOES_GEE",
                anoBase: 2023,
                anoMeta: 2028,
                percentualReducao: NumberDecimal("20")
            },
            {
                tipo: "USO_FERTILIZANTES",
                anoBase: 2023,
                anoMeta: 2028,
                percentualReducao: NumberDecimal("15")
            }
        ],
        governanca: {
            responsavelEsg: "Roberto Martins",
            comiteEsg: false,
            frequenciaAuditoria: "ANUAL",
            relatorioPublico: false,
            canalDenuncias: false
        }
    },

    {
        codigo: "EMP-009",
        razaoSocial: "ReTech Eletrônicos Circulares S.A.",
        nomeFantasia: "ReTech",
        cnpj: "10000000000977",
        setor: "ELETRONICOS",
        ativa: true,
        metasReducao: [
            {
                tipo: "EMISSOES_GEE",
                anoBase: 2025,
                anoMeta: 2040,
                percentualReducao: NumberDecimal("60")
            },
            {
                tipo: "RECUPERACAO_PRODUTOS",
                anoBase: 2025,
                anoMeta: 2032,
                percentualReducao: NumberDecimal("80")
            }
        ],
        governanca: {
            responsavelEsg: "Amanda Oliveira",
            comiteEsg: true,
            frequenciaAuditoria: "SEMESTRAL",
            relatorioPublico: true,
            canalDenuncias: true,
            conselhoSupervisao: true
        }
    },

    {
        codigo: "EMP-010",
        razaoSocial: "Saúde Viva Produtos Hospitalares Ltda.",
        nomeFantasia: "Saúde Viva",
        cnpj: "10000000001000",
        setor: "SAUDE",
        ativa: true,
        metasReducao: [
            {
                tipo: "EMISSOES_GEE",
                anoBase: 2025,
                anoMeta: 2031,
                percentualReducao: NumberDecimal("28")
            },
            {
                tipo: "RESIDUOS_HOSPITALARES",
                anoBase: 2025,
                anoMeta: 2030,
                percentualReducao: NumberDecimal("35")
            }
        ],
        governanca: {
            responsavelEsg: "Eduardo Lima",
            comiteEsg: true,
            frequenciaAuditoria: "ANUAL",
            relatorioPublico: true,
            canalDenuncias: true
        }
    }
];

empresas.forEach(empresa => {
    upsertDocument(
        "empresas",
        { codigo: empresa.codigo },
        empresa
    );
});

const fornecedores = [
    {
        codigo: "FOR-001",
        razaoSocial: "Transportes Verdes Ltda.",
        nomeFantasia: "Transportes Verdes",
        cnpj: "20000000000101",
        ativo: true,
        categoriasAtuacao: ["TRANSPORTE", "LOGISTICA"],
        certificacoes: [
            {
                nome: "ISO 14001",
                emissor: "Organismo Certificador Ambiental",
                validade: ISODate("2028-12-31T00:00:00Z")
            }
        ],
        indicadoresSociais: {
            acidentesUltimos12Meses: 0,
            percentualMulheresLideranca: NumberDecimal("42"),
            possuiProgramaDiversidade: true
        },
        conformidadeAmbiental: {
            possuiLicenca: true,
            ocorrenciasUltimos12Meses: 0,
            descarteMonitorado: true
        },
        auditoriaEsg: {
            status: "APROVADO",
            nivelRisco: "BAIXO",
            dataUltimaAuditoria: ISODate("2026-01-15T00:00:00Z")
        }
    },

    {
        codigo: "FOR-002",
        razaoSocial: "Energia Solar Paulista S.A.",
        nomeFantasia: "Solar Paulista",
        cnpj: "20000000000292",
        ativo: true,
        categoriasAtuacao: ["ENERGIA_RENOVAVEL"],
        certificacoes: [
            {
                nome: "ISO 14001",
                emissor: "Organismo Certificador Ambiental",
                validade: ISODate("2029-06-30T00:00:00Z")
            },
            {
                nome: "I-REC",
                emissor: "International REC Standard",
                validade: ISODate("2027-12-31T00:00:00Z")
            }
        ],
        indicadoresSociais: {
            acidentesUltimos12Meses: 1,
            percentualMulheresLideranca: NumberDecimal("38"),
            possuiProgramaDiversidade: true
        },
        conformidadeAmbiental: {
            possuiLicenca: true,
            ocorrenciasUltimos12Meses: 0,
            energiaRenovavel: true
        },
        auditoriaEsg: {
            status: "APROVADO",
            nivelRisco: "BAIXO",
            dataUltimaAuditoria: ISODate("2026-02-10T00:00:00Z")
        }
    },

    {
        codigo: "FOR-003",
        razaoSocial: "Recicla Materiais Industriais Ltda.",
        nomeFantasia: "Recicla Materiais",
        cnpj: "20000000000373",
        ativo: true,
        categoriasAtuacao: ["RECICLAGEM", "MATERIA_PRIMA"],
        certificacoes: [
            {
                nome: "Selo Verde",
                emissor: "Instituto de Gestão Ambiental",
                validade: ISODate("2027-10-31T00:00:00Z")
            }
        ],
        indicadoresSociais: {
            acidentesUltimos12Meses: 2,
            percentualMulheresLideranca: NumberDecimal("35"),
            possuiProgramaDiversidade: true
        },
        conformidadeAmbiental: {
            possuiLicenca: true,
            ocorrenciasUltimos12Meses: 1,
            percentualMaterialReciclado: NumberDecimal("85")
        },
        auditoriaEsg: {
            status: "APROVADO_COM_RESSALVAS",
            nivelRisco: "MEDIO",
            dataUltimaAuditoria: ISODate("2026-03-05T00:00:00Z")
        }
    },

    {
        codigo: "FOR-004",
        razaoSocial: "Agro Sustentável Insumos Ltda.",
        nomeFantasia: "Agro Sustentável",
        cnpj: "20000000000454",
        ativo: true,
        categoriasAtuacao: ["AGRONEGOCIO", "MATERIA_PRIMA"],
        certificacoes: [
            {
                nome: "Produto Orgânico Brasil",
                emissor: "Organismo de Avaliação da Conformidade",
                validade: ISODate("2028-04-30T00:00:00Z")
            }
        ],
        indicadoresSociais: {
            acidentesUltimos12Meses: 0,
            percentualMulheresLideranca: NumberDecimal("30"),
            possuiProgramaDiversidade: false
        },
        conformidadeAmbiental: {
            possuiLicenca: true,
            ocorrenciasUltimos12Meses: 0,
            utilizaDefensivosControlados: true
        },
        auditoriaEsg: {
            status: "APROVADO",
            nivelRisco: "BAIXO",
            dataUltimaAuditoria: ISODate("2026-01-28T00:00:00Z")
        }
    },

    {
        codigo: "FOR-005",
        razaoSocial: "BioQuímica Industrial S.A.",
        nomeFantasia: "BioQuímica Industrial",
        cnpj: "20000000000535",
        ativo: true,
        categoriasAtuacao: ["INSUMOS_INDUSTRIAIS"],
        certificacoes: [
            {
                nome: "ISO 9001",
                emissor: "Organismo Certificador de Qualidade",
                validade: ISODate("2028-08-31T00:00:00Z")
            },
            {
                nome: "ISO 45001",
                emissor: "Organismo Certificador de Segurança",
                validade: ISODate("2027-11-30T00:00:00Z")
            }
        ],
        indicadoresSociais: {
            acidentesUltimos12Meses: 3,
            percentualMulheresLideranca: NumberDecimal("27"),
            possuiProgramaDiversidade: true
        },
        conformidadeAmbiental: {
            possuiLicenca: true,
            ocorrenciasUltimos12Meses: 1,
            controlaEfluentes: true
        },
        auditoriaEsg: {
            status: "APROVADO_COM_RESSALVAS",
            nivelRisco: "MEDIO",
            dataUltimaAuditoria: ISODate("2026-04-12T00:00:00Z")
        }
    },

    {
        codigo: "FOR-006",
        razaoSocial: "Papel Circular Embalagens Ltda.",
        nomeFantasia: "Papel Circular",
        cnpj: "20000000000616",
        ativo: true,
        categoriasAtuacao: ["PAPEL", "EMBALAGEM"],
        certificacoes: [
            {
                nome: "FSC",
                emissor: "Forest Stewardship Council",
                validade: ISODate("2029-03-31T00:00:00Z")
            }
        ],
        indicadoresSociais: {
            acidentesUltimos12Meses: 0,
            percentualMulheresLideranca: NumberDecimal("46"),
            possuiProgramaDiversidade: true
        },
        conformidadeAmbiental: {
            possuiLicenca: true,
            ocorrenciasUltimos12Meses: 0,
            percentualMaterialReciclado: NumberDecimal("90")
        },
        auditoriaEsg: {
            status: "APROVADO",
            nivelRisco: "BAIXO",
            dataUltimaAuditoria: ISODate("2026-02-18T00:00:00Z")
        }
    },

    {
        codigo: "FOR-007",
        razaoSocial: "Logística Rápida Nacional Ltda.",
        nomeFantasia: "Logística Rápida",
        cnpj: "20000000000705",
        ativo: true,
        categoriasAtuacao: ["TRANSPORTE", "LOGISTICA"],
        certificacoes: [],
        indicadoresSociais: {
            acidentesUltimos12Meses: 5,
            percentualMulheresLideranca: NumberDecimal("18"),
            possuiProgramaDiversidade: false
        },
        conformidadeAmbiental: {
            possuiLicenca: true,
            ocorrenciasUltimos12Meses: 2,
            frotaBaixoCarbono: false
        },
        auditoriaEsg: {
            status: "PENDENTE_ADEQUACAO",
            nivelRisco: "ALTO",
            dataUltimaAuditoria: ISODate("2026-05-20T00:00:00Z")
        }
    },

    {
        codigo: "FOR-008",
        razaoSocial: "Tratamento Verde de Resíduos S.A.",
        nomeFantasia: "Tratamento Verde",
        cnpj: "20000000000896",
        ativo: true,
        categoriasAtuacao: ["RESIDUO", "RECICLAGEM"],
        certificacoes: [
            {
                nome: "ISO 14001",
                emissor: "Organismo Certificador Ambiental",
                validade: ISODate("2028-09-30T00:00:00Z")
            },
            {
                nome: "Certificação Lixo Zero",
                emissor: "Instituto Lixo Zero Brasil",
                validade: ISODate("2027-12-31T00:00:00Z")
            }
        ],
        indicadoresSociais: {
            acidentesUltimos12Meses: 1,
            percentualMulheresLideranca: NumberDecimal("50"),
            possuiProgramaDiversidade: true
        },
        conformidadeAmbiental: {
            possuiLicenca: true,
            ocorrenciasUltimos12Meses: 0,
            rastreiaDestinacaoFinal: true
        },
        auditoriaEsg: {
            status: "APROVADO",
            nivelRisco: "BAIXO",
            dataUltimaAuditoria: ISODate("2026-03-22T00:00:00Z")
        }
    },

    {
        codigo: "FOR-009",
        razaoSocial: "Metais do Futuro S.A.",
        nomeFantasia: "Metais do Futuro",
        cnpj: "20000000000977",
        ativo: true,
        categoriasAtuacao: ["METAIS", "MATERIA_PRIMA"],
        certificacoes: [
            {
                nome: "ISO 9001",
                emissor: "Organismo Certificador de Qualidade",
                validade: ISODate("2027-07-31T00:00:00Z")
            }
        ],
        indicadoresSociais: {
            acidentesUltimos12Meses: 4,
            percentualMulheresLideranca: NumberDecimal("22"),
            possuiProgramaDiversidade: false
        },
        conformidadeAmbiental: {
            possuiLicenca: true,
            ocorrenciasUltimos12Meses: 3,
            percentualMaterialReciclado: NumberDecimal("40")
        },
        auditoriaEsg: {
            status: "APROVADO_COM_RESSALVAS",
            nivelRisco: "ALTO",
            dataUltimaAuditoria: ISODate("2026-04-30T00:00:00Z")
        }
    },

    {
        codigo: "FOR-010",
        razaoSocial: "Água Limpa Saneamento Ltda.",
        nomeFantasia: "Água Limpa",
        cnpj: "20000000001000",
        ativo: false,
        categoriasAtuacao: ["SANEAMENTO"],
        certificacoes: [
            {
                nome: "ISO 14001",
                emissor: "Organismo Certificador Ambiental",
                validade: ISODate("2026-12-31T00:00:00Z")
            }
        ],
        indicadoresSociais: {
            acidentesUltimos12Meses: 2,
            percentualMulheresLideranca: NumberDecimal("33"),
            possuiProgramaDiversidade: true
        },
        conformidadeAmbiental: {
            possuiLicenca: false,
            ocorrenciasUltimos12Meses: 2,
            controlaEfluentes: true
        },
        auditoriaEsg: {
            status: "SUSPENSO",
            nivelRisco: "ALTO",
            dataUltimaAuditoria: ISODate("2026-06-05T00:00:00Z")
        }
    }
];

fornecedores.forEach(fornecedorInput => {
    const {
        auditoriaEsg,
        certificacoes,
        ...fornecedor
    } = fornecedorInput;
    const documento = {
        ...fornecedor,
        certificacoes: certificacoes.map(certificacao => {
            const {
                validade,
                ...dadosCertificacao
            } = certificacao;

            return {
                ...dadosCertificacao,
                validaAte: validade
            };
        }),
        conformidadeAmbiental: {
            ...fornecedor.conformidadeAmbiental,
            dataUltimaAuditoria: auditoriaEsg.dataUltimaAuditoria
        },
        statusAuditoria: auditoriaEsg.status,
        nivelRiscoEsg: auditoriaEsg.nivelRisco
    };

    upsertDocument(
        "fornecedores",
        { codigo: documento.codigo },
        documento
    );
});

const fatoresEmissao = [
    {
        codigo: "FE-ENERGIA-001",
        nome: "Energia elétrica — rede nacional",
        categoria: "ENERGIA",
        valor: NumberDecimal("0.0817"),
        unidadeBase: "kWh",
        escopo: "ESCOPO_2",
        versao: 1,
        fonteReferencia:
            "Dataset demonstrativo FIAP — energia elétrica",
        metodologia:
            "GHG Protocol — consumo de energia multiplicado pelo fator",
        validadeInicio: ISODate("2026-01-01T00:00:00Z"),
        validadeFim: ISODate("2026-12-31T23:59:59Z"),
        ativo: true
    },

    {
        codigo: "FE-DIESEL-001",
        nome: "Combustão de óleo diesel",
        categoria: "COMBUSTIVEL",
        valor: NumberDecimal("2.6800"),
        unidadeBase: "litro",
        escopo: "ESCOPO_1",
        versao: 1,
        fonteReferencia:
            "Dataset demonstrativo FIAP — combustíveis",
        metodologia:
            "GHG Protocol — combustível consumido multiplicado pelo fator",
        validadeInicio: ISODate("2026-01-01T00:00:00Z"),
        validadeFim: ISODate("2026-12-31T23:59:59Z"),
        ativo: true
    },

    {
        codigo: "FE-TRANSPORTE-001",
        nome: "Transporte rodoviário de carga",
        categoria: "TRANSPORTE",
        valor: NumberDecimal("0.1200"),
        unidadeBase: "ton_km",
        escopo: "ESCOPO_3",
        versao: 1,
        fonteReferencia:
            "Dataset demonstrativo FIAP — transporte de carga",
        metodologia:
            "Distância percorrida multiplicada pela carga transportada",
        validadeInicio: ISODate("2026-01-01T00:00:00Z"),
        validadeFim: ISODate("2026-12-31T23:59:59Z"),
        ativo: true
    },

    {
        codigo: "FE-GAS-NATURAL-001",
        nome: "Combustão de gás natural",
        categoria: "COMBUSTIVEL",
        valor: NumberDecimal("2.0300"),
        unidadeBase: "m3",
        escopo: "ESCOPO_1",
        versao: 1,
        fonteReferencia:
            "Dataset demonstrativo FIAP — combustíveis gasosos",
        metodologia:
            "GHG Protocol — volume consumido multiplicado pelo fator",
        validadeInicio: ISODate("2026-01-01T00:00:00Z"),
        validadeFim: ISODate("2026-12-31T23:59:59Z"),
        ativo: true
    },

    {
        codigo: "FE-GLP-001",
        nome: "Combustão de gás liquefeito de petróleo",
        categoria: "COMBUSTIVEL",
        valor: NumberDecimal("1.6100"),
        unidadeBase: "kg",
        escopo: "ESCOPO_1",
        versao: 1,
        fonteReferencia:
            "Dataset demonstrativo FIAP — combustíveis gasosos",
        metodologia:
            "GHG Protocol — massa consumida multiplicada pelo fator",
        validadeInicio: ISODate("2026-01-01T00:00:00Z"),
        validadeFim: ISODate("2026-12-31T23:59:59Z"),
        ativo: true
    },

    {
        codigo: "FE-ALUMINIO-VIRGEM-001",
        nome: "Produção de alumínio virgem",
        categoria: "MATERIA_PRIMA",
        valor: NumberDecimal("8.2400"),
        unidadeBase: "kg",
        escopo: "ESCOPO_3",
        versao: 1,
        fonteReferencia:
            "Dataset demonstrativo FIAP — matérias-primas",
        metodologia:
            "Avaliação simplificada do ciclo de vida do material",
        validadeInicio: ISODate("2026-01-01T00:00:00Z"),
        validadeFim: ISODate("2026-12-31T23:59:59Z"),
        ativo: true
    },

    {
        codigo: "FE-ALUMINIO-RECICLADO-001",
        nome: "Produção de alumínio reciclado",
        categoria: "MATERIA_PRIMA",
        valor: NumberDecimal("0.7200"),
        unidadeBase: "kg",
        escopo: "ESCOPO_3",
        versao: 1,
        fonteReferencia:
            "Dataset demonstrativo FIAP — matérias-primas recicladas",
        metodologia:
            "Avaliação simplificada do ciclo de vida do material",
        validadeInicio: ISODate("2026-01-01T00:00:00Z"),
        validadeFim: ISODate("2026-12-31T23:59:59Z"),
        ativo: true
    },

    {
        codigo: "FE-PLASTICO-PET-001",
        nome: "Produção de plástico PET virgem",
        categoria: "MATERIA_PRIMA",
        valor: NumberDecimal("2.7300"),
        unidadeBase: "kg",
        escopo: "ESCOPO_3",
        versao: 1,
        fonteReferencia:
            "Dataset demonstrativo FIAP — matérias-primas plásticas",
        metodologia:
            "Avaliação simplificada do ciclo de vida do material",
        validadeInicio: ISODate("2026-01-01T00:00:00Z"),
        validadeFim: ISODate("2026-12-31T23:59:59Z"),
        ativo: true
    },

    {
        codigo: "FE-RESIDUO-ATERRO-001",
        nome: "Destinação de resíduos para aterro",
        categoria: "RESIDUO",
        valor: NumberDecimal("0.4500"),
        unidadeBase: "kg",
        escopo: "ESCOPO_3",
        versao: 1,
        fonteReferencia:
            "Dataset demonstrativo FIAP — gestão de resíduos",
        metodologia:
            "Massa destinada ao aterro multiplicada pelo fator",
        validadeInicio: ISODate("2026-01-01T00:00:00Z"),
        validadeFim: ISODate("2026-12-31T23:59:59Z"),
        ativo: true
    },

    {
        codigo: "FE-ENERGIA-001",
        nome: "Energia elétrica — rede nacional",
        categoria: "ENERGIA",
        valor: NumberDecimal("0.0750"),
        unidadeBase: "kWh",
        escopo: "ESCOPO_2",
        versao: 2,
        fonteReferencia:
            "Dataset demonstrativo FIAP — energia elétrica",
        metodologia:
            "GHG Protocol — consumo de energia multiplicado pelo fator",
        validadeInicio: ISODate("2027-01-01T00:00:00Z"),
        validadeFim: ISODate("2027-12-31T23:59:59Z"),
        ativo: false
    }
];

fatoresEmissao.forEach(fatorInput => {
    const {
        validadeInicio,
        validadeFim,
        ...fator
    } = fatorInput;
    const documento = {
        ...fator,
        versao: NumberInt(fator.versao),
        validoDe: validadeInicio,
        validoAte: validadeFim
    };

    upsertDocument(
        "fatores_emissao",
        {
            codigo: documento.codigo,
            versao: documento.versao
        },
        documento
    );
});

const produtos = [
    {
        empresaCodigo: "EMP-001",
        codigo: "PRO-001",
        nome: "Barra de Cereal Sustentável",
        unidadeFuncional: "unidade",
        ativo: true,
        atributosAmbientais: {
            percentualReciclavel: NumberDecimal("85"),
            embalagem: "PAPEL_RECICLADO",
            seloSustentabilidade: "PRODUTO_VERDE"
        },
        materiais: [
            {
                nome: "Aveia",
                percentualComposicao: NumberDecimal("55"),
                origemRenovavel: true
            },
            {
                nome: "Frutas desidratadas",
                percentualComposicao: NumberDecimal("25"),
                origemRenovavel: true
            },
            {
                nome: "Açúcar orgânico",
                percentualComposicao: NumberDecimal("15"),
                origemRenovavel: true
            },
            {
                nome: "Embalagem de papel",
                percentualComposicao: NumberDecimal("5"),
                origemReciclada: true
            }
        ]
    },

    {
        empresaCodigo: "EMP-002",
        codigo: "PRO-002",
        nome: "Camiseta de Algodão Reciclado",
        unidadeFuncional: "camiseta",
        ativo: true,
        atributosAmbientais: {
            percentualReciclavel: NumberDecimal("90"),
            percentualMaterialReciclado: NumberDecimal("65"),
            embalagem: "PAPEL_SEM_PLASTICO",
            seloSustentabilidade: "MODA_CIRCULAR"
        },
        materiais: [
            {
                nome: "Algodão reciclado",
                percentualComposicao: NumberDecimal("65"),
                origemReciclada: true
            },
            {
                nome: "Algodão orgânico",
                percentualComposicao: NumberDecimal("30"),
                origemRenovavel: true
            },
            {
                nome: "Tinta à base de água",
                percentualComposicao: NumberDecimal("5"),
                origemRenovavel: false
            }
        ]
    },

    {
        empresaCodigo: "EMP-003",
        codigo: "PRO-003",
        nome: "Painel Solar Residencial 450 W",
        unidadeFuncional: "painel",
        ativo: true,
        atributosAmbientais: {
            percentualReciclavel: NumberDecimal("92"),
            vidaUtilAnos: 25,
            embalagem: "PAPELAO_RECICLADO",
            seloSustentabilidade: "ENERGIA_LIMPA"
        },
        materiais: [
            {
                nome: "Vidro",
                percentualComposicao: NumberDecimal("70"),
                origemReciclada: false
            },
            {
                nome: "Alumínio reciclado",
                percentualComposicao: NumberDecimal("15"),
                origemReciclada: true
            },
            {
                nome: "Silício",
                percentualComposicao: NumberDecimal("10"),
                origemReciclada: false
            },
            {
                nome: "Componentes elétricos",
                percentualComposicao: NumberDecimal("5"),
                origemReciclada: false
            }
        ]
    },

    {
        empresaCodigo: "EMP-004",
        codigo: "PRO-004",
        nome: "Caixa de Papelão Circular",
        unidadeFuncional: "caixa",
        ativo: true,
        atributosAmbientais: {
            percentualReciclavel: NumberDecimal("100"),
            percentualMaterialReciclado: NumberDecimal("90"),
            embalagem: "SEM_EMBALAGEM_ADICIONAL",
            seloSustentabilidade: "FSC_RECICLADO"
        },
        materiais: [
            {
                nome: "Papelão reciclado",
                percentualComposicao: NumberDecimal("90"),
                origemReciclada: true
            },
            {
                nome: "Fibra de celulose certificada",
                percentualComposicao: NumberDecimal("8"),
                origemRenovavel: true
            },
            {
                nome: "Cola à base de água",
                percentualComposicao: NumberDecimal("2"),
                origemRenovavel: true
            }
        ]
    },

    {
        empresaCodigo: "EMP-005",
        codigo: "PRO-005",
        nome: "Água Mineral em Garrafa Reciclada",
        unidadeFuncional: "garrafa_500ml",
        ativo: true,
        atributosAmbientais: {
            percentualReciclavel: NumberDecimal("100"),
            percentualMaterialReciclado: NumberDecimal("50"),
            embalagem: "PET_RECICLADO",
            seloSustentabilidade: "EMBALAGEM_CIRCULAR"
        },
        materiais: [
            {
                nome: "Água mineral",
                percentualComposicao: NumberDecimal("95"),
                origemRenovavel: true
            },
            {
                nome: "PET reciclado",
                percentualComposicao: NumberDecimal("4"),
                origemReciclada: true
            },
            {
                nome: "Polietileno da tampa",
                percentualComposicao: NumberDecimal("1"),
                origemReciclada: false
            }
        ]
    },

    {
        empresaCodigo: "EMP-006",
        codigo: "PRO-006",
        nome: "Bloco de Construção Ecológico",
        unidadeFuncional: "bloco",
        ativo: true,
        atributosAmbientais: {
            percentualReciclavel: NumberDecimal("95"),
            percentualMaterialReciclado: NumberDecimal("60"),
            embalagem: "PALETE_REUTILIZAVEL",
            seloSustentabilidade: "CONSTRUCAO_SUSTENTAVEL"
        },
        materiais: [
            {
                nome: "Agregado reciclado",
                percentualComposicao: NumberDecimal("60"),
                origemReciclada: true
            },
            {
                nome: "Cimento de baixo carbono",
                percentualComposicao: NumberDecimal("25"),
                origemReciclada: false
            },
            {
                nome: "Areia",
                percentualComposicao: NumberDecimal("10"),
                origemRenovavel: false
            },
            {
                nome: "Água de reuso",
                percentualComposicao: NumberDecimal("5"),
                origemReciclada: true
            }
        ]
    },

    {
        empresaCodigo: "EMP-007",
        codigo: "PRO-007",
        nome: "Transporte Rodoviário de Baixo Carbono",
        unidadeFuncional: "ton_km",
        ativo: true,
        atributosAmbientais: {
            percentualFrotaEletrificada: NumberDecimal("35"),
            percentualBiocombustivel: NumberDecimal("45"),
            compensacaoCarbono: true,
            seloSustentabilidade: "LOGISTICA_VERDE"
        },
        materiais: []
    },

    {
        empresaCodigo: "EMP-008",
        codigo: "PRO-008",
        nome: "Café Orgânico Torrado",
        unidadeFuncional: "pacote_1kg",
        ativo: true,
        atributosAmbientais: {
            percentualReciclavel: NumberDecimal("80"),
            origemAgriculturaOrganica: true,
            embalagem: "PAPEL_COMPOSTAVEL",
            seloSustentabilidade: "PRODUTO_ORGANICO"
        },
        materiais: [
            {
                nome: "Café orgânico",
                percentualComposicao: NumberDecimal("98"),
                origemRenovavel: true
            },
            {
                nome: "Embalagem compostável",
                percentualComposicao: NumberDecimal("2"),
                origemRenovavel: true
            }
        ]
    },

    {
        empresaCodigo: "EMP-009",
        codigo: "PRO-009",
        nome: "Notebook Remanufaturado",
        unidadeFuncional: "unidade_remanufaturada",
        ativo: true,
        atributosAmbientais: {
            percentualReciclavel: NumberDecimal("85"),
            percentualComponentesReutilizados: NumberDecimal("70"),
            garantiaMeses: 24,
            embalagem: "PAPELAO_RECICLADO",
            seloSustentabilidade: "ELETRONICO_CIRCULAR"
        },
        materiais: [
            {
                nome: "Componentes eletrônicos reutilizados",
                percentualComposicao: NumberDecimal("70"),
                origemReciclada: true
            },
            {
                nome: "Alumínio reciclado",
                percentualComposicao: NumberDecimal("15"),
                origemReciclada: true
            },
            {
                nome: "Plástico reciclado",
                percentualComposicao: NumberDecimal("10"),
                origemReciclada: true
            },
            {
                nome: "Componentes novos",
                percentualComposicao: NumberDecimal("5"),
                origemReciclada: false
            }
        ]
    },

    {
        empresaCodigo: "EMP-010",
        codigo: "PRO-010",
        nome: "Kit Hospitalar de Baixo Impacto",
        unidadeFuncional: "kit",
        ativo: true,
        atributosAmbientais: {
            percentualReciclavel: NumberDecimal("75"),
            percentualMaterialReciclado: NumberDecimal("40"),
            esterilizacaoBaixoConsumo: true,
            embalagem: "PAPEL_GRAU_CIRURGICO_RECICLAVEL",
            seloSustentabilidade: "SAUDE_SUSTENTAVEL"
        },
        materiais: [
            {
                nome: "Tecido não tecido reciclável",
                percentualComposicao: NumberDecimal("50"),
                origemReciclada: false
            },
            {
                nome: "Plástico reciclado",
                percentualComposicao: NumberDecimal("25"),
                origemReciclada: true
            },
            {
                nome: "Papel grau cirúrgico",
                percentualComposicao: NumberDecimal("20"),
                origemRenovavel: true
            },
            {
                nome: "Componentes metálicos",
                percentualComposicao: NumberDecimal("5"),
                origemReciclada: true
            }
        ]
    }
];

produtos.forEach(produtoInput => {
    const empresa = requireDocument(
        "empresas",
        { codigo: produtoInput.empresaCodigo },
        `empresa ${produtoInput.empresaCodigo}`
    );

    const {
        empresaCodigo,
        materiais,
        ...produto
    } = produtoInput;

    upsertDocument(
        "produtos",
        {
            empresaId: empresa._id,
            codigo: produto.codigo
        },
        {
            ...produto,
            empresaId: empresa._id,
            atributosAmbientais: {
                ...produto.atributosAmbientais,
                materiais
            }
        }
    );
});

const emissoes = [
    {
        codigo: "EMI-001",
        empresaCodigo: "EMP-001",
        produtoCodigo: "PRO-001",
        fornecedorCodigo: "FOR-002",
        fatorCodigo: "FE-ENERGIA-001",
        fatorVersao: 1,

        lote: {
            codigo: "LOTE-2026-001",
            quantidadeProduzida: NumberDecimal("10000"),
            unidade: "unidades",
            dataProducao: ISODate("2026-01-10T00:00:00Z")
        },

        etapa: {
            nome: "Consumo de energia na produção",
            ordem: 2,
            categoria: "ENERGIA",
            local: "São Paulo"
        },

        quantidadeAtividade: NumberDecimal("1500"),

        dadosAtividade: {
            tipo: "ENERGIA",
            consumoKwh: NumberDecimal("1500"),
            fonteEnergia: "REDE_NACIONAL",
            percentualRenovavel: NumberDecimal("25")
        },

        fonteEmissao: "Energia elétrica",
        observacao: "Consumo de energia da produção do lote",
        calculadoPor: "admin@carbono.com",
        dataEmissao: ISODate("2026-01-15T12:00:00Z")
    },

    {
        codigo: "EMI-002",
        empresaCodigo: "EMP-001",
        produtoCodigo: "PRO-001",
        fornecedorCodigo: "FOR-001",
        fatorCodigo: "FE-TRANSPORTE-001",
        fatorVersao: 1,

        lote: {
            codigo: "LOTE-2026-001",
            quantidadeProduzida: NumberDecimal("10000"),
            unidade: "unidades",
            dataProducao: ISODate("2026-01-10T00:00:00Z")
        },

        etapa: {
            nome: "Transporte das matérias-primas",
            ordem: 1,
            categoria: "TRANSPORTE",
            local: "São Paulo"
        },

        quantidadeAtividade: NumberDecimal("420"),

        dadosAtividade: {
            tipo: "TRANSPORTE",
            distanciaKm: NumberDecimal("350"),
            cargaToneladas: NumberDecimal("1.2"),
            combustivel: "DIESEL",
            modal: "RODOVIARIO"
        },

        fonteEmissao: "Transporte rodoviário",
        observacao: "Transporte dos ingredientes até a fábrica",
        calculadoPor: "analista@carbono.com",
        dataEmissao: ISODate("2026-01-12T10:00:00Z")
    },

    {
        codigo: "EMI-003",
        empresaCodigo: "EMP-002",
        produtoCodigo: "PRO-002",
        fornecedorCodigo: "FOR-002",
        fatorCodigo: "FE-ENERGIA-001",
        fatorVersao: 1,

        lote: {
            codigo: "LOTE-2026-002",
            quantidadeProduzida: NumberDecimal("5000"),
            unidade: "camisetas",
            dataProducao: ISODate("2026-02-05T00:00:00Z")
        },

        etapa: {
            nome: "Energia utilizada na confecção",
            ordem: 3,
            categoria: "ENERGIA",
            local: "Campinas"
        },

        quantidadeAtividade: NumberDecimal("2200"),

        dadosAtividade: {
            tipo: "ENERGIA",
            consumoKwh: NumberDecimal("2200"),
            fonteEnergia: "REDE_NACIONAL",
            percentualRenovavel: NumberDecimal("35")
        },

        fonteEmissao: "Energia elétrica",
        observacao: "Consumo das máquinas de corte e costura",
        calculadoPor: "analista@carbono.com",
        dataEmissao: ISODate("2026-02-08T14:00:00Z")
    },

    {
        codigo: "EMI-004",
        empresaCodigo: "EMP-002",
        produtoCodigo: "PRO-002",
        fornecedorCodigo: "FOR-008",
        fatorCodigo: "FE-RESIDUO-ATERRO-001",
        fatorVersao: 1,

        lote: {
            codigo: "LOTE-2026-002",
            quantidadeProduzida: NumberDecimal("5000"),
            unidade: "camisetas",
            dataProducao: ISODate("2026-02-05T00:00:00Z")
        },

        etapa: {
            nome: "Destinação de retalhos",
            ordem: 4,
            categoria: "RESIDUO",
            local: "Campinas"
        },

        quantidadeAtividade: NumberDecimal("180"),

        dadosAtividade: {
            tipo: "RESIDUO",
            classe: "NAO_PERIGOSO",
            pesoKg: NumberDecimal("180"),
            tipoResiduo: "RETALHO_TEXTIL",
            tratamento: "ATERRO",
            distanciaDestinoKm: NumberDecimal("25"),
            percentualReciclavel: NumberDecimal("70")
        },

        fonteEmissao: "Resíduos têxteis",
        observacao: "Retalhos não aproveitados no lote",
        calculadoPor: "auditoria@carbono.com",
        dataEmissao: ISODate("2026-02-09T09:30:00Z")
    },

    {
        codigo: "EMI-005",
        empresaCodigo: "EMP-003",
        produtoCodigo: "PRO-003",
        fornecedorCodigo: "FOR-009",
        fatorCodigo: "FE-ALUMINIO-VIRGEM-001",
        fatorVersao: 1,

        lote: {
            codigo: "LOTE-2026-003",
            quantidadeProduzida: NumberDecimal("800"),
            unidade: "paineis",
            dataProducao: ISODate("2026-03-03T00:00:00Z")
        },

        etapa: {
            nome: "Aquisição de alumínio virgem",
            ordem: 1,
            categoria: "MATERIA_PRIMA",
            local: "Sorocaba"
        },

        quantidadeAtividade: NumberDecimal("750"),

        dadosAtividade: {
            tipo: "MATERIA_PRIMA",
            material: "ALUMINIO_VIRGEM",
            pesoKg: NumberDecimal("750"),
            percentualReciclado: NumberDecimal("0"),
            origem: "NACIONAL"
        },

        fonteEmissao: "Alumínio virgem",
        observacao: "Alumínio utilizado nas estruturas dos painéis",
        calculadoPor: "admin@carbono.com",
        dataEmissao: ISODate("2026-03-04T11:00:00Z")
    },

    {
        codigo: "EMI-006",
        empresaCodigo: "EMP-003",
        produtoCodigo: "PRO-003",
        fornecedorCodigo: "FOR-003",
        fatorCodigo: "FE-ALUMINIO-RECICLADO-001",
        fatorVersao: 1,

        lote: {
            codigo: "LOTE-2026-003",
            quantidadeProduzida: NumberDecimal("800"),
            unidade: "paineis",
            dataProducao: ISODate("2026-03-03T00:00:00Z")
        },

        etapa: {
            nome: "Aquisição de alumínio reciclado",
            ordem: 1,
            categoria: "MATERIA_PRIMA",
            local: "Sorocaba"
        },

        quantidadeAtividade: NumberDecimal("300"),

        dadosAtividade: {
            tipo: "MATERIA_PRIMA",
            material: "ALUMINIO_RECICLADO",
            pesoKg: NumberDecimal("300"),
            percentualReciclado: NumberDecimal("100"),
            origem: "NACIONAL"
        },

        fonteEmissao: "Alumínio reciclado",
        observacao: "Material reciclado utilizado nas molduras",
        calculadoPor: "admin@carbono.com",
        dataEmissao: ISODate("2026-03-04T11:30:00Z")
    },

    {
        codigo: "EMI-007",
        empresaCodigo: "EMP-004",
        produtoCodigo: "PRO-004",
        fornecedorCodigo: "FOR-005",
        fatorCodigo: "FE-DIESEL-001",
        fatorVersao: 1,

        lote: {
            codigo: "LOTE-2026-004",
            quantidadeProduzida: NumberDecimal("20000"),
            unidade: "caixas",
            dataProducao: ISODate("2026-04-07T00:00:00Z")
        },

        etapa: {
            nome: "Operação de gerador a diesel",
            ordem: 2,
            categoria: "ENERGIA",
            local: "Jundiaí"
        },

        quantidadeAtividade: NumberDecimal("320"),

        dadosAtividade: {
            tipo: "ENERGIA",
            combustivel: "DIESEL",
            consumoLitros: NumberDecimal("320"),
            equipamento: "GERADOR_INDUSTRIAL"
        },

        fonteEmissao: "Combustão de diesel",
        observacao: "Gerador utilizado durante manutenção elétrica",
        calculadoPor: "analista@carbono.com",
        dataEmissao: ISODate("2026-04-08T08:30:00Z")
    },

    {
        codigo: "EMI-008",
        empresaCodigo: "EMP-004",
        produtoCodigo: "PRO-004",
        fornecedorCodigo: "FOR-008",
        fatorCodigo: "FE-RESIDUO-ATERRO-001",
        fatorVersao: 1,

        lote: {
            codigo: "LOTE-2026-004",
            quantidadeProduzida: NumberDecimal("20000"),
            unidade: "caixas",
            dataProducao: ISODate("2026-04-07T00:00:00Z")
        },

        etapa: {
            nome: "Destinação de aparas de papelão",
            ordem: 3,
            categoria: "RESIDUO",
            local: "Jundiaí"
        },

        quantidadeAtividade: NumberDecimal("500"),

        dadosAtividade: {
            tipo: "RESIDUO",
            classe: "NAO_PERIGOSO",
            pesoKg: NumberDecimal("500"),
            tipoResiduo: "APARAS_PAPELAO",
            tratamento: "ATERRO",
            distanciaDestinoKm: NumberDecimal("18"),
            percentualReciclavel: NumberDecimal("100")
        },

        fonteEmissao: "Resíduos de papelão",
        observacao: "Aparas não recuperadas durante a produção",
        calculadoPor: "auditoria@carbono.com",
        dataEmissao: ISODate("2026-04-08T15:00:00Z")
    },

    {
        codigo: "EMI-009",
        empresaCodigo: "EMP-005",
        produtoCodigo: "PRO-005",
        fornecedorCodigo: "FOR-005",
        fatorCodigo: "FE-PLASTICO-PET-001",
        fatorVersao: 1,

        lote: {
            codigo: "LOTE-2026-005",
            quantidadeProduzida: NumberDecimal("30000"),
            unidade: "garrafas",
            dataProducao: ISODate("2026-05-02T00:00:00Z")
        },

        etapa: {
            nome: "Aquisição de resina PET",
            ordem: 1,
            categoria: "MATERIA_PRIMA",
            local: "Ribeirão Preto"
        },

        quantidadeAtividade: NumberDecimal("900"),

        dadosAtividade: {
            tipo: "MATERIA_PRIMA",
            material: "PLASTICO_PET",
            pesoKg: NumberDecimal("900"),
            percentualReciclado: NumberDecimal("50"),
            origem: "NACIONAL"
        },

        fonteEmissao: "Plástico PET",
        observacao: "Resina utilizada na fabricação das garrafas",
        calculadoPor: "admin@carbono.com",
        dataEmissao: ISODate("2026-05-03T10:00:00Z")
    },

    {
        codigo: "EMI-010",
        empresaCodigo: "EMP-005",
        produtoCodigo: "PRO-005",
        fornecedorCodigo: "FOR-001",
        fatorCodigo: "FE-TRANSPORTE-001",
        fatorVersao: 1,

        lote: {
            codigo: "LOTE-2026-005",
            quantidadeProduzida: NumberDecimal("30000"),
            unidade: "garrafas",
            dataProducao: ISODate("2026-05-02T00:00:00Z")
        },

        etapa: {
            nome: "Transporte das embalagens",
            ordem: 2,
            categoria: "TRANSPORTE",
            local: "Ribeirão Preto"
        },

        quantidadeAtividade: NumberDecimal("600"),

        dadosAtividade: {
            tipo: "TRANSPORTE",
            distanciaKm: NumberDecimal("400"),
            cargaToneladas: NumberDecimal("1.5"),
            combustivel: "DIESEL",
            modal: "RODOVIARIO"
        },

        fonteEmissao: "Transporte rodoviário",
        observacao: "Transporte das embalagens até a fábrica",
        calculadoPor: "analista@carbono.com",
        dataEmissao: ISODate("2026-05-04T13:00:00Z")
    },

    {
        codigo: "EMI-011",
        empresaCodigo: "EMP-006",
        produtoCodigo: "PRO-006",
        fornecedorCodigo: "FOR-005",
        fatorCodigo: "FE-GAS-NATURAL-001",
        fatorVersao: 1,

        lote: {
            codigo: "LOTE-2026-006",
            quantidadeProduzida: NumberDecimal("12000"),
            unidade: "blocos",
            dataProducao: ISODate("2026-06-06T00:00:00Z")
        },

        etapa: {
            nome: "Secagem térmica dos blocos",
            ordem: 3,
            categoria: "ENERGIA",
            local: "Belo Horizonte"
        },

        quantidadeAtividade: NumberDecimal("1250"),

        dadosAtividade: {
            tipo: "ENERGIA",
            combustivel: "GAS_NATURAL",
            consumoM3: NumberDecimal("1250"),
            equipamento: "FORNO_INDUSTRIAL"
        },

        fonteEmissao: "Combustão de gás natural",
        observacao: "Gás natural utilizado na secagem dos blocos",
        calculadoPor: "analista@carbono.com",
        dataEmissao: ISODate("2026-06-07T16:00:00Z")
    },

    {
        codigo: "EMI-012",
        empresaCodigo: "EMP-007",
        produtoCodigo: "PRO-007",
        fornecedorCodigo: "FOR-005",
        fatorCodigo: "FE-DIESEL-001",
        fatorVersao: 1,

        lote: {
            codigo: "OPERACAO-2026-007",
            quantidadeProduzida: NumberDecimal("50"),
            unidade: "operacoes_logisticas",
            dataProducao: ISODate("2026-07-10T00:00:00Z")
        },

        etapa: {
            nome: "Operação da frota a diesel",
            ordem: 1,
            categoria: "TRANSPORTE",
            local: "São Paulo"
        },

        quantidadeAtividade: NumberDecimal("800"),

        dadosAtividade: {
            tipo: "TRANSPORTE",
            combustivel: "DIESEL",
            consumoLitros: NumberDecimal("800"),
            quantidadeVeiculos: 12,
            tipoFrota: "MISTA"
        },

        fonteEmissao: "Combustão de diesel",
        observacao: "Consumo mensal da frota não eletrificada",
        calculadoPor: "admin@carbono.com",
        dataEmissao: ISODate("2026-07-31T18:00:00Z")
    },

    {
        codigo: "EMI-013",
        empresaCodigo: "EMP-008",
        produtoCodigo: "PRO-008",
        fornecedorCodigo: "FOR-002",
        fatorCodigo: "FE-ENERGIA-001",
        fatorVersao: 1,

        lote: {
            codigo: "LOTE-2026-008",
            quantidadeProduzida: NumberDecimal("3000"),
            unidade: "pacotes",
            dataProducao: ISODate("2026-08-12T00:00:00Z")
        },

        etapa: {
            nome: "Torrefação do café",
            ordem: 2,
            categoria: "ENERGIA",
            local: "Minas Gerais"
        },

        quantidadeAtividade: NumberDecimal("1100"),

        dadosAtividade: {
            tipo: "ENERGIA",
            consumoKwh: NumberDecimal("1100"),
            fonteEnergia: "REDE_NACIONAL",
            percentualRenovavel: NumberDecimal("60")
        },

        fonteEmissao: "Energia elétrica",
        observacao: "Energia consumida na torrefação do café",
        calculadoPor: "analista@carbono.com",
        dataEmissao: ISODate("2026-08-13T12:30:00Z")
    },

    {
        codigo: "EMI-014",
        empresaCodigo: "EMP-009",
        produtoCodigo: "PRO-009",
        fornecedorCodigo: "FOR-003",
        fatorCodigo: "FE-ALUMINIO-RECICLADO-001",
        fatorVersao: 1,

        lote: {
            codigo: "LOTE-2026-009",
            quantidadeProduzida: NumberDecimal("600"),
            unidade: "notebooks",
            dataProducao: ISODate("2026-09-05T00:00:00Z")
        },

        etapa: {
            nome: "Aquisição de alumínio reciclado",
            ordem: 1,
            categoria: "MATERIA_PRIMA",
            local: "Curitiba"
        },

        quantidadeAtividade: NumberDecimal("450"),

        dadosAtividade: {
            tipo: "MATERIA_PRIMA",
            material: "ALUMINIO_RECICLADO",
            pesoKg: NumberDecimal("450"),
            percentualReciclado: NumberDecimal("100"),
            origem: "NACIONAL"
        },

        fonteEmissao: "Alumínio reciclado",
        observacao: "Alumínio utilizado nas carcaças remanufaturadas",
        calculadoPor: "admin@carbono.com",
        dataEmissao: ISODate("2026-09-06T09:00:00Z")
    },

    {
        codigo: "EMI-015",
        empresaCodigo: "EMP-010",
        produtoCodigo: "PRO-010",
        fornecedorCodigo: "FOR-005",
        fatorCodigo: "FE-GLP-001",
        fatorVersao: 1,

        lote: {
            codigo: "LOTE-2026-010",
            quantidadeProduzida: NumberDecimal("2500"),
            unidade: "kits",
            dataProducao: ISODate("2026-10-04T00:00:00Z")
        },

        etapa: {
            nome: "Esterilização térmica",
            ordem: 3,
            categoria: "ENERGIA",
            local: "Rio de Janeiro"
        },

        quantidadeAtividade: NumberDecimal("275"),

        dadosAtividade: {
            tipo: "ENERGIA",
            combustivel: "GLP",
            consumoKg: NumberDecimal("275"),
            equipamento: "AUTOCLAVE_INDUSTRIAL"
        },

        fonteEmissao: "Combustão de GLP",
        observacao: "GLP utilizado no processo de esterilização",
        calculadoPor: "auditoria@carbono.com",
        dataEmissao: ISODate("2026-10-05T14:00:00Z")
    }
];

emissoes.forEach(emissaoInput => {
    const empresa = requireDocument(
        "empresas",
        { codigo: emissaoInput.empresaCodigo },
        `empresa ${emissaoInput.empresaCodigo}`
    );

    const produto = requireDocument(
        "produtos",
        {
            empresaId: empresa._id,
            codigo: emissaoInput.produtoCodigo
        },
        `produto ${emissaoInput.produtoCodigo}`
    );

    const fornecedor = requireDocument(
        "fornecedores",
        { codigo: emissaoInput.fornecedorCodigo },
        `fornecedor ${emissaoInput.fornecedorCodigo}`
    );

    const fator = requireDocument(
        "fatores_emissao",
        {
            codigo: emissaoInput.fatorCodigo,
            versao: emissaoInput.fatorVersao
        },
        `fator ${emissaoInput.fatorCodigo}`
    );

    const quantityFromDetails = activityQuantityFromDetails(
        emissaoInput,
        fator
    );
    requireSeedCondition(
        quantityFromDetails !== undefined &&
        decimalsAreEqual(
            emissaoInput.quantidadeAtividade,
            quantityFromDetails
        ),
        `${emissaoInput.codigo} activity details do not match ` +
        `quantidadeAtividade in ${fator.unidadeBase}`
    );

    const quantidadeEmitida = multiplyDecimals(
        emissaoInput.quantidadeAtividade,
        fator.valor
    );

    const documento = {
        codigo: emissaoInput.codigo,

        empresaId: empresa._id,
        produtoId: produto._id,
        fornecedorId: fornecedor._id,
        fatorEmissaoId: fator._id,

        lote: emissaoInput.lote,
        etapa: {
            ...emissaoInput.etapa,
            ordem: NumberInt(emissaoInput.etapa.ordem)
        },
        quantidadeAtividade: emissaoInput.quantidadeAtividade,
        dadosAtividade: emissaoInput.dadosAtividade,

        fatorAplicado: {
            codigo: fator.codigo,
            nome: fator.nome,
            valor: fator.valor,
            unidadeBase: fator.unidadeBase,
            escopo: fator.escopo,
            versao: fator.versao,
            fonteReferencia: fator.fonteReferencia,
            metodologia: fator.metodologia
        },

        quantidadeEmitidaKgCO2e: quantidadeEmitida,
        metodoCalculo:
            "quantidadeAtividade × fatorAplicado.valor",

        fonteEmissao: emissaoInput.fonteEmissao,
        observacao: emissaoInput.observacao,
        calculadoPor: emissaoInput.calculadoPor,
        dataEmissao: emissaoInput.dataEmissao
    };

    upsertDocument(
        "emissoes_carbono",
        { codigo: emissaoInput.codigo },
        documento
    );
});

function countAggregationResults(collectionName, pipeline) {
    const result = targetDb
        .getCollection(collectionName)
        .aggregate([
            ...pipeline,
            { $count: "total" }
        ])
        .toArray();

    return result.length === 0 ? 0 : result[0].total;
}

const minimumCounts = {
    empresas: 10,
    produtos: 10,
    fornecedores: 10,
    fatores_emissao: 10,
    emissoes_carbono: 15
};
const finalCounts = Object.fromEntries(
    Object.entries(minimumCounts).map(([collectionName, minimum]) => {
        const count = targetDb
            .getCollection(collectionName)
            .countDocuments({});

        requireSeedCondition(
            count >= minimum,
            `${collectionName} contains ${count}; expected at least ${minimum}`
        );

        return [collectionName, count];
    })
);

const orphanChecks = [
    {
        label: "products without companies",
        collectionName: "produtos",
        pipeline: [
            {
                $lookup: {
                    from: "empresas",
                    localField: "empresaId",
                    foreignField: "_id",
                    as: "reference"
                }
            },
            { $match: { reference: { $size: 0 } } }
        ]
    },
    ...[
        ["companies", "empresaId", "empresas"],
        ["products", "produtoId", "produtos"],
        ["suppliers", "fornecedorId", "fornecedores"],
        ["factors", "fatorEmissaoId", "fatores_emissao"]
    ].map(([label, localField, from]) => ({
        label: `emissions without ${label}`,
        collectionName: "emissoes_carbono",
        pipeline: [
            {
                $lookup: {
                    from,
                    localField,
                    foreignField: "_id",
                    as: "reference"
                }
            },
            { $match: { reference: { $size: 0 } } }
        ]
    }))
];
const orphanCounts = Object.fromEntries(
    orphanChecks.map(check => {
        const count = countAggregationResults(
            check.collectionName,
            check.pipeline
        );

        requireSeedCondition(count === 0, `${check.label}: ${count}`);
        return [check.label, count];
    })
);

const formulaMismatchCount = countAggregationResults(
    "emissoes_carbono",
    [
        {
            $match: {
                $expr: {
                    $ne: [
                        "$quantidadeEmitidaKgCO2e",
                        {
                            $multiply: [
                                "$quantidadeAtividade",
                                "$fatorAplicado.valor"
                            ]
                        }
                    ]
                }
            }
        }
    ]
);
requireSeedCondition(
    formulaMismatchCount === 0,
    `${formulaMismatchCount} emission calculations do not match the stored factor snapshot`
);

const activityTypes = targetDb.emissoes_carbono
    .distinct("dadosAtividade.tipo")
    .sort();
const expectedActivityTypes = [
    "ENERGIA",
    "MATERIA_PRIMA",
    "RESIDUO",
    "TRANSPORTE"
];
expectedActivityTypes.forEach(activityType => {
    requireSeedCondition(
        activityTypes.includes(activityType),
        `missing dadosAtividade variant ${activityType}`
    );
});

const ghgScopes = targetDb.emissoes_carbono
    .distinct("fatorAplicado.escopo")
    .sort();
["ESCOPO_1", "ESCOPO_2", "ESCOPO_3"].forEach(scope => {
    requireSeedCondition(
        ghgScopes.includes(scope),
        `missing GHG scope ${scope}`
    );
});

const calculationEvidence = targetDb.emissoes_carbono
    .find(
        {},
        {
            _id: 0,
            codigo: 1,
            quantidadeAtividade: 1,
            "fatorAplicado.valor": 1,
            quantidadeEmitidaKgCO2e: 1
        }
    )
    .sort({ codigo: 1 })
    .toArray();

print("Phase 4 seed verification passed.");
printjson({
    finalCounts,
    orphanCounts,
    formulaMismatchCount,
    activityTypes,
    ghgScopes,
    calculationEvidence
});
