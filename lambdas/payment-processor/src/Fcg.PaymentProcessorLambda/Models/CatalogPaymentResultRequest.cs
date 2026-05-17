namespace Fcg.PaymentProcessorLambda.Models;

public class CatalogPaymentResultRequest
{
    public int PurchaseId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ProcessedAt { get; set; }
    public string? CorrelationId { get; set; }
}
