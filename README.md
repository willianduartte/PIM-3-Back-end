# Template Minimal API

Template base para projetos em Minimal API com foco em simplicidade e boas praticas de arquitetura.

## Estrutura (Clean Architecture)

- `src/laVaiPizza.Domain`: entidades e regras centrais.
- `src/laVaiPizza.Application`: contratos e casos de uso.
- `src/laVaiPizza.Infrastructure`: EF Core (PostgreSQL), repositorios e DI.
- `src/laVaiPizza.Api`: endpoints, autenticacao, Swagger e Health Check.

## O que ja vem configurado

- Entity Framework Core com PostgreSQL (Npgsql)
- Swagger/OpenAPI com suporte a JWT Bearer
- JWT Bearer simples
- Health Check (`/health`)

## Rodando o projeto

```bash
dotnet run --project src/laVaiPizza.Api
```

## Configuracao do banco (PostgreSQL)

- Connection string padrao em `ConnectionStrings:DefaultConnection` no `appsettings`.
- Exemplo:

```text
Host=localhost;Port=5432;Database=template_minimal_api;Username=postgres;Password=postgres
```

## Fluxo de exemplo (request/response)

1. Gerar token:

```http
POST /auth/token
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

Resposta:

```json
{
  "accessToken": "eyJhbGciOi..."
}
```

2. Criar todo (endpoint protegido):

```http
POST /todos
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "title": "Criar template minimal api"
}
```

Resposta:

```json
{
  "id": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "title": "Criar template minimal api",
  "isDone": false,
  "createdAtUtc": "2026-04-24T17:10:00Z"
}
```
