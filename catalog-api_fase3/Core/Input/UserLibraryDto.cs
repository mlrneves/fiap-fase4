namespace Core.Input
{
    public class UserLibraryDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public required int UserId { get; set; }
        public required int GameId { get; set; }
        public required int PurchaseId { get; set; }
        public required DateTime AddedAt { get; set; }
    }
}