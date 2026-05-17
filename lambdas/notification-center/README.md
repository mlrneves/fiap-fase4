# README - Notification Center Lambda

## Visão geral
A **Notification Center Lambda** é a função serverless responsável por consumir eventos de notificação da fila SQS e simular o envio de notificações por meio de logs no CloudWatch.

Nesta implementação, ela **não integra com provedor real de e-mail, SMS ou push**. O objetivo é demonstrar corretamente o fluxo assíncrono com AWS Lambda + SQS, atendendo ao requisito de trigger automática do desafio.

## Objetivo
Centralizar o recebimento de eventos de notificação e desacoplar essa responsabilidade dos microsserviços principais.

Atualmente, a Lambda trata principalmente os eventos:
- `UserRegistered`
- `PaymentProcessed`

## Como funciona
1. Uma mensagem chega na fila **fcg-notifications**.
2. A Lambda é acionada automaticamente por um **trigger SQS**.
3. O `FunctionHandler` desserializa o payload recebido para `NotificationEnvelope`.
4. O `NotificationProcessor` identifica o `EventType` e monta uma mensagem simples.
5. O `NotificationSink` registra essa mensagem no log.
6. Se uma mensagem do lote falhar, a Lambda retorna falha parcial com `ReportBatchItemFailures`, permitindo retry apenas do item com erro.

## Trigger / acionamento automático
A Lambda é acionada automaticamente por uma fila **Amazon SQS** (`fcg-notifications`).

A integração entre a fila e a Lambda foi configurada utilizando **AWS CLI**, por meio da criação de um *event source mapping*. Dessa forma, sempre que uma nova mensagem chega na fila, a função é executada automaticamente.

O acionamento automático **não é configurado no código C#**, mas sim na infraestrutura/configuração do recurso na AWS.

## Deploy

O deploy da Lambda foi realizado utilizando a **AWS CLI (CloudShell)**, atendendo ao requisito do desafio que permite o uso da CLI da cloud escolhida.

Etapas realizadas:
- O projeto foi compilado com `dotnet publish`
- Os artefatos foram empacotados em um arquivo `.zip`
- A função foi criada utilizando o comando:

```bash
aws lambda create-function ...
```

- Atualizações podem ser feitas com:

```bash
aws lambda update-function-code ...
```

Essa abordagem garante que o deploy foi realizado de forma automatizada via CLI, conforme exigido.

## Permissões e integração com SQS

Para permitir que a Lambda consuma mensagens da fila SQS, foi utilizada uma role IAM com a policy gerenciada:

- `AWSLambdaSQSQueueExecutionRole`

Essa policy garante permissões para:
- receber mensagens da fila (`ReceiveMessage`)
- deletar mensagens processadas (`DeleteMessage`)
- consultar atributos da fila (`GetQueueAttributes`)
- registrar logs no CloudWatch

Sem essa configuração, a Lambda não conseguiria ser acionada automaticamente pela fila.

## Estrutura principal
- `Function.cs`: ponto de entrada da Lambda
- `Models/NotificationEnvelope.cs`: modelo da mensagem recebida
- `Services/NotificationProcessor.cs`: interpreta o tipo do evento e monta a mensagem
- `Services/NotificationSink.cs`: simula a notificação por log
- `template.yaml`: define uma opção de infraestrutura para a solução

## Particularidades desta Lambda
- opera de forma assíncrona
- é acionada automaticamente por SQS
- diferencia o conteúdo conforme o tipo do evento
- implementa falha parcial por item do lote
- simula o envio de notificação por log no CloudWatch

## O que precisa de configuração
Para funcionar na AWS, é necessário:
- criar/publicar a Lambda
- criar ou utilizar a fila SQS de notificações
- associar a fila à Lambda como trigger
- configurar a role IAM com permissões adequadas
- garantir acesso ao CloudWatch Logs

## O que não precisa ser configurado no código
- o trigger da fila SQS
- a associação da fila com a Lambda
- a origem dos eventos publicados na fila

Esses pontos são configurados na infraestrutura ou diretamente na AWS.

## Comunicação com outros componentes
Esta Lambda recebe eventos produzidos por outros serviços, por exemplo:
- `payment-processor`, ao publicar `PaymentProcessed`
- outro produtor de eventos, ao publicar `UserRegistered`

Ela **não faz chamadas HTTP** para outros microsserviços neste fluxo.

## Dependências
Pacotes principais:
- `Amazon.Lambda.Core`
- `Amazon.Lambda.Serialization.SystemTextJson`
- `Amazon.Lambda.SQSEvents`

Recursos AWS utilizados:
- AWS Lambda
- Amazon SQS
- CloudWatch Logs

## Limitações atuais
- não envia notificações reais
- não integra com provedor externo
- depende de o payload já conter os dados necessários para montar a mensagem
- trata apenas alguns tipos de evento explicitamente

## Aderência ao desafio da fase 3
Esta Lambda atende ao requisito de **Serverless com trigger automático**, pois:

- é uma AWS Lambda real
- é acionada automaticamente por SQS
- processa eventos de forma assíncrona
- demonstra desacoplamento entre serviços
- trata falhas por item do lote

Além disso, o deploy foi realizado utilizando **AWS CLI**, atendendo ao requisito obrigatório de uso de ferramenta de deploy compatível com a cloud escolhida.
