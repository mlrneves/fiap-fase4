namespace Core.Input
{
    public class PurchaseDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public required int UserId { get; set; }
        public required int GameId { get; set; }
        public required DateTime PurchaseDate { get; set; }
        public required decimal PricePaid { get; set; }
        public required string Status { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}