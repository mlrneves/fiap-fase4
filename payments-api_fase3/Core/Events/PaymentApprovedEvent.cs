namespace Core.Events
{
    public class PaymentApprovedEvent : IntegrationEvent
    {
        public int PaymentId { get; set; }
        public int PurchaseId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }

        public PaymentApprovedEvent()
        {
            EventType = "PaymentApproved";
            Source = "payments-api";
        }
    }
}