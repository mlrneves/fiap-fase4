namespace Fcg.PaymentProcessorLambda.Models;

public class PurchaseCreatedEvent
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string Source { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public int PurchaseId { get; set; }
    public int UserId { get; set; }
    public int GameId { get; set; }
    public decimal Price { get; set; }
}
