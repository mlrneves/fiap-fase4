namespace Core.Entity
{
    public class UserLibrary : EntityBase, IAuditableEntity
    {
        public required int UserId { get; set; }

        public required int GameId { get; set; }

        public required int PurchaseId { get; set; }

        public required DateTime AddedAt { get; set; }

        public virtual Game Game { get; set; }
    }
}