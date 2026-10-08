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

A API está em `src/Web.Fiap.Carbono/` e a ferramenta de migração em `src/Web.Fiap.Carbono.Migration/`. O projeto `Web.Fiap.Carbono.Tests/`, a solução, o `Dockerfile`, `database/` e `archive/` permanecem na raiz. Execute os comandos deste README a partir da raiz do repositório.

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

Para executar a API diretamente com o SDK, use `src/Web.Fiap.Carbono/appsettings.Development.json`, que não é versionado, ou variáveis de ambiente:

| Variável | Exemplo |
|---|---|
| MongoDb__ConnectionString | mongodb://localhost:27017 |
| MongoDb__DatabaseName | fiap_carbono |
| Jwt__SecretKey | chave local com pelo menos 32 caracteres |
| Jwt__Issuer | Web.Fiap.Carbono |
| Jwt__Audience | Web.Fiap.Carbono.Users |
| Jwt__ExpirationMinutes | 60 |

Não coloque credenciais, tokens ou strings de conexão reais no Git.

No Docker Compose, prepare o arquivo `.env` conforme a seção abaixo. A conexão da API está definida no `docker-compose.yml` como `mongodb://mongodb:27017`, com banco `fiap_carbono`. Os valores `MongoDb__ConnectionString` e `MongoDb__DatabaseName` do `.env.example` documentam esses valores, mas atualmente não os substituem no Compose. `localhost` dentro do container da API não aponta para o MongoDB.

## Como executar localmente com Docker

Pré-requisitos: Docker Engine em execução, plugin Docker Compose com suporte a `--wait`, acesso aos registros de imagens e `curl` no host. O comando de geração de chave abaixo usa OpenSSL. Não é necessário instalar .NET SDK ou `mongosh` no host para executar via Compose.

### Preparar a configuração

Na raiz do repositório, crie `.env` somente se ele ainda não existir:

~~~bash
if [ ! -f .env ]; then cp .env.example .env; fi
openssl rand -hex 32
~~~

Copie a chave gerada para `Jwt__SecretKey` no `.env`; esse campo está vazio no exemplo, e o Compose rejeita a configuração enquanto ele não for preenchido. Se você já tem uma chave local configurada, mantenha-a. Mantenha a chave privada; não inclua sua saída em prints. O `.env` é ignorado pelo Git e excluído do contexto da imagem.

Configure também `API_PORT` (padrão `8081`), `Jwt__Issuer`, `Jwt__Audience` e `Jwt__ExpirationMinutes` (padrão `60`). O `.env` fornece esses valores ao Compose; ele não é carregado automaticamente por `dotnet run`.

Na entrega, inclua `.env.example`, mas exclua `.env`, `staging.env`, `production.env` e `appsettings.Development.json` do ZIP. As regras do Git e do Docker não filtram automaticamente um ZIP criado por outra ferramenta; confira seu conteúdo na fase de empacotamento. O arquivo histórico `Web.Fiap.Carbono.Source.zip` não representa a entrega CI/CD atual.

### Iniciar API e banco

~~~bash
docker compose -p carbono-local --env-file .env config --quiet
docker compose -p carbono-local --env-file .env up -d --build --wait --wait-timeout 120
docker compose -p carbono-local --env-file .env ps
~~~

Espere os serviços `api` e `mongodb` ficarem `healthy`. O limite de 120 segundos se aplica à espera de disponibilidade, não à duração do build. Use `config --quiet` para validar sem imprimir a configuração resolvida, que contém a chave JWT.

Com `API_PORT=8081`, verifique:

~~~bash
curl --fail --silent --show-error --include --max-time 5 \
  http://localhost:8081/api/empresas
~~~

O resultado esperado é HTTP `200` e um único array JSON contendo uma empresa com `codigo` igual a `EMP-001`. O health check interno exige essas condições; corpo vazio, múltiplos documentos JSON, erro HTTP ou ausência dessa empresa falham. Ajuste a URL se escolher outra porta.

A API escuta HTTP na porta interna `8080`. O Compose atual usa o ambiente ASP.NET Core padrão `Production`, sem Swagger; não use `/swagger` para verificar disponibilidade. Esse nome de ambiente não significa que houve deploy de produção. MongoDB não publica a porta `27017` no host. Para inspecioná-lo, use:

~~~bash
docker compose -p carbono-local --env-file .env exec mongodb mongosh fiap_carbono
~~~

### Inicialização e dados persistentes

Em um volume novo, o MongoDB executa automaticamente, nesta ordem:

1. `01-create-collections.js`: cria as cinco coleções e validadores.
2. `02-create-indexes.js`: cria índices únicos e de consulta.
3. `03-seed.js`: insere o conjunto coerente de demonstração com pelo menos dez documentos por coleção.

Os scripts `04-crud-demo.js` e `05-aggregation-queries.js` permanecem em `database/mongodb/` para execução manual deliberada. Eles não fazem parte da inicialização. O primeiro demonstra CRUD com registros `CRUD-TEMP`; o segundo consulta agregações.

Os dados ficam no volume `carbono-local_fiap_carbono_mongodb_data`, montado em `/data/db`. Com arquivos de banco existentes, a inicialização é ignorada, mesmo se as coleções estiverem vazias. Não execute novamente o seed para reiniciar: seus upserts substituem documentos e podem sobrescrever alterações. Não use `down -v` nem remova o volume para uma reinicialização normal.

Se a primeira inicialização falhar, preserve o volume e examine os logs antes de escolher uma recuperação. Um volume parcialmente inicializado pode ser ignorado na próxima partida; reiniciar não garante completar o seed. A presença de `EMP-001` também não comprova todos os índices e dados. Não há reset automático do banco.

### Reiniciar, atualizar e parar

Para reiniciar os containers com a configuração existente e aguardar disponibilidade:

~~~bash
docker compose -p carbono-local --env-file .env restart
docker compose -p carbono-local --env-file .env up -d --wait --wait-timeout 120
~~~

Depois de alterar código, Dockerfile, Compose ou `.env`, use o comando abaixo para reconstruir a imagem e aplicar a configuração. `restart` sozinho não aplica novas variáveis de ambiente. Consulte as referências de [up](https://docs.docker.com/reference/cli/docker/compose/up/) e [restart](https://docs.docker.com/reference/cli/docker/compose/restart/).

~~~bash
docker compose -p carbono-local --env-file .env up -d --build --wait --wait-timeout 120
~~~

Para recriar os containers deliberadamente, preservando o volume:

~~~bash
docker compose -p carbono-local --env-file .env up -d --force-recreate --wait --wait-timeout 120
~~~

Para parar e depois retomar o mesmo ambiente:

~~~bash
docker compose -p carbono-local --env-file .env stop
docker compose -p carbono-local --env-file .env up -d --wait --wait-timeout 120
~~~

Mantenha o mesmo nome de projeto (`-p`) em todos os comandos: ele identifica a rede, os containers e o volume desse ambiente. Trocar o nome cria recursos separados. Se a porta estiver ocupada, escolha outra em `.env` e ajuste a URL de verificação.

O projeto usado nas verificações anteriores é `carbono-init-check`, com API na porta `8083`. Para continuar usando seus dados, em vez de criar `carbono-local`, use `-p carbono-init-check` nos comandos e preserve `API_PORT=8083`, por exemplo:

~~~bash
API_PORT=8083 docker compose -p carbono-init-check --env-file .env up -d --wait --wait-timeout 120
~~~

Para investigar uma falha, consulte `ps` e os logs; não remova o volume:

~~~bash
docker compose -p carbono-local --env-file .env ps -a
docker compose -p carbono-local --env-file .env logs --no-color --tail 100 api mongodb
~~~

Repita a consulta a `/api/empresas` após a retomada. Revise os logs antes de compartilhar evidências para evitar exposição de dados ou credenciais.

## Execução com o SDK

Como alternativa ao Compose completo, instale .NET 8 SDK e disponibilize uma instância MongoDB separada, acessível pelo host e previamente inicializada com os scripts `01`, `02` e `03` em um banco novo. Configure MongoDB e JWT em `appsettings.Development.json` ou no ambiente. O MongoDB do Compose acima não fica acessível em `localhost:27017`.

Compile e execute:

~~~bash
dotnet restore Web.Fiap.Carbono.sln
dotnet build Web.Fiap.Carbono.sln
dotnet run --project src/Web.Fiap.Carbono/Web.Fiap.Carbono.csproj --launch-profile http
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

A configuração JWT dos testes usa valores sintéticos definidos na factory; não é necessário fornecer `appsettings.Development.json` nem iniciar o MongoDB do Compose. A fixture de API usa rede do host Linux e porta `27019`: evite execuções simultâneas. Se houver timeout com VPN ativa, investigue a conectividade do Docker; o baseline registra uma execução que passou após desativar a VPN.

Para repetir a verificação em Release e guardar o resultado após a reorganização:

~~~bash
dotnet restore Web.Fiap.Carbono.sln
dotnet build Web.Fiap.Carbono.sln --configuration Release --no-restore
dotnet test Web.Fiap.Carbono.sln --configuration Release --no-build \
  --logger "trx;LogFileName=baseline.trx" \
  --results-directory ./artifacts/test-results/after
~~~

## Containerização

O `Dockerfile` na raiz usa build multi-stage: restaura e publica a API com o SDK .NET 8 e copia a publicação para a imagem ASP.NET Core 8. A imagem final contém `curl` e `jq` para o health check e executa a API com o usuário definido por `APP_UID`.

Para apenas construir a imagem local:

~~~bash
docker build --file Dockerfile --tag web-fiap-carbono:local .
~~~

O `docker-compose.yml` executa essa API e `mongo:8.0.29-noble` na rede padrão do projeto, com volume persistente próprio. A API aguarda o health check do MongoDB e verifica dados pelo endpoint `/api/empresas`. A imagem `web-fiap-carbono:local` é local; publicação no Docker Hub e deploys CI/CD continuam planejados no [roadmap](docs/ci-cd-roadmap.md).

## Erros e documentação complementar

Os códigos principais são 400 para validação, 401 para autenticação, 403 para autorização, 404 para ausências, 409 para conflitos, 422 para fator inválido e 500 para falhas inesperadas.

- [Decisão técnica da migração](docs/mongodb-migration.md)
- [Baseline Oracle da reconciliação](docs/oracle-baseline.md)
- [Relatório de reconciliação](docs/oracle-mongodb-reconciliation.md)
- [Ferramenta de migração legada](src/Web.Fiap.Carbono.Migration/README.md)
- [Roadmap CI/CD](docs/ci-cd-roadmap.md)
- [Baseline CI/CD](docs/ci-cd-baseline.md)
