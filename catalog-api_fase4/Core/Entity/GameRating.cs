namespace Core.Entity
{
    public class GameRating
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int GameId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
