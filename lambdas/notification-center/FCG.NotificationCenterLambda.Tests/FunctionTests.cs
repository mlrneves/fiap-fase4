using Amazon.Lambda.SQSEvents;
using System.Text.Json;
using Xunit;

namespace FCG.NotificationCenterLambda.Tests;

public class FunctionTests
{
    [Fact]
    public async Task Deve_processar_evento_UserRegistered_localmente()
    {
        var function = new FCG.NotificationCenterLambda.Function();

        var notificationEvent = new
        {
            eventType = "UserRegistered",
            source = "users-api",
            correlationId = Guid.NewGuid().ToString(),
            userId = 1,
            name = "Fabiana",
            email = "fabiana@email.com",
            role = "Customer",
            createdAt = DateTime.UtcNow
        };

        var sqsEvent = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new SQSEvent.SQSMessage
                {
                    MessageId = Guid.NewGuid().ToString(),
                    Body = JsonSerializer.Serialize(notificationEvent)
                }
            }
        };

        var result = await function.FunctionHandler(sqsEvent, new TestLambdaContext());

        Assert.NotNull(result);
        Assert.Empty(result.BatchItemFailures);
    }

    [Fact]
    public async Task Deve_processar_evento_PaymentProcessed_localmente()
    {
        var function = new FCG.NotificationCenterLambda.Function();

        var notificationEvent = new
        {
            eventType = "PaymentProcessed",
            source = "payment-processor-lambda",
            correlationId = Guid.NewGuid().ToString(),
            purchaseId = 1,
            userId = 1,
            gameId = 1,
            amount = 150.00m,
            status = "Approved",
            transactionId = "TXN-123",
            processedAt = DateTime.UtcNow
        };

        var sqsEvent = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new SQSEvent.SQSMessage
                {
                    MessageId = Guid.NewGuid().ToString(),
                    Body = JsonSerializer.Serialize(notificationEvent)
                }
            }
        };

        var result = await function.FunctionHandler(sqsEvent, new TestLambdaContext());

        Assert.NotNull(result);
        Assert.Empty(result.BatchItemFailures);
    }
}