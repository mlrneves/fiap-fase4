# Payments API — FCG Fase 4

A **Payments API** é o microsserviço responsável pelo processamento de pagamentos da plataforma FIAP Cloud Games.

---

## Responsabilidades

- Processamento de pagamentos (simulado: 80% aprovação / 20% rejeição)
- Registro de transações no SQL Server
- Consulta de pagamentos por ID e por compra
- Exposição de endpoint interno para a Lambda `payment-processor`

---

## Endpoints principais

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/payments` | Lista pagamentos — Autenticado |
| `GET` | `/api/payments/{id}` | Busca por ID — Autenticado |
| `GET` | `/api/payments/purchase/{purchaseId}` | Busca por compra — Autenticado |
| `POST` | `/api/payments/process` | Processa pagamento — Autenticado |
| `POST` | `/api/payments/internal/process` | Endpoint interno (Lambda) — `x-internal-api-key` |
| `GET` | `/health` | Health check |

---

## Fluxo de pagamento

```
CatalogAPI → SQS (fcg-purchase-created)
    → Lambda payment-processor
        → POST /api/payments/internal/process
            → Aprovado (80%) ou Rejeitado (20%)
        → CatalogAPI (atualiza status da compra)
        → SQS (fcg-notifications)
            → Lambda notification-center → CloudWatch
```

---

## Segurança

- JWT Bearer para endpoints externos
- Header `x-internal-api-key` para o endpoint da Lambda

---

## Observabilidade

- **Serilog** — logs estruturados em JSON
- **Datadog** — APM, traces e métricas
- **Correlation ID** — rastreabilidade ponta a ponta

---

## Arquitetura em camadas

```text
PaymentsAPI/     → controllers, middleware, Program.cs
Core/            → entidades, interfaces, DTOs, inputs, eventos
Infrastructure/  → PaymentRepository (EF Core + SQL Server), PaymentService
Tests/           → unit tests (PaymentService) + integração (health, smoke)
```

---

## Testes

```bash
dotnet test payments-api_fase4/FCGPaymentsAPI.sln
```

Testes incluídos:
- `PaymentServiceTests` — input nulo, amount zero, purchaseId inválido, retorno Approved/Rejected, persistência
- `HealthCheckTests` — `GET /health` retorna 200
- `SmokeTests` — Swagger acessível

---

## Configuração local

### appsettings.Development.json

```json
{
  "Jwt": { "Key": "..." },
  "InternalApi": { "ApiKey": "..." }
}
```

Em produção, todos os valores sensíveis chegam via **K8s Secrets** injetados como variáveis de ambiente.
