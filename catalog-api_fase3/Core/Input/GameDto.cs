namespace Core.Input
{
    public class GameDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required decimal Price { get; set; }
        public string? Genre { get; set; }
        public string? Developer { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}