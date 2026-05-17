namespace Core.Entity
{
    public class Game : EntityBase, IAuditableEntity
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required decimal Price { get; set; }
        public string? Genre { get; set; }
        public string? Developer { get; set; }
        public DateTime? ReleaseDate { get; set; }

        public virtual ICollection<Purchase> Purchases { get; set; }
        public virtual ICollection<Promotion> Promotions { get; set; }
    }
}