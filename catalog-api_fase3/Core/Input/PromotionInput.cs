namespace Core.Input
{
    public class PromotionInput
    {
        public required int GameId { get; set; }
        public required decimal DiscountPercentage { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
    }
}
