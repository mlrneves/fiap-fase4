# README - Payment Processor Lambda

## Visão geral
A **Payment Processor Lambda** é a função serverless responsável por consumir eventos de compra de uma fila SQS, processar o pagamento de forma assíncrona, atualizar o status da compra no catálogo e publicar um novo evento para continuidade do fluxo.

Ela atua como um componente intermediário entre a criação da compra e a etapa de notificação ao usuário.

## Objetivo
Desacoplar o processamento de pagamento do fluxo síncrono da aplicação principal, permitindo que a compra seja tratada de forma assíncrona, orientada a eventos e com retry automático em caso de falha.

## Como funciona
1. Um evento de compra chega na fila **fcg-purchase-created**.
2. A Lambda é acionada automaticamente por um **trigger SQS**.
3. O `FunctionHandler` desserializa a mensagem recebida para `PurchaseCreatedEvent`.
4. A função chama o `PaymentsAPI` para processar o pagamento.
5. Em seguida, chama o `CatalogAPI` para registrar o resultado do pagamento.
6. Depois publica um evento `PaymentProcessed` na fila **fcg-notifications**.
7. Se alguma etapa falhar, a função retorna falha parcial do lote com `ReportBatchItemFailures`, permitindo retry apenas do item com erro.

## Trigger / acionamento automático
A Lambda é acionada automaticamente por uma fila **Amazon SQS** (`fcg-purchase-created`).

A associação entre a fila e a Lambda pode ser configurada utilizando **AWS CLI**, por meio da criação de um *event source mapping*. Dessa forma, sempre que uma nova mensagem chega na fila, a função é executada automaticamente.

O acionamento automático **não é configurado no código C#**, mas sim na infraestrutura/configuração do recurso na AWS.

## Deploy
O deploy da Lambda pode ser realizado utilizando a **AWS CLI (CloudShell)**, atendendo ao requisito do desafio que permite o uso da CLI da cloud escolhida.

Etapas principais:
- compilação do projeto com `dotnet publish`
- empacotamento dos artefatos em um arquivo `.zip`
- criação da função com `aws lambda create-function`
- atualização posterior com `aws lambda update-function-code`

## Fluxo de comunicação

### Entrada
Recebe da fila SQS um evento do fluxo de compra.

### Processamento interno
Durante o processamento, a Lambda se comunica com:
- `PaymentsAPI` via HTTP
- `CatalogAPI` via HTTP

### Saída
Ao final, a Lambda publica um evento na fila:
- `fcg-notifications`

## Estrutura principal
- `Function.cs`: orquestra todo o fluxo da Lambda
- `Models/PurchaseCreatedEvent.cs`: contrato do evento de entrada
- `Models/PaymentProcessRequest.cs`: payload enviado ao `PaymentsAPI`
- `Models/PaymentProcessResponse.cs`: resposta do `PaymentsAPI`
- `Models/CatalogPaymentResultRequest.cs`: payload enviado ao `CatalogAPI`
- `Models/PaymentProcessedEvent.cs`: evento publicado na fila de notificações
- `template.yaml`: definição da infraestrutura da Lambda e da fila de entrada

## Particularidades desta Lambda
- opera de forma assíncrona
- é acionada automaticamente por SQS
- integra dois microsserviços internos
- publica evento de saída em outra fila SQS
- usa `ReportBatchItemFailures` para retry apenas dos itens que falharem
- propaga `x-internal-api-key` e `x-correlation-id` nas chamadas internas
- reutiliza `HttpClient` e `AmazonSQSClient` em invocações quentes

## Variáveis de ambiente usadas pelo código

### Obrigatórias
- `AWS_REGION`
- `PAYMENTS_API_BASE_URL`
- `CATALOG_API_BASE_URL`
- `INTERNAL_API_KEY`
- `NOTIFICATIONS_QUEUE_URL`

### Opcional
- `HTTP_TIMEOUT_SECONDS`

## O que precisa de configuração
Para funcionar na AWS, é necessário configurar:
- a fila SQS de entrada `fcg-purchase-created`
- a fila SQS de saída `fcg-notifications`
- o trigger SQS da fila de entrada
- a role IAM da Lambda
- as variáveis de ambiente com as URLs internas e chave de integração
- permissão para envio de mensagens para a fila de notificações

## Permissões necessárias
Além da permissão básica de execução da Lambda, esta função precisa de:
- permissão para consumir mensagens da fila `fcg-purchase-created`
- permissão para enviar mensagens para a fila `fcg-notifications`

## O que não precisa ser configurado no código
- o trigger da fila SQS
- a associação entre fila e Lambda
- a produção do evento inicial de compra
- a configuração de segurança dos microsserviços além dos headers exigidos

Esses pontos são configurados na infraestrutura ou diretamente na AWS.

## Comunicação com outros componentes
Esta Lambda integra o fluxo entre:
- origem do evento de compra
- `PaymentsAPI`, para processar o pagamento
- `CatalogAPI`, para atualizar o status da compra
- `notification-center`, por meio da fila `fcg-notifications`

## Dependências
Pacotes principais:
- `Amazon.Lambda.Core`
- `Amazon.Lambda.Serialization.SystemTextJson`
- `Amazon.Lambda.SQSEvents`
- `AWSSDK.SQS`

Recursos AWS utilizados:
- AWS Lambda
- Amazon SQS
- CloudWatch Logs
- IAM

## Limitações atuais
- depende da disponibilidade do `PaymentsAPI` e do `CatalogAPI`
- se um endpoint interno falhar, a mensagem volta para retry
- não há implementação explícita de DLQ, idempotência ou circuit breaker
- a política de `sqs:SendMessage` deve ser restrita à ARN da fila de notificações
- esta versão do código não ignora certificados inválidos, o que é mais adequado para publicação em ambiente real

## Aderência ao desafio da fase 3
Esta Lambda atende ao requisito de **Serverless com trigger automático**, pois:
- é uma AWS Lambda real
- é acionada automaticamente por SQS
- processa eventos de forma assíncrona
- integra microsserviços internos
- publica um novo evento para continuidade do fluxo

Além disso, o deploy pode ser realizado utilizando **AWS CLI**, atendendo ao requisito obrigatório de uso de ferramenta de deploy compatível com a cloud escolhida.
