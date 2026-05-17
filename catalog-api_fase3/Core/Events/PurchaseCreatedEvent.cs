namespace Core.Events
{
    public class PurchaseCreatedEvent : IntegrationEvent
    {
        public int PurchaseId { get; set; }
        public int UserId { get; set; }
        public int GameId { get; set; }
        public decimal Price { get; set; }

        public PurchaseCreatedEvent()
        {
            EventType = "PurchaseCreated";
            Source = "catalog-api";
        }
    }
}