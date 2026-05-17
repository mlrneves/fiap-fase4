using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using Fcg.PaymentProcessorLambda.Models;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Fcg.PaymentProcessorLambda;

public class Function
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    private static readonly HttpClient HttpClient = CreateHttpClient();
    private static readonly IAmazonSQS SqsClient = CreateSqsClient();

    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        var failures = new List<SQSBatchResponse.BatchItemFailure>();

        if (sqsEvent.Records is null || sqsEvent.Records.Count == 0)
        {
            context.Logger.LogInformation("Nenhuma mensagem recebida da SQS.");
            return new SQSBatchResponse(failures);
        }

        foreach (var record in sqsEvent.Records)
        {
            try
            {
                await ProcessRecordAsync(record, context);
            }
            catch (Exception ex)
            {
                context.Logger.LogError(
                    $"Erro ao processar mensagem {record.MessageId}. Ela será reenfileirada. Erro: {ex}");

                failures.Add(new SQSBatchResponse.BatchItemFailure
                {
                    ItemIdentifier = record.MessageId
                });
            }
        }

        return new SQSBatchResponse(failures);
    }

    private static async Task ProcessRecordAsync(SQSEvent.SQSMessage record, ILambdaContext context)
    {
        if (string.IsNullOrWhiteSpace(record.Body))
            throw new InvalidOperationException("Body da mensagem está vazio.");

        var purchaseEvent = JsonSerializer.Deserialize<PurchaseCreatedEvent>(record.Body, JsonOptions)
                            ?? throw new InvalidOperationException("Não foi possível desserializar PurchaseCreatedEvent.");

        ValidateEnvironmentVariables();

        var correlationId = string.IsNullOrWhiteSpace(purchaseEvent.CorrelationId)
            ? Guid.NewGuid().ToString("D")
            : purchaseEvent.CorrelationId;

        context.Logger.LogInformation(
            $"Iniciando processamento da compra PurchaseId={purchaseEvent.PurchaseId} UserId={purchaseEvent.UserId} GameId={purchaseEvent.GameId} CorrelationId={correlationId}");

        var paymentResponse = await CallPaymentsApiAsync(purchaseEvent, correlationId, context);
        await CallCatalogApiAsync(paymentResponse, correlationId, context);
        await PublishNotificationAsync(purchaseEvent, paymentResponse, correlationId, context);

        context.Logger.LogInformation(
            $"Compra processada com sucesso. PurchaseId={purchaseEvent.PurchaseId} PaymentId={paymentResponse.PaymentId} Status={paymentResponse.Status} CorrelationId={correlationId}");
    }

    private static async Task<PaymentProcessResponse> CallPaymentsApiAsync(
        PurchaseCreatedEvent purchaseEvent,
        string correlationId,
        ILambdaContext context)
    {
        var requestBody = new PaymentProcessRequest
        {
            PurchaseId = purchaseEvent.PurchaseId,
            UserId = purchaseEvent.UserId,
            GameId = purchaseEvent.GameId,
            Amount = purchaseEvent.Price,
            CorrelationId = correlationId
        };

        var endpoint = CombineUrl(
            GetRequiredEnvironmentVariable("PAYMENTS_API_BASE_URL"),
            "/api/payments/internal/process");

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = CreateJsonContent(requestBody)
        };

        AddInternalHeaders(request, correlationId);

        using var response = await HttpClient.SendAsync(request);
        var payload = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"PaymentsAPI retornou {(int)response.StatusCode} ({response.StatusCode}). Body: {payload}");
        }

        var result = JsonSerializer.Deserialize<PaymentProcessResponse>(payload, JsonOptions)
                     ?? throw new InvalidOperationException("Resposta do PaymentsAPI inválida.");

        context.Logger.LogInformation(
            $"PaymentsAPI respondeu com sucesso. PurchaseId={result.PurchaseId} PaymentId={result.PaymentId} Status={result.Status} CorrelationId={correlationId}");

        return result;
    }

    private static async Task CallCatalogApiAsync(
        PaymentProcessResponse paymentResponse,
        string correlationId,
        ILambdaContext context)
    {
        var requestBody = new CatalogPaymentResultRequest
        {
            PurchaseId = paymentResponse.PurchaseId,
            Status = paymentResponse.Status,
            ProcessedAt = paymentResponse.ProcessedAt == default
                ? DateTime.UtcNow
                : paymentResponse.ProcessedAt,
            CorrelationId = correlationId
        };

        var endpoint = CombineUrl(
            GetRequiredEnvironmentVariable("CATALOG_API_BASE_URL"),
            "/api/internal/purchases/payment-result");

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = CreateJsonContent(requestBody)
        };

        AddInternalHeaders(request, correlationId);

        using var response = await HttpClient.SendAsync(request);
        var payload = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"CatalogAPI retornou {(int)response.StatusCode} ({response.StatusCode}). Body: {payload}");
        }

        context.Logger.LogInformation(
            $"CatalogAPI atualizado com sucesso. PurchaseId={requestBody.PurchaseId} Status={requestBody.Status} CorrelationId={correlationId}");
    }

    private static async Task PublishNotificationAsync(
        PurchaseCreatedEvent purchaseEvent,
        PaymentProcessResponse paymentResponse,
        string correlationId,
        ILambdaContext context)
    {
        var queueUrl = GetRequiredEnvironmentVariable("NOTIFICATIONS_QUEUE_URL");

        var notificationEvent = new PaymentProcessedEvent
        {
            CorrelationId = correlationId,
            PurchaseId = paymentResponse.PurchaseId,
            UserId = paymentResponse.UserId,
            GameId = purchaseEvent.GameId,
            Amount = paymentResponse.Amount,
            Status = paymentResponse.Status,
            TransactionId = paymentResponse.TransactionId,
            ProcessedAt = paymentResponse.ProcessedAt == default
                ? DateTime.UtcNow
                : paymentResponse.ProcessedAt
        };

        var body = JsonSerializer.Serialize(notificationEvent, JsonOptions);

        var request = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = body,
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                ["eventType"] = new()
                {
                    DataType = "String",
                    StringValue = notificationEvent.EventType
                },
                ["source"] = new()
                {
                    DataType = "String",
                    StringValue = notificationEvent.Source
                },
                ["correlationId"] = new()
                {
                    DataType = "String",
                    StringValue = correlationId
                }
            }
        };

        var response = await SqsClient.SendMessageAsync(request);

        context.Logger.LogInformation(
            $"Evento PaymentProcessed publicado na fila de notificações. PurchaseId={notificationEvent.PurchaseId} MessageId={response.MessageId} CorrelationId={correlationId}");
    }

    private static void AddInternalHeaders(HttpRequestMessage request, string correlationId)
    {
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("x-internal-api-key", GetRequiredEnvironmentVariable("INTERNAL_API_KEY"));
        request.Headers.Add("x-correlation-id", correlationId);
    }

    private static StringContent CreateJsonContent<T>(T value)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static HttpClient CreateHttpClient()
    {
        var timeoutSeconds = 30;

        if (int.TryParse(Environment.GetEnvironmentVariable("HTTP_TIMEOUT_SECONDS"), out var parsed)
            && parsed > 0)
        {
            timeoutSeconds = parsed;
        }

        return new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };
    }

    private static IAmazonSQS CreateSqsClient()
    {
        var region = GetRequiredEnvironmentVariable("AWS_REGION");
        return new AmazonSQSClient(Amazon.RegionEndpoint.GetBySystemName(region));
    }

    private static void ValidateEnvironmentVariables()
    {
        _ = GetRequiredEnvironmentVariable("AWS_REGION");
        _ = GetRequiredEnvironmentVariable("PAYMENTS_API_BASE_URL");
        _ = GetRequiredEnvironmentVariable("CATALOG_API_BASE_URL");
        _ = GetRequiredEnvironmentVariable("INTERNAL_API_KEY");
        _ = GetRequiredEnvironmentVariable("NOTIFICATIONS_QUEUE_URL");
    }

    private static string GetRequiredEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"A variável de ambiente '{name}' não foi configurada.");

        return value;
    }

    private static string CombineUrl(string baseUrl, string relativePath)
    {
        return $"{baseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";
    }
}