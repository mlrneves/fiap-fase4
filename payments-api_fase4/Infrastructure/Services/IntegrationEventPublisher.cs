using Core.Events;
using Core.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class IntegrationEventPublisher : IIntegrationEventPublisher
    {
        private readonly ILogger<IntegrationEventPublisher> _logger;

        public IntegrationEventPublisher(ILogger<IntegrationEventPublisher> logger)
        {
            _logger = logger;
        }

        public Task PublishAsync(IntegrationEvent integrationEvent)
        {
            var payload = JsonSerializer.Serialize(integrationEvent);

            _logger.LogInformation(
                "Integration event published locally. EventType: {EventType}. Payload: {Payload}",
                integrationEvent.EventType,
                payload);

            return Task.CompletedTask;
        }
    }
}