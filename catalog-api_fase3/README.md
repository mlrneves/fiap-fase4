# Catalog API - FCG Fase 3

## 📌 Descrição

A **Catalog API** é um microserviço responsável pela gestão do catálogo de produtos do sistema.

Este serviço faz parte da arquitetura distribuída desenvolvida para o Desafio da Fase 3, utilizando práticas modernas de microsserviços, observabilidade e integração com serviços AWS.

---

## 🚀 Responsabilidades

* Cadastro de produtos
* Consulta de catálogo
* Atualização e remoção de itens
* Exposição de endpoints para consumo por outros serviços
* Integração com outros microserviços do ecossistema

---

## 🔐 Segurança

* Proteção de endpoints sensíveis
* Integração com autenticação via API Gateway

---

## 📡 Observabilidade

O serviço possui integração com:

* **Serilog** para logs estruturados
* **Datadog** para monitoramento, logs e traces
* Middleware de **Correlation ID** para rastreabilidade

---

## 📊 Auditoria

Registros relevantes podem ser auditados conforme as operações realizadas no catálogo.

---

## ☁️ Integração com AWS

* Preparado para execução em ambiente cloud (AWS)
* Comunicação com outros serviços via arquitetura distribuída

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

* Teste de Health Check
* Teste de inicialização da aplicação (smoke test)

Executar localmente:

dotnet test

---

## 🏗️ Arquitetura

O projeto segue uma estrutura em camadas:

* **CatalogAPI** → camada de apresentação (controllers)
* **Core** → regras de negócio
* **Infrastructure** → acesso a dados e integrações externas
* **Tests** → testes automatizados

---

## 📌 Observação

A configuração de infraestrutura (AWS, API Gateway, deploy e orquestração) é realizada externamente a este repositório.

---

## 👩‍💻 Projeto acadêmico

Este projeto foi desenvolvido como parte do desafio da pós-graduação, com foco em:

* Microsserviços
* Cloud (AWS)
* Observabilidade
* CI/CD
* Arquitetura distribuída

---
