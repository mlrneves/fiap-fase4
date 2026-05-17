namespace Fcg.PaymentProcessorLambda.Models;

public class PaymentProcessRequest
{
    public int PurchaseId { get; set; }
    public int UserId { get; set; }
    public int GameId { get; set; }
    public decimal Amount { get; set; }
    public string? CorrelationId { get; set; }
}
