namespace Core.Input
{
    public class GameInput
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required decimal Price { get; set; }
        public string? Genre { get; set; }
        public string? Developer { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}
