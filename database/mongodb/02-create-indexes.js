const targetDb = db.getSiblingDB("fiap_carbono");
const requiredCollections = [
  "empresas",
  "produtos",
  "fornecedores",
  "fatores_emissao",
  "emissoes_carbono"
];

const existingCollections = new Set(targetDb.getCollectionNames());
const missingCollections = requiredCollections.filter(
  (name) => !existingCollections.has(name)
);

if (missingCollections.length > 0) {
  throw new Error(
    `Run 01-create-collections.js first. Missing collections: ${missingCollections.join(", ")}`
  );
}

targetDb.empresas.createIndex(
  { cnpj: 1 },
  { unique: true, name: "ux_empresas_cnpj" }
);

targetDb.empresas.createIndex(
  { codigo: 1 },
  { unique: true, name: "ux_empresas_codigo" }
);

targetDb.fornecedores.createIndex(
  { cnpj: 1 },
  { unique: true, name: "ux_fornecedores_cnpj" }
);

targetDb.fornecedores.createIndex(
  { codigo: 1 },
  { unique: true, name: "ux_fornecedores_codigo" }
);

targetDb.produtos.createIndex(
  { empresaId: 1, codigo: 1 },
  { unique: true, name: "ux_produtos_empresa_codigo" }
);

targetDb.fatores_emissao.createIndex(
  { codigo: 1, versao: 1 },
  { unique: true, name: "ux_fatores_codigo_versao" }
);

targetDb.emissoes_carbono.createIndex(
  { produtoId: 1, dataEmissao: -1 },
  { name: "ix_emissoes_produto_data" }
);

targetDb.emissoes_carbono.createIndex(
  { empresaId: 1, dataEmissao: -1 },
  { name: "ix_emissoes_empresa_data" }
);

targetDb.emissoes_carbono.createIndex(
  { fornecedorId: 1, dataEmissao: -1 },
  { name: "ix_emissoes_fornecedor_data" }
);

targetDb.emissoes_carbono.createIndex(
  { fatorEmissaoId: 1 },
  { name: "ix_emissoes_fator" }
);

targetDb.emissoes_carbono.createIndex(
  { "etapa.categoria": 1 },
  { name: "ix_emissoes_etapa_categoria" }
);

targetDb.emissoes_carbono.createIndex(
  { codigo: 1 },
  {
    unique: true,
    name: "ux_emissoes_codigo_seed",
    partialFilterExpression: {
      codigo: { $type: "string" }
    }
  }
);

print("MongoDB indexes created or already present.");
