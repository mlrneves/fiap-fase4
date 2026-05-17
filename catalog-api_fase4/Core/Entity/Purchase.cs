namespace Core.Entity
{
    public class Purchase : EntityBase, IAuditableEntity
    {
        public required int UserId { get; set; }

        public required int GameId { get; set; }

        public required DateTime PurchaseDate { get; set; }

        public required decimal PricePaid { get; set; }

        public required string Status { get; set; } // Pending, Approved, Rejected

        public DateTime? ProcessedAt { get; set; }

        public virtual Game Game { get; set; }
    }
}