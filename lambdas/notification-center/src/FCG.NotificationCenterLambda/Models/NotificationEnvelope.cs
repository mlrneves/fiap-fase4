namespace FCG.NotificationCenterLambda.Models;

public sealed class NotificationEnvelope
{
    public string EventType { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public int? UserId { get; set; }
    public string? Email { get; set; }
    public int? PurchaseId { get; set; }
    public int? GameId { get; set; }
    public string? Status { get; set; }
    public decimal? Amount { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
