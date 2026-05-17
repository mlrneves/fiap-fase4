using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using FCG.NotificationCenterLambda.Models;
using FCG.NotificationCenterLambda.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FCG.NotificationCenterLambda;

public class Function
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    private readonly NotificationProcessor _processor;

    public Function()
    {
        var sink = new NotificationSink();
        _processor = new NotificationProcessor(sink);
    }

    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        var failures = new List<SQSBatchResponse.BatchItemFailure>();

        if (evnt.Records is null || evnt.Records.Count == 0)
        {
            context.Logger.LogInformation("[NotificationCenter] Nenhuma mensagem recebida.");
            return new SQSBatchResponse(failures);
        }

        foreach (var message in evnt.Records)
        {
            try
            {
                context.Logger.LogInformation($"[NotificationCenter] Processing message {message.MessageId}");

                var envelope = JsonSerializer.Deserialize<NotificationEnvelope>(message.Body, JsonOptions)
                               ?? throw new InvalidOperationException("Message body could not be deserialized.");

                await _processor.ProcessAsync(envelope, CancellationToken.None);
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"[NotificationCenter] Failed to process message {message.MessageId}: {ex}");
                failures.Add(new SQSBatchResponse.BatchItemFailure
                {
                    ItemIdentifier = message.MessageId
                });
            }
        }

        return new SQSBatchResponse(failures);
    }
}