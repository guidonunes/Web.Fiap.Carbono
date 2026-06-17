# Web.Fiap.Carbono

API RESTful em **.NET 8** para gestão, cálculo e análise de emissões de carbono na cadeia produtiva, desenvolvida para o curso de Análise e Desenvolvimento de Sistemas.

## Tema ESG

O tema escolhido é **gestão de emissões de carbono**, com foco no pilar ambiental de ESG. A solução permite registrar emissões por etapa da cadeia produtiva, calcular emissões a partir de fatores de emissão e gerar indicadores para apoiar decisões sustentáveis.

## Problema resolvido

Empresas precisam medir, rastrear e analisar suas emissões de carbono para melhorar a gestão ambiental e identificar pontos críticos da cadeia produtiva. A API centraliza dados de empresas, produtos, fornecedores, lotes, etapas da cadeia, fatores de emissão e emissões calculadas, permitindo consultas consolidadas e rastreáveis.

## Tecnologias utilizadas

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- Oracle Database
- Migrations
- AutoMapper
- JWT Authentication
- Swagger / OpenAPI
- xUnit
- Rider no Linux Ubuntu

## Arquitetura

O projeto segue uma organização baseada em **MVVM**, com separação entre entidades de domínio, ViewModels, controllers, services e repositories.

```text
Controllers/        Endpoints da API
Models/             Entidades do domínio
ViewModel/          Objetos de entrada e saída da API
Services/           Regras de negócio
Repository/         Acesso aos dados
Data/Contexts/      DatabaseContext do Entity Framework
Mapping/            Configuração do AutoMapper
Config/Security/    Configurações de JWT
Middlewares/        Tratamento global de exceções
Exceptions/         Exceções customizadas
Migrations/         Histórico de alterações do banco
```

## MER e tabelas principais

As tabelas usam o prefixo `EC_` para diferenciar o projeto no banco Oracle.

| Tabela | Função |
|---|---|
| `EC_EMPRESAS` | Empresas monitoradas pela solução |
| `EC_PRODUTOS` | Produtos vinculados a empresas |
| `EC_FORNECEDORES` | Fornecedores da cadeia produtiva |
| `EC_LOTES_PRODUCAO` | Lotes de produção dos produtos |
| `EC_ETAPAS_CADEIA` | Etapas da cadeia produtiva de cada lote |
| `EC_FATORES_EMISSAO` | Fatores usados no cálculo de CO2e |
| `EC_EMISSOES_CARBONO` | Registros de emissões calculadas |

Relacionamento principal:

```text
Empresa 1:N Produtos
Produto 1:N Lotes de Produção
Lote 1:N Etapas da Cadeia
Fornecedor 1:N Etapas da Cadeia
Etapa 1:N Emissões de Carbono
Fator de Emissão 1:N Emissões de Carbono
```

## Endpoints principais

### Autenticação

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/auth/login` | Gera token JWT |

### Emissões de carbono

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/emissoes-carbono?pageNumber=1&pageSize=10` | Lista emissões com paginação |
| `GET` | `/api/emissoes-carbono/{idEmissao}` | Busca emissão por ID |
| `POST` | `/api/emissoes-carbono/calcular` | Calcula e registra emissão de carbono |

### Produtos

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/produtos-carbono/{idProduto}/pegada` | Retorna a pegada de carbono de um produto |

### Fornecedores

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/fornecedores-carbono/ranking?pageNumber=1&pageSize=10` | Ranking de fornecedores por emissão |

### Dashboard

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/dashboard-carbono/empresas/{idEmpresa}/resumo` | Resumo ESG de emissões por empresa |

## Autenticação e autorização

O endpoint de cálculo de emissão é protegido por JWT:

```http
POST /api/emissoes-carbono/calcular
Authorization: Bearer {token}
```

Usuários de teste:

```text
admin@carbono.com / Carbono@123
analista@carbono.com / Carbono@123
```

## Configuração do Oracle

O arquivo `appsettings.Development.json` deve conter a connection string real e não deve ser versionado no Git.

```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=SEU_DATASOURCE"
  },
  "Jwt": {
    "SecretKey": "uma-chave-super-secreta-com-mais-de-32-caracteres",
    "Issuer": "Web.Fiap.Carbono",
    "Audience": "Web.Fiap.Carbono.Users",
    "ExpirationMinutes": 60
  }
}
```

O `appsettings.json` deve manter apenas valores genéricos.

## Executar migrations

Na pasta do projeto principal:

```bash
dotnet ef migrations add CreateCarbonEmissionSchema -o Migrations
dotnet ef database update
```

Para listar migrations:

```bash
dotnet ef migrations list
```

## Rodar o projeto no Linux/Rider

Na raiz do projeto da API:

```bash
dotnet restore
dotnet build
dotnet run
```

Acesse o Swagger em:

```text
http://localhost:SUA_PORTA/swagger
```

## Executar testes xUnit

Na raiz da solution:

```bash
dotnet test
```

Os testes utilizam uma base `InMemory` com dados de seed controlados, permitindo validar os endpoints sem depender diretamente do ambiente Oracle da FIAP. A integração real com Oracle foi validada manualmente via Swagger/Postman.

## Exemplos de JSON

### Login

```json
{
  "email": "admin@carbono.com",
  "senha": "Carbono@123"
}
```

### Cálculo de emissão

```json
{
  "idEtapa": 1,
  "idFator": 1,
  "quantidadeAtividade": 100,
  "fonteEmissao": "Diesel",
  "observacao": "Teste de cálculo de emissão de carbono"
}
```

Resposta esperada:

```http
201 Created
```

## Validações e segurança

- Paginação obrigatória em endpoints de listagem
- `pageNumber` mínimo igual a 1
- `pageSize` entre 1 e 50
- Tratamento global de exceções
- Validação de entrada com Data Annotations
- JWT em endpoint crítico
- Teste automatizado para endpoints públicos
- Teste de segurança para endpoint protegido sem token

## Status da entrega

- API em .NET 8 criada
- Oracle integrado via Entity Framework Core
- Migrations implementadas
- Endpoints RESTful principais criados
- Paginação implementada
- JWT configurado
- Swagger organizado por tema
- Testes xUnit implementados
