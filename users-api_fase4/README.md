# Users API — FCG Fase 4

A **Users API** é o microsserviço responsável pela gestão de usuários, autenticação e autorização da plataforma FIAP Cloud Games.

---

## Responsabilidades

- Cadastro de usuários com validação de e-mail e senha
- Autenticação via JWT (tokens com expiração configurável)
- Autorização baseada em roles: `User` e `Admin`
- Publicação de eventos de registro na fila SQS (`fcg-notifications`)
- Audit log de ações no banco de dados

---

## Endpoints principais

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/auth/login` | Autenticação — retorna JWT |
| `GET` | `/api/users` | Lista usuários — Admin |
| `GET` | `/api/users/{id}` | Busca usuário por ID — Admin |
| `POST` | `/api/users` | Cria usuário — Admin |
| `PUT` | `/api/users` | Atualiza usuário — Admin |
| `DELETE` | `/api/users/{id}` | Remove usuário — Admin |
| `GET` | `/health` | Health check |

---

## Segurança

- JWT Bearer com `HmacSha256`
- Roles: `User` e `Admin`
- Senhas validadas: mínimo 8 caracteres, letras + números + caractere especial
- E-mail validado no cadastro (formato e unicidade)

---

## Observabilidade

- **Serilog** — logs estruturados em JSON
- **Datadog** — APM, traces e métricas
- **Correlation ID** — rastreabilidade ponta a ponta

---

## Arquitetura em camadas

```text
UsersAPI/        → controllers, middleware, Program.cs
Core/            → entidades, interfaces, DTOs, inputs, eventos
Infrastructure/  → UserRepository (EF Core + SQL Server), UserService,
                   AuthService (JWT), SqsIntegrationEventPublisher
Tests/           → unit tests (UserService) + integração (health, smoke)
```

---

## Testes

```bash
dotnet test users-api_fase4/FCGUsersAPI.sln
```

Testes incluídos:
- `UserServiceTests` — e-mail inválido, senha fraca, e-mail duplicado, cadastro válido + evento SQS
- `HealthCheckTests` — `GET /health` retorna 200
- `SmokeTests` — Swagger acessível

---

## Configuração local

### appsettings.Development.json

```json
{
  "Jwt": { "Key": "..." },
  "AdminUser": { "Password": "..." },
  "InternalApi": { "ApiKey": "..." },
  "Aws": {
    "Sqs": {
      "Region": "us-east-1",
      "NotificationsQueueUrl": "https://sqs..."
    }
  }
}
```

Em produção, todos os valores sensíveis chegam via **K8s Secrets** injetados como variáveis de ambiente.
