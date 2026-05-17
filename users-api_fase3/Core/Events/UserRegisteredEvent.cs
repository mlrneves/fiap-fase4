namespace Core.Events
{
    public class UserRegisteredEvent : IntegrationEvent
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public UserRegisteredEvent()
        {
            EventType = "UserRegistered";
            Source = "users-api";
        }
    }
}