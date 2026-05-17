# Payments API - FCG Fase 3

## 📌 Descrição

A **Payments API** é um microserviço responsável pelo processamento e gerenciamento de pagamentos no sistema.

Este serviço faz parte da arquitetura distribuída desenvolvida para o Desafio da Fase 3, utilizando práticas modernas de microsserviços, observabilidade e integração com serviços AWS.

---

## 🚀 Responsabilidades

- Recebimento e processamento de pagamentos
- Registro de transações
- Atualização de status de pagamento
- Integração com outros microserviços do ecossistema
- Publicação e consumo de eventos em arquitetura distribuída

---

## 🔐 Segurança

- Proteção de endpoints sensíveis
- Integração com autenticação e controle de acesso no ambiente distribuído

---

## 📡 Observabilidade

O serviço possui integração com:

- **Serilog** para logs estruturados
- **Datadog** para monitoramento, logs e traces
- Middleware de **Correlation ID** para rastreabilidade ponta a ponta

---

## 📊 Auditoria

O serviço registra ações relevantes por meio de **Audit Log**, permitindo rastreabilidade e apoio à auditoria operacional.

---

## ☁️ Integração com AWS

- Preparado para execução em ambiente cloud (AWS)
- Integração com componentes distribuídos da solução
- Suporte a comunicação assíncrona quando aplicável

---

## ❤️ Health Check

Endpoint disponível para verificação de saúde do serviço:

GET /health

---

## 🐳 Containerização

O serviço é containerizado utilizando Docker, permitindo execução consistente em qualquer ambiente.

---

## 🧪 Testes

O projeto possui testes automatizados básicos para validação de funcionamento:

- Teste de Health Check
- Teste de inicialização da aplicação (smoke test)

Executar localmente:

dotnet test

---

## 🏗️ Arquitetura

O projeto segue uma estrutura em camadas:

- **PaymentsAPI** → camada de apresentação (controllers)
- **Core** → regras de negócio
- **Infrastructure** → acesso a dados e integrações externas
- **Tests** → testes automatizados

---

## 📌 Observação

A configuração de infraestrutura, deploy, API Gateway e orquestração é realizada externamente a este repositório.

---

## 👩‍💻 Projeto acadêmico

Este projeto foi desenvolvido como parte do desafio da pós-graduação, com foco em:

- Microsserviços
- Cloud (AWS)
- Observabilidade
- CI/CD
- Arquitetura distribuída
- Auditoria e rastreabilidade
