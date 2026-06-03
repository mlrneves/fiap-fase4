# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**FIAP Cloud Games (FCG) – Fase 4** is a cloud-native microservices platform built with C# 12 / .NET 8, deployed to AWS EKS. It demonstrates polyglot persistence, event-driven processing, and Kubernetes orchestration.

## Build & Test Commands

Each service has its own solution file. Run these from the repository root:

```bash
# Build a service
dotnet build ./catalog-api_fase4/FCGCatalogAPI.sln --configuration Release
dotnet build ./users-api_fase4/FCGUsersAPI.sln --configuration Release
dotnet build ./payments-api_fase4/FCGPaymentsAPI.sln --configuration Release
dotnet build ./GatewayAPI/GatewayAPI/GatewayAPI.csproj --configuration Release

# Run all tests for a service
dotnet test catalog-api_fase4/FCGCatalogAPI.sln --configuration Release
dotnet test users-api_fase4/FCGUsersAPI.sln --configuration Release
dotnet test payments-api_fase4/FCGPaymentsAPI.sln --configuration Release

# Run a single test class
dotnet test catalog-api_fase4/FCGCatalogAPI.sln --filter "FullyQualifiedName~GameServiceTests"

# Restore dependencies
dotnet restore ./catalog-api_fase4/FCGCatalogAPI.sln
```

## Local Development

The full local stack lives in `observability/`:

```bash
cd observability
cp .env.example .env   # fill in required values
docker compose up --build
```

Services and their local ports:
- Users API → `http://localhost:5001`
- Catalog API → `http://localhost:5002`
- Payments API → `http://localhost:5003`
- SQL Server 2022 → `localhost:1433`
- Redis 7 → `localhost:6379`
- Elasticsearch 8.13 → `localhost:9200`
- Datadog Agent → `localhost:8126`

Key environment variables (see `observability/.env`):
- `JWT_KEY`, `JWT_ISSUER` — auth token signing
- `INTERNAL_API_KEY` — shared secret for inter-service calls
- `PURCHASE_CREATED_QUEUE_URL`, `NOTIFICATIONS_QUEUE_URL` — AWS SQS queues
- `PAYMENTS_API_BASE_URL`, `CATALOG_API_BASE_URL` — Lambda-to-service routing

## Architecture

### Services

| Service | Port | Purpose |
|---------|------|---------|
| `GatewayAPI/` | 5000/8080 | YARP reverse proxy + aggregated Swagger UI |
| `users-api_fase4/` | 5001/8080 | User registration, JWT auth, role management |
| `catalog-api_fase4/` | 5002/8080 | Games CRUD, purchases, library, promotions |
| `payments-api_fase4/` | 5003/8080 | Payment processing and transaction recording |
| `lambdas/payment-processor/` | — | SQS-triggered payment orchestration |
| `lambdas/notification-center/` | — | SQS-triggered notification logging |

Each service (`users-api_fase4/`, `catalog-api_fase4/`, `payments-api_fase4/`) follows the same internal layout:
```
<ServiceName>/          ← Controllers, Program.cs
Core/                   ← Entities, interfaces, DTOs, domain events
Infrastructure/         ← Repositories, service implementations, EF Core
Tests/                  ← xUnit tests (unit + integration via CustomWebApplicationFactory)
Dockerfile              ← Multi-stage (sdk:8.0 → aspnet:8.0)
```

### Polyglot Persistence

- **SQL Server**: primary relational store for users, games, purchases, payments (EF Core, code-first migrations)
- **Redis**: read-through cache for the games catalog (5-minute TTL, invalidated on write)
- **DynamoDB**: append-only audit log for game create/update/delete events
- **OpenSearch (Amazon)**: full-text fuzzy search over the games catalog

### Event-Driven Flow

```
Catalog API (POST /purchases)
  → SQS: fcg-purchase-created
    → Lambda: payment-processor
      → Payments API (internal): /api/payments/internal/process
      → Catalog API (internal): update purchase status
      → SQS: fcg-notifications
        → Lambda: notification-center
          → CloudWatch logs
```

Lambdas use **per-item batch failure** (`SQSBatchResponse`) so a single failed message is retried without reprocessing the rest of the batch.

### Security

- JWT Bearer (HS256) for user-facing endpoints; roles are `User` and `Admin`.
- Internal service-to-service calls (Lambda → API) use the `x-internal-api-key` header — these endpoints are decorated `[ApiExplorerSettings(IgnoreApi = true)]` and not exposed through the Gateway.
- All secrets are injected via environment variables or Kubernetes Secrets; nothing sensitive is hardcoded.

### Observability

- **Serilog** with `RenderedCompactJsonFormatter` for structured JSON logs
- **Datadog APM** — DaemonSet in K8s, `DD_TRACE_*` env vars injected at pod level
- **Correlation ID** (`x-correlation-id`) propagated across all HTTP service calls
- `/health` endpoints on every service for Kubernetes readiness/liveness probes

## Kubernetes & CI/CD

### K8s Manifests (`k8s/`)

All workloads run in the `fcg` namespace. Deployment strategy is rolling update (`maxUnavailable=0`, `maxSurge=1`). The Gateway is exposed via a `LoadBalancer` service; all other services use `ClusterIP`.

Non-sensitive config lives in `k8s/configmaps/fcg-config.yaml`. Secrets (passwords, keys, connection strings) are in `k8s/secrets.yaml` (template — fill before applying).

### CI/CD Pipelines (`.github/workflows/`)

- **`ci-pr.yml`** — runs on PRs to `main`: restore → build → test (no image push)
- **`cicd-aws.yml`** — runs on push to `main`:
  1. Build all four projects (Release)
  2. Run all tests
  3. Login to ECR and push images tagged with `<short-sha>` and `latest`
  4. Trivy vulnerability scan (HIGH/CRITICAL, non-blocking)
  5. Apply K8s manifests and rolling-deploy all services (600 s timeout)

Image tags are set from the git short SHA (`${{ github.sha }}`) so each deploy is traceable to a specific commit.

## Testing Conventions

- Framework: **xUnit** + **FluentAssertions** + **Moq**
- Integration tests use a `CustomWebApplicationFactory` with `appsettings.Testing.json`
- `PaymentService` deliberately simulates an 80 % approval / 20 % rejection rate — tests account for this randomness via mocked dependencies
- Lambda functions have their own `Tests/` project alongside the handler project
