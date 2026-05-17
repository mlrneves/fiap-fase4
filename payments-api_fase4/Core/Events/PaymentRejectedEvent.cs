namespace Core.Events
{
    public class PaymentRejectedEvent : IntegrationEvent
    {
        public int PaymentId { get; set; }
        public int PurchaseId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }

        public PaymentRejectedEvent()
        {
            EventType = "PaymentRejected";
            Source = "payments-api";
        }
    }
}