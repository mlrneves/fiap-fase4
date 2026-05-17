# Catalog API — FCG Fase 4

A **Catalog API** é o microsserviço responsável pelo catálogo de jogos da plataforma FIAP Cloud Games.

Faz parte da arquitetura Cloud-Native da Fase 4, com persistência poliglota, cache distribuído e busca avançada.

---

## Responsabilidades

- CRUD de jogos (Admin)
- Consulta do catálogo (usuários autenticados)
- Compras e biblioteca do usuário
- Promoções de jogos
- **Cache distribuído com Redis** — listagem de jogos com TTL de 5 minutos
- **Busca avançada com Elasticsearch** — fuzzy search, ordenação por relevância
- **Audit log com DynamoDB** — registra criação, alteração e remoção de jogos

---

## Endpoints principais

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/games` | Lista todos os jogos (com cache Redis) |
| `GET` | `/api/games/{id}` | Busca jogo por ID |
| `GET` | `/api/games/search?q=` | Busca fuzzy no Elasticsearch |
| `POST` | `/api/games` | Cria jogo — Admin |
| `PUT` | `/api/games` | Atualiza jogo — Admin |
| `DELETE` | `/api/games/{id}` | Remove jogo — Admin |
| `GET` | `/api/games/recommendations/{userId}` | Recomendações por histórico de compras |
| `GET` | `/api/purchases/user/{userId}/library` | Biblioteca do usuário |
| `GET` | `/api/audit-logs?entityType=Game` | Audit logs do DynamoDB — Admin |
| `GET` | `/health` | Health check |

---

## Persistência Poliglota

### SQL Server (principal)
- Jogos, compras, promoções e biblioteca do usuário
- Entity Framework Core 8 com migrations automáticas

### Redis (cache)
- Chave: `fcg:games:all`
- TTL: 5 minutos
- Invalidado automaticamente em `POST`, `PUT` e `DELETE` de jogos
- Logs: `[Cache HIT]`, `[Cache MISS]`, `[Cache SET]`, `[Cache INVALIDADO]`

### DynamoDB (audit log)
- Tabela: `fcg-audit-logs`
- Registra toda criação, alteração e remoção de jogos com payload JSON
- Driver: `AWSSDK.DynamoDBv2`

### Elasticsearch / OpenSearch (busca)
- Índice: `fcg-games`
- Query: MultiMatch com fuzziness `AUTO`, boost no campo `title`
- Campos indexados: `title` (x3), `description`, `genre`, `developer`
- Indexação automática a cada `POST`/`PUT`; remoção a cada `DELETE`

---

## Segurança

- Autenticação JWT Bearer em todos os endpoints
- Endpoints de escrita requerem role `Admin`
- Comunicação interna via header `x-internal-api-key`

---

## Observabilidade

- **Serilog** — logs estruturados em JSON
- **Datadog** — APM, traces e métricas
- **Correlation ID** — rastreabilidade ponta a ponta
- **Logs de cache** visíveis no Datadog e no console

---

## Arquitetura em camadas

```text
CatalogAPI/          → controllers, middleware, Program.cs
Core/                → entidades, interfaces, DTOs, inputs
Infrastructure/      → repositórios EF, GameService, ElasticsearchSearchService,
                       DynamoDbAuditLogRepository, SqsIntegrationEventPublisher
Tests/               → unit tests (GameService) + integração (health, smoke)
```

---

## Testes

```bash
dotnet test catalog-api_fase4/FCGCatalogAPI.sln
```

Testes incluídos:
- `GameServiceTests` — cache hit, cache miss, invalidação em CRUD, indexação, audit log
- `HealthCheckTests` — `GET /health` retorna 200
- `SmokeTests` — Swagger acessível

---

## Configuração local

### appsettings.Development.json

```json
{
  "Jwt": { "Key": "..." },
  "InternalApi": { "ApiKey": "..." },
  "Redis": { "ConnectionString": "localhost:6379" },
  "Elasticsearch": { "Url": "http://localhost:9200" },
  "DynamoDB": { "TableName": "fcg-audit-logs" }
}
```

### Docker Compose (pasta `observability`)

```bash
docker compose up --build
```

Sobe SQL Server, Redis, Elasticsearch, Datadog Agent e as 3 APIs.
