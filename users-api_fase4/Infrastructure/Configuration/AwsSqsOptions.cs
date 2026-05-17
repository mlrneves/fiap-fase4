namespace Infrastructure.Configuration
{
    public class AwsSqsOptions
    {
        public string Region { get; set; } = string.Empty;
        public string NotificationsQueueUrl { get; set; } = string.Empty;
    }
}