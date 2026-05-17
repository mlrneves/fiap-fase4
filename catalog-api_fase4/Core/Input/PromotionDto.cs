namespace Core.Input
{
    public class PromotionDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public required int GameId { get; set; }
        public required decimal DiscountPercentage { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required bool IsActive { get; set; }
    }
}