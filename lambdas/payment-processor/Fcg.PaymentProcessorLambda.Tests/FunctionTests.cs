using Amazon.Lambda.SQSEvents;
using System.Text.Json;
using Xunit;

namespace Fcg.PaymentProcessorLambda.Tests;

public class FunctionTests
{
    [Fact]
    public async Task Deve_processar_pagamento_localmente()
    {
        Environment.SetEnvironmentVariable("AWS_REGION", "us-east-1");
        Environment.SetEnvironmentVariable("PAYMENTS_API_BASE_URL", "https://localhost:53782");
        Environment.SetEnvironmentVariable("CATALOG_API_BASE_URL", "http://localhost:5009");
        Environment.SetEnvironmentVariable("INTERNAL_API_KEY", "fcg_internal_key_dev_2026");
        Environment.SetEnvironmentVariable("NOTIFICATIONS_QUEUE_URL", "https://sqs.us-east-1.amazonaws.com/123456789012/fcg-notifications");
        Environment.SetEnvironmentVariable("HTTP_TIMEOUT_SECONDS", "30");

        var function = new Fcg.PaymentProcessorLambda.Function();

        var purchaseEvent = new
        {
            eventType = "PurchaseCreated",
            source = "catalog-api",
            correlationId = Guid.NewGuid().ToString(),
            purchaseId = 1,
            userId = 1,
            gameId = 1,
            price = 150.00m
        };

        var sqsEvent = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new SQSEvent.SQSMessage
                {
                    MessageId = Guid.NewGuid().ToString(),
                    Body = JsonSerializer.Serialize(purchaseEvent)
                }
            }
        };

        var result = await function.FunctionHandler(sqsEvent, new TestLambdaContext());

        Assert.NotNull(result);
    }
}