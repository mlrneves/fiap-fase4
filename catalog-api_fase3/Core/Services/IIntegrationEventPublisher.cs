using Core.Events;

namespace Core.Services
{
    public interface IIntegrationEventPublisher
    {
        Task PublishAsync(IntegrationEvent integrationEvent);
    }
}