namespace Core.Entity;

public class Payment: IAuditableEntity
{
    public int Id { get; set; }
    public int PurchaseId { get; set; }
    public int UserId { get; set; }
    public int GameId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty; // Approved / Rejected
    public DateTime ProcessedAt { get; set; }
}
