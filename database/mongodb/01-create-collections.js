const targetDb = db.getSiblingDB("fiap_carbono");

function ensureCommandWorked(result, message) {
  if (!result || result.ok !== 1) {
    throw new Error(`${message}: ${EJSON.stringify(result)}`);
  }
}

function ensureCollection(name, validator) {
  const exists =
    targetDb.getCollectionInfos({ name }).length > 0;

  const options = {
    validator,
    validationLevel: "strict",
    validationAction: "error"
  };

  if (exists) {
    ensureCommandWorked(
      targetDb.runCommand({
        collMod: name,
        ...options
      }),
      `Could not update validator for ${name}`
    );

    print(`Updated validator: ${name}`);
    return;
  }

  ensureCommandWorked(
    targetDb.createCollection(name, options),
    `Could not create collection ${name}`
  );
  print(`Created collection: ${name}`);
}

const empresasValidator = {
  $jsonSchema: {
    bsonType: "object",
    title: "Empresa",
    required: [
      "_id",
      "codigo",
      "razaoSocial",
      "cnpj",
      "ativa",
      "criadoEm",
      "atualizadoEm",
      "schemaVersion"
    ],
    properties: {
      _id: {
        bsonType: "objectId",
        description: "Identificador MongoDB da empresa"
      },

      codigo: {
        bsonType: "string",
        minLength: 1,
        description: "Código interno da empresa"
      },

      razaoSocial: {
        bsonType: "string",
        minLength: 1,
        description: "Razão social da empresa"
      },

      nomeFantasia: {
        bsonType: "string",
        description: "Nome fantasia opcional"
      },

      cnpj: {
        bsonType: "string",
        pattern: "^[0-9]{14}$",
        description: "CNPJ contendo exatamente 14 dígitos"
      },

      setor: {
        bsonType: "string",
        description: "Setor de atuação"
      },

      ativa: {
        bsonType: "bool",
        description: "Indica se a empresa está ativa"
      },

      metasReducao: {
        bsonType: "array",
        description: "Metas ESG para redução de emissões"
      },

      governanca: {
        bsonType: "object",
        description: "Informações opcionais de governança"
      },

      criadoEm: {
        bsonType: "date",
        description: "Data de criação do documento"
      },

      atualizadoEm: {
        bsonType: "date",
        description: "Data da última atualização"
      },

      schemaVersion: {
        bsonType: "int",
        minimum: 1,
        description: "Versão do formato do documento"
      }
    }
  }
};

const produtosValidator = {
  $jsonSchema: {
    bsonType: "object",
    title: "Produto",
    required: [
      "_id",
      "empresaId",
      "codigo",
      "nome",
      "unidadeFuncional",
      "ativo",
      "criadoEm",
      "atualizadoEm",
      "schemaVersion"
    ],
    properties: {
      _id: {
        bsonType: "objectId",
        description: "Identificador MongoDB do produto"
      },

      empresaId: {
        bsonType: "objectId",
        description: "Referência para empresas._id"
      },

      codigo: {
        bsonType: "string",
        minLength: 1,
        description: "Código do produto dentro da empresa"
      },

      nome: {
        bsonType: "string",
        minLength: 1,
        description: "Nome do produto"
      },

      unidadeFuncional: {
        bsonType: "string",
        minLength: 1,
        description: "Unidade usada para avaliar a pegada do produto"
      },

      ativo: {
        bsonType: "bool",
        description: "Indica se o produto está ativo"
      },

      atributosAmbientais: {
        bsonType: "object",
        additionalProperties: true,
        description: "Atributos ambientais extensíveis do produto"
      },

      criadoEm: {
        bsonType: "date"
      },

      atualizadoEm: {
        bsonType: "date"
      },

      schemaVersion: {
        bsonType: "int",
        minimum: 1
      }
    }
  }
};

const fornecedoresValidator = {
  $jsonSchema: {
    bsonType: "object",
    title: "Fornecedor",
    required: [
      "_id",
      "codigo",
      "razaoSocial",
      "cnpj",
      "ativo",
      "criadoEm",
      "atualizadoEm",
      "schemaVersion"
    ],
    properties: {
      _id: {
        bsonType: "objectId",
        description: "Identificador MongoDB do fornecedor"
      },

      codigo: {
        bsonType: "string",
        minLength: 1,
        description: "Código interno do fornecedor"
      },

      razaoSocial: {
        bsonType: "string",
        minLength: 1,
        description: "Razão social do fornecedor"
      },

      nomeFantasia: {
        bsonType: "string"
      },

      cnpj: {
        bsonType: "string",
        pattern: "^[0-9]{14}$",
        description: "CNPJ contendo exatamente 14 dígitos"
      },

      ativo: {
        bsonType: "bool"
      },

      certificacoes: {
        bsonType: "array",
        description: "Certificações ESG do fornecedor",
        items: {
          bsonType: "object",
          required: ["nome"],
          additionalProperties: true,
          properties: {
            nome: {
              bsonType: "string",
              minLength: 1
            },

            emissor: {
              bsonType: "string"
            },

            validaAte: {
              bsonType: "date"
            }
          }
        }
      },

      categoriasAtuacao: {
        bsonType: "array",
        description: "Categorias operacionais do fornecedor",
        items: {
          bsonType: "string",
          minLength: 1
        }
      },

      indicadoresSociais: {
        bsonType: "object",
        additionalProperties: true,
        description: "Indicadores sociais e trabalhistas"
      },

      conformidadeAmbiental: {
        bsonType: "object",
        additionalProperties: true,
        description: "Situação de conformidade ambiental"
      },

      statusAuditoria: {
        bsonType: "string",
        minLength: 1,
        description: "Situação da auditoria ESG"
      },

      nivelRiscoEsg: {
        bsonType: "string",
        minLength: 1,
        description: "Classificação de risco ESG"
      },

      criadoEm: {
        bsonType: "date"
      },

      atualizadoEm: {
        bsonType: "date"
      },

      schemaVersion: {
        bsonType: "int",
        minimum: 1
      }
    }
  }
};

const fatoresEmissaoValidator = {
  $jsonSchema: {
    bsonType: "object",
    title: "Fator de emissão",
    required: [
      "_id",
      "codigo",
      "nome",
      "valor",
      "unidadeBase",
      "escopo",
      "versao",
      "ativo",
      "criadoEm",
      "atualizadoEm",
      "schemaVersion"
    ],
    properties: {
      _id: {
        bsonType: "objectId",
        description: "Identificador MongoDB do fator"
      },

      codigo: {
        bsonType: "string",
        minLength: 1,
        description: "Código do fator de emissão"
      },

      nome: {
        bsonType: "string",
        minLength: 1
      },

      valor: {
        bsonType: "decimal",
        minimum: NumberDecimal("0"),
        description: "Valor do fator armazenado como Decimal128"
      },

      unidadeBase: {
        bsonType: "string",
        minLength: 1,
        description: "Unidade sobre a qual o fator é aplicado"
      },

      categoria: {
        bsonType: "string",
        minLength: 1,
        description: "Categoria de atividade do fator"
      },

      escopo: {
        enum: ["ESCOPO_1", "ESCOPO_2", "ESCOPO_3"],
        description: "Escopo do GHG Protocol"
      },

      versao: {
        bsonType: "int",
        minimum: 1,
        description: "Versão do fator de emissão"
      },

      fonteReferencia: {
        bsonType: "string",
        description: "Fonte técnica ou oficial do fator"
      },

      metodologia: {
        bsonType: "string",
        description: "Metodologia utilizada pela fonte"
      },

      validoDe: {
        bsonType: "date"
      },

      validoAte: {
        bsonType: ["date", "null"]
      },

      ativo: {
        bsonType: "bool"
      },

      criadoEm: {
        bsonType: "date"
      },

      atualizadoEm: {
        bsonType: "date"
      },

      schemaVersion: {
        bsonType: "int",
        minimum: 1
      }
    }
  }
};

const emissoesCarbonoValidator = {
  $jsonSchema: {
    bsonType: "object",
    title: "Emissão de carbono",
    required: [
      "_id",
      "empresaId",
      "produtoId",
      "fornecedorId",
      "fatorEmissaoId",
      "lote",
      "etapa",
      "quantidadeAtividade",
      "dadosAtividade",
      "fatorAplicado",
      "quantidadeEmitidaKgCO2e",
      "metodoCalculo",
      "calculadoPor",
      "dataEmissao",
      "criadoEm",
      "atualizadoEm",
      "schemaVersion"
    ],
    properties: {
      _id: {
        bsonType: "objectId",
        description: "Identificador MongoDB da emissão"
      },

      empresaId: {
        bsonType: "objectId",
        description: "Referência para empresas._id"
      },

      produtoId: {
        bsonType: "objectId",
        description: "Referência para produtos._id"
      },

      fornecedorId: {
        bsonType: "objectId",
        description: "Referência para fornecedores._id"
      },

      fatorEmissaoId: {
        bsonType: "objectId",
        description: "Referência para fatores_emissao._id"
      },

      lote: {
        bsonType: "object",
        required: [
          "codigo",
          "quantidadeProduzida",
          "unidade",
          "dataProducao"
        ],
        properties: {
          codigo: {
            bsonType: "string",
            minLength: 1
          },

          quantidadeProduzida: {
            bsonType: "decimal",
            minimum: NumberDecimal("0")
          },

          unidade: {
            bsonType: "string",
            minLength: 1
          },

          dataProducao: {
            bsonType: "date"
          }
        }
      },

      etapa: {
        bsonType: "object",
        required: [
          "nome",
          "ordem",
          "categoria"
        ],
        properties: {
          nome: {
            bsonType: "string",
            minLength: 1
          },

          ordem: {
            bsonType: "int",
            minimum: 1
          },

          categoria: {
            bsonType: "string",
            minLength: 1,
            description: "Categoria descritiva da etapa"
          },

          local: {
            bsonType: "string"
          }
        }
      },

      quantidadeAtividade: {
        bsonType: "decimal",
        minimum: NumberDecimal("0"),
        description: "Quantidade utilizada no cálculo"
      },

      dadosAtividade: {
        bsonType: "object",
        required: ["tipo"],
        additionalProperties: true,
        description: "Dados flexíveis conforme o tipo de atividade",
        properties: {
          tipo: {
            enum: [
              "TRANSPORTE",
              "ENERGIA",
              "MATERIA_PRIMA",
              "RESIDUO"
            ]
          },

          distanciaKm: {
            bsonType: "decimal",
            minimum: NumberDecimal("0")
          },

          cargaToneladas: {
            bsonType: "decimal",
            minimum: NumberDecimal("0")
          },

          consumoKwh: {
            bsonType: "decimal",
            minimum: NumberDecimal("0")
          },

          percentualRenovavel: {
            bsonType: "decimal",
            minimum: NumberDecimal("0"),
            maximum: NumberDecimal("100")
          },

          pesoKg: {
            bsonType: "decimal",
            minimum: NumberDecimal("0")
          },

          percentualReciclado: {
            bsonType: "decimal",
            minimum: NumberDecimal("0"),
            maximum: NumberDecimal("100")
          },

          distanciaDestinoKm: {
            bsonType: "decimal",
            minimum: NumberDecimal("0")
          }
        }
      },

      fatorAplicado: {
        bsonType: "object",
        description: "Snapshot do fator utilizado no cálculo",
        required: [
          "codigo",
          "nome",
          "valor",
          "unidadeBase",
          "escopo",
          "versao"
        ],
        properties: {
          codigo: {
            bsonType: "string",
            minLength: 1
          },

          nome: {
            bsonType: "string",
            minLength: 1
          },

          valor: {
            bsonType: "decimal",
            minimum: NumberDecimal("0")
          },

          unidadeBase: {
            bsonType: "string",
            minLength: 1
          },

          escopo: {
            enum: [
              "ESCOPO_1",
              "ESCOPO_2",
              "ESCOPO_3"
            ]
          },

          versao: {
            bsonType: "int",
            minimum: 1
          },

          fonteReferencia: {
            bsonType: "string"
          },

          metodologia: {
            bsonType: "string"
          }
        }
      },

      quantidadeEmitidaKgCO2e: {
        bsonType: "decimal",
        minimum: NumberDecimal("0"),
        description: "Resultado calculado em kgCO2e"
      },

      metodoCalculo: {
        bsonType: "string",
        minLength: 1
      },

      fonteEmissao: {
        bsonType: "string"
      },

      observacao: {
        bsonType: "string"
      },

      calculadoPor: {
        bsonType: "string",
        minLength: 1,
        description: "Usuário responsável pelo cálculo"
      },

      dataEmissao: {
        bsonType: "date"
      },

      criadoEm: {
        bsonType: "date"
      },

      atualizadoEm: {
        bsonType: "date"
      },

      schemaVersion: {
        bsonType: "int",
        minimum: 1
      }
    }
  }
};

ensureCollection("empresas", empresasValidator);
ensureCollection("produtos", produtosValidator);
ensureCollection("fornecedores", fornecedoresValidator);
ensureCollection("fatores_emissao", fatoresEmissaoValidator);
ensureCollection("emissoes_carbono", emissoesCarbonoValidator);

print("MongoDB collections and validators are ready.");
