# Web.Fiap.Carbono

API REST acadêmica da FIAP para medir e analisar emissões de gases de efeito estufa em cadeias de suprimentos. A persistência ativa usa MongoDB e o projeto mantém exatamente cinco coleções ESG.

## Funcionalidades

- Autenticação JWT com os papéis ADMIN e ANALISTA_ESG.
- CRUD de empresas, produtos, fornecedores e fatores de emissão.
- Cálculo decimal de emissões em kgCO2e.
- Pegada de produto, ranking de fornecedores e dashboard empresarial por agregações MongoDB.
- Validação de referências, fatores, ObjectIds, paginação e índices únicos.
- Testes de integração contra MongoDB descartável.
- Swagger/OpenAPI no ambiente Development.

Fórmula: quantidadeEmitidaKgCO2e = quantidadeAtividade × fatorAplicado.valor. A unidade da atividade deve ser compatível com a unidade-base do fator.

## Tecnologias

| Área | Tecnologia |
|---|---|
| Runtime e API | .NET 8, C# e ASP.NET Core |
| Persistência | MongoDB 8 com MongoDB.Driver |
| Autenticação | JWT Bearer |
| Documentação | Swagger/OpenAPI com Swashbuckle |
| Testes | xUnit, WebApplicationFactory e Testcontainers |
| Empacotamento | Docker multi-stage |

## Arquitetura

~~~mermaid
flowchart LR
    Cliente[Cliente HTTP] --> Controllers[Controllers]
    Controllers --> Services[Serviços MongoDB]
    Services --> Repositories[Repositórios MongoDB]
    Repositories --> Mongo[(MongoDB)]
    Services --> Rules[Validações e cálculo decimal]
    Controllers --> Swagger[Swagger/OpenAPI]
~~~

O código ativo está organizado em Controllers, Services/MongoDb, Data/MongoDb, Models/Documents, Dtos/MongoDb, Config/MongoDb e Middlewares. A ferramenta Web.Fiap.Carbono.Migration é separada da API e o código Oracle histórico está em archive/oracle.

## Modelo MongoDB

O banco padrão é fiap_carbono e contém exatamente:

| Coleção | Responsabilidade |
|---|---|
| empresas | Identidade, metas ESG e governança |
| produtos | Produtos, empresa proprietária e atributos ambientais flexíveis |
| fornecedores | Dados ambientais, sociais, certificações e auditorias |
| fatores_emissao | Fatores versionados, unidades, fontes, escopos e validade |
| emissoes_carbono | Eventos auditáveis e contexto do cálculo |

Produtos referenciam empresas por empresaId. Emissões referenciam empresa, produto, fornecedor e fator por ObjectId. Em cada emissão, lote, etapa, dadosAtividade e fatorAplicado são snapshots embutidos. Lotes e etapas não são coleções.

A flexibilidade de dadosAtividade representa diferenças reais entre TRANSPORTE, ENERGIA, MATERIA_PRIMA e RESIDUO.

## Configuração

Use appsettings.Development.json, que não é versionado, ou variáveis de ambiente:

| Variável | Exemplo |
|---|---|
| MongoDb__ConnectionString | mongodb://localhost:27017 |
| MongoDb__DatabaseName | fiap_carbono |
| Jwt__SecretKey | chave local com pelo menos 32 caracteres |
| Jwt__Issuer | Web.Fiap.Carbono |
| Jwt__Audience | Web.Fiap.Carbono.Users |
| Jwt__ExpirationMinutes | 60 |

Não coloque credenciais, tokens ou strings de conexão reais no Git.

## Inicialização

Pré-requisitos: .NET 8 SDK, Docker Compose ou MongoDB 8 local e mongosh.

Inicie o banco:

~~~bash
docker compose up -d mongodb
docker compose ps
~~~

Execute os scripts nesta ordem:

~~~bash
mongosh "mongodb://localhost:27017/fiap_carbono" --file database/mongodb/01-create-collections.js
mongosh "mongodb://localhost:27017/fiap_carbono" --file database/mongodb/02-create-indexes.js
mongosh "mongodb://localhost:27017/fiap_carbono" --file database/mongodb/03-seed.js
mongosh "mongodb://localhost:27017/fiap_carbono" --file database/mongodb/04-crud-demo.js
mongosh "mongodb://localhost:27017/fiap_carbono" --file database/mongodb/05-aggregation-queries.js
~~~

1. 01-create-collections.js cria as cinco coleções e validadores.
2. 02-create-indexes.js cria índices únicos e de consulta.
3. 03-seed.js insere pelo menos dez documentos coerentes em cada coleção.
4. 04-crud-demo.js demonstra CRUD com registros CRUD-TEMP e preserva os dados permanentes.
5. 05-aggregation-queries.js executa agregações sem alterar dados.

O seed usa chaves de negócio determinísticas. Para repetir a demonstração, use um banco de desenvolvimento limpo. A API nunca remove o banco no startup.

Compile e execute:

~~~bash
dotnet restore Web.Fiap.Carbono.sln
dotnet build Web.Fiap.Carbono.sln
dotnet run --project Web.Fiap.Carbono/Web.Fiap.Carbono.csproj
~~~

A API fica em http://localhost:5269 e o Swagger em http://localhost:5269/swagger no ambiente Development.

## Endpoints

Todas as rotas usam o prefixo /api.

| Método | Rota | Autorização | Finalidade |
|---|---|---|---|
| POST | /api/auth/login | Pública | Emitir JWT |
| GET | /api/emissoes-carbono?pageNumber=1&pageSize=10 | Pública | Listar emissões |
| GET | /api/emissoes-carbono/{idEmissao} | Pública | Consultar emissão por legacyId |
| GET | /api/emissoes-carbono/mongodb/{id} | Pública | Consultar por ObjectId |
| POST | /api/emissoes-carbono/calcular | ADMIN ou ANALISTA_ESG | Calcular emissão |
| PUT, DELETE | /api/emissoes-carbono/mongodb/{id} | ADMIN/ANALISTA_ESG e ADMIN | CRUD temporário de emissão |

CRUD completo está disponível em /api/empresas, /api/produtos, /api/fornecedores e /api/fatores-emissao, com rotas POST, GET, PUT e DELETE. Criação e atualização exigem ADMIN ou ANALISTA_ESG, exceto fatores, que exigem ADMIN; exclusão exige ADMIN.

Analytics:

- GET /api/produtos-carbono/{id}/pegada: total e totais por etapa.
- GET /api/fornecedores-carbono/ranking?pageNumber=1&pageSize=10: ranking paginado.
- GET /api/dashboard-carbono/empresas/{id}/resumo: totais, meses, produtos, fornecedores e escopos.

Paginação começa em 1 e aceita pageSize entre 1 e 50.

## Testes

~~~bash
dotnet test Web.Fiap.Carbono.sln
~~~

Os testes usam a aplicação real e MongoDB 8 descartável em container, com banco isolado. Cobrem CRUD, índices únicos, ObjectIds inválidos, referências ausentes, fatores inativos/expirados, cálculo decimal, snapshots, quatro variantes de atividade, agregações, paginação, autenticação, autorização, erros e Swagger.

## Docker

~~~bash
docker build --file Web.Fiap.Carbono/Dockerfile --tag web-fiap-carbono .
docker run --rm --publish 8080:8080 \
  --env MongoDb__ConnectionString="mongodb://host.docker.internal:27017" \
  --env MongoDb__DatabaseName="fiap_carbono" \
  --env Jwt__SecretKey="SUA_CHAVE_JWT_LOCAL" \
  --env Jwt__Issuer="Web.Fiap.Carbono" \
  --env Jwt__Audience="Web.Fiap.Carbono.Users" \
  --env Jwt__ExpirationMinutes="60" \
  web-fiap-carbono
~~~

## Erros e documentação complementar

Os códigos principais são 400 para validação, 401 para autenticação, 403 para autorização, 404 para ausências, 409 para conflitos, 422 para fator inválido e 500 para falhas inesperadas.

- [Decisão técnica da migração](docs/mongodb-migration.md)
- [Baseline Oracle da reconciliação](docs/oracle-baseline.md)
- [Relatório de reconciliação](docs/oracle-mongodb-reconciliation.md)
- [Ferramenta de migração legada](Web.Fiap.Carbono.Migration/README.md)
