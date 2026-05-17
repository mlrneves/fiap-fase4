namespace Core.Input
{
    public class GameRecommendationDto
    {
        public int GameId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Genre { get; set; }
        public string? Developer { get; set; }
        public decimal Price { get; set; }
        public int TotalPurchases { get; set; }
    }
}
