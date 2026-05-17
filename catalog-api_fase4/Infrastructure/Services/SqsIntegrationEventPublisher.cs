using Amazon.SQS;
using Amazon.SQS.Model;
using Core.Events;
using Core.Services;
using Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class SqsIntegrationEventPublisher : IIntegrationEventPublisher
    {
        private readonly IAmazonSQS _amazonSqs;
        private readonly AwsSqsOptions _options;
        private readonly ILogger<SqsIntegrationEventPublisher> _logger;

        public SqsIntegrationEventPublisher(
            IAmazonSQS amazonSqs,
            IOptions<AwsSqsOptions> options,
            ILogger<SqsIntegrationEventPublisher> logger)
        {
            _amazonSqs = amazonSqs;
            _options = options.Value;
            _logger = logger;
        }

        public async Task PublishAsync(IntegrationEvent integrationEvent)
        {
            if (string.IsNullOrWhiteSpace(_options.PurchaseCreatedQueueUrl))
                throw new InvalidOperationException("A fila SQS PurchaseCreatedQueueUrl não foi configurada.");

            var payload = JsonSerializer.Serialize(integrationEvent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var request = new SendMessageRequest
            {
                QueueUrl = _options.PurchaseCreatedQueueUrl,
                MessageBody = payload,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    ["eventType"] = new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = integrationEvent.EventType
                    },
                    ["source"] = new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = integrationEvent.Source ?? "catalog-api"
                    },
                    ["correlationId"] = new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = integrationEvent.CorrelationId ?? string.Empty
                    }
                }
            };

            var response = await _amazonSqs.SendMessageAsync(request);

            _logger.LogInformation(
                "Evento publicado na SQS com sucesso. EventType: {EventType}, MessageId: {MessageId}, CorrelationId: {CorrelationId}",
                integrationEvent.EventType,
                response.MessageId,
                integrationEvent.CorrelationId);
        }
    }
}