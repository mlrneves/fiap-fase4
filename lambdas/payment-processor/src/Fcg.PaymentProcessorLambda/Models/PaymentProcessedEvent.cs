namespace Fcg.PaymentProcessorLambda.Models;

public class PaymentProcessedEvent
{
    public string EventType { get; set; } = "PaymentProcessed";
    public string Source { get; set; } = "payment-processor-lambda";
    public string? CorrelationId { get; set; }

    public int PurchaseId { get; set; }
    public int UserId { get; set; }
    public int GameId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public DateTime ProcessedAt { get; set; }
}