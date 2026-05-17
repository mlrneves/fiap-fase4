🚀 GatewayAPI com YARP

Este projeto implementa um API Gateway em ASP.NET Core utilizando YARP (Yet Another Reverse Proxy) para centralizar o acesso aos microsserviços da aplicação.

🎯 Objetivo
Centralizar a entrada das requisições
Abstrair a comunicação com os microsserviços
Evitar exposição direta dos serviços internos
Simplificar roteamento e manutenção
Facilitar observabilidade e segurança
🧱 Arquitetura
Cliente (Browser / Postman)
        ↓
GatewayAPI (YARP - porta 5000)
        ↓
---------------------------------
| UsersAPI     (users-api:8080) |
| CatalogAPI   (catalog-api:8080) |
| PaymentsAPI  (payments-api:8080) |
---------------------------------
🔀 Rotas do Gateway
Rota Gateway	Serviço Destino
/users/*	UsersAPI
/games/*	CatalogAPI
/payments/*	PaymentsAPI
⚙️ Funcionalidades

✔ Roteamento baseado em path
✔ Remoção automática de prefixo (/users, /games, /payments)
✔ Swagger centralizado
✔ Endpoint de health check (/health)
✔ CORS habilitado para testes
✔ Pronto para Docker
✔ Compatível com AWS / EC2

🔁 Funcionamento
Requisição no Gateway:
POST /users/api/Auth/login
Encaminhado para:
http://users-api:8080/api/Auth/login
📚 Swagger Unificado (Diferencial 🚀)

O Gateway expõe um Swagger centralizado, permitindo escolher qual microsserviço visualizar através de um dropdown (combo).

Acesso:
http://SEU-IP:5000/swagger/index.html
Serviços disponíveis no combo:
GatewayAPI v1
UsersAPI v1
CatalogAPI v1
PaymentsAPI v1
Como funciona

O Swagger do Gateway consome os endpoints dos serviços via YARP:

/users/swagger/v1/swagger.json
/games/swagger/v1/swagger.json
/payments/swagger/v1/swagger.json

👉 Isso permite navegar entre os serviços sem acessar cada API individualmente.

❤️ Vantagens do Swagger Unificado
Interface única para todos os serviços
Evita múltiplas URLs
Facilita testes e demonstração
Simula comportamento de API Gateway corporativo
🐳 Execução com Docker Compose

O GatewayAPI faz parte do Docker Compose principal da solução, sendo executado junto com os demais serviços.

Serviços do ambiente
SQL Server
Datadog Agent
UsersAPI
CatalogAPI
PaymentsAPI
GatewayAPI
📦 Arquivo principal
docker-compose.aws.yml
🌐 Acesso externo

Após subir o ambiente:

http://SEU-IP:5000
🔗 Rotas disponíveis
http://SEU-IP:5000/users/swagger/index.html
http://SEU-IP:5000/users/api/Auth/login

http://SEU-IP:5000/games/swagger/index.html
http://SEU-IP:5000/payments/swagger/index.html

http://SEU-IP:5000/swagger/index.html
⚙️ Comunicação interna

O Gateway utiliza os nomes dos serviços definidos no Docker Compose:

users-api:8080
catalog-api:8080
payments-api:8080

⚠️ Caso os nomes mudem no docker-compose, atualize também:

appsettings.json
🔐 Segurança (base)
Microsserviços não expostos diretamente
Comunicação via rede interna Docker
Entrada centralizada pelo Gateway
Possíveis evoluções:
JWT validado no Gateway
Rate limiting
API Key
Logging centralizado
Integração com AWS API Gateway
⚠️ Observações importantes
O Swagger dos serviços foi ajustado para funcionar atrás do Gateway
Uso de headers x-forwarded-* para compatibilidade
O Gateway remove prefixos de rota automaticamente
🚀 Execução
Subir ambiente completo:
docker compose -f docker-compose.aws.yml --env-file .env up -d
Verificar containers:
docker ps
Health check:
http://SEU-IP:5000/health
🧠 Arquitetura adotada
API Gateway implementado com YARP
Microsserviços independentes
Comunicação interna via Docker network
Gateway como ponto único de entrada