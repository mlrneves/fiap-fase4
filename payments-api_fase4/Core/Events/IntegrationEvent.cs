namespace Core.Events
{
    public abstract class IntegrationEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public string EventType { get; set; } = string.Empty;
        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
        public string CorrelationId { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }
}