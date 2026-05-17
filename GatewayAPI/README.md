# Gateway API — FCG Fase 4

API Gateway da plataforma FIAP Cloud Games, implementado com **YARP (Yet Another Reverse Proxy)** em ASP.NET Core 8.

---

## Objetivo

- Centralizar o ponto de entrada das requisições
- Rotear chamadas para os microsserviços corretos
- Expor a aplicação via Load Balancer no Kubernetes (EKS)
- Evitar exposição direta dos serviços internos

---

## Arquitetura

```
Cliente / Swagger
        ↓
Load Balancer (EKS)
        ↓
GatewayAPI (YARP — porta 8080)
        ↓
┌─────────────────────────────────────┐
│ users-api:8080   (Users API)        │
│ catalog-api:8080 (Catalog API)      │
│ payments-api:8080 (Payments API)    │
└─────────────────────────────────────┘
```

---

## Rotas configuradas

| Rota no Gateway | Serviço de destino |
|---|---|
| `/users/*` | `users-api:8080` |
| `/games/*` | `catalog-api:8080` |
| `/payments/*` | `payments-api:8080` |

O prefixo de rota é removido automaticamente pelo YARP antes de encaminhar.

**Exemplo:**
```
POST /users/api/auth/login
  → encaminhado para →
http://users-api:8080/api/auth/login
```

---

## Swagger Unificado

O Gateway expõe um Swagger centralizado com dropdown para navegar entre os serviços:

```
http://SEU-ENDPOINT/swagger/index.html
```

Serviços disponíveis:
- GatewayAPI v1
- UsersAPI v1
- CatalogAPI v1
- PaymentsAPI v1

Os JSONs são consumidos via YARP:
- `/users/swagger/v1/swagger.json`
- `/games/swagger/v1/swagger.json`
- `/payments/swagger/v1/swagger.json`

---

## Kubernetes (EKS)

O Gateway é exposto externamente via `Service` do tipo `LoadBalancer`:

```yaml
# k8s/gateway-api/service.yaml
type: LoadBalancer
port: 80 → 8080
```

Os demais serviços (`users-api`, `catalog-api`, `payments-api`) são do tipo `ClusterIP` — acessíveis apenas internamente.

---

## Configuração YARP

As rotas são configuradas em dois lugares:

- **Desenvolvimento local**: `appsettings.json` e `appsettings.Development.json`
- **Kubernetes**: `k8s/configmaps/gateway-yarp-config.yaml` (montado como volume no pod)

---

## Execução local

### Via Docker Compose (pasta `observability`)

```bash
docker compose -f docker-compose.aws.yml --env-file .env up -d
```

Acesso:
```
http://localhost:5000/swagger/index.html
http://localhost:5000/users/api/auth/login
http://localhost:5000/games/api/games/search?q=zelda
```

### Health check

```
GET /health
```

---

## Segurança

- Microsserviços não expostos diretamente ao exterior
- Comunicação via rede interna Docker / Kubernetes
- Entrada centralizada pelo Gateway
- JWT validado nos microsserviços (não no Gateway)
