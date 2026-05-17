# Users API - FCG Fase 3

## 📌 Descrição

A **Users API** é um microserviço responsável pela gestão de usuários, autenticação e autorização dentro do sistema.

Este serviço faz parte da arquitetura distribuída desenvolvida para o Desafio da Fase 3, utilizando práticas modernas de microsserviços, observabilidade e integração com serviços AWS.

---

## 🚀 Responsabilidades

- Cadastro de usuários
- Autenticação via JWT
- Autorização baseada em perfis (ex: Admin)
- Publicação de eventos assíncronos (AWS SQS)
- Auditoria de ações dos usuários

---

## 🔐 Segurança

- Autenticação baseada em JWT
- Controle de acesso por roles
- Proteção de endpoints sensíveis

---

## 📡 Observabilidade

- Serilog para logs estruturados
- Datadog para monitoramento, logs e traces
- Correlation ID para rastreabilidade

---

## 📊 Auditoria

Registro de ações relevantes em banco de dados.

---

## ☁️ Integração com AWS

- Publicação de eventos via Amazon SQS

---

## ❤️ Health Check

GET /health

---

## 🐳 Containerização

Docker

---

## 🧪 Testes

dotnet test

---

## 🏗️ Arquitetura

- UsersAPI
- Core
- Infrastructure
- Tests

---

## 📌 Observação

Infraestrutura é configurada externamente.

---

## 👩‍💻 Projeto acadêmico

Microsserviços, AWS, CI/CD e observabilidade.
