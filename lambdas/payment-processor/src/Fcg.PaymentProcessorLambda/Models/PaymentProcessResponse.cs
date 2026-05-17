namespace Fcg.PaymentProcessorLambda.Models;

public class PaymentProcessResponse
{
    public int PaymentId { get; set; }
    public int PurchaseId { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public string? TransactionId { get; set; }
    public string? CorrelationId { get; set; }
}
