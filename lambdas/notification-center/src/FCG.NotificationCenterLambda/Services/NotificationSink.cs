namespace FCG.NotificationCenterLambda.Services;

public sealed class NotificationSink
{
    public Task SendAsync(string body, CancellationToken cancellationToken)
    {
        Console.WriteLine($"""
        ================== NOTIFICATION CENTER ==================
        Simulando notificação:
        {body}
        =========================================================
        """);

        return Task.CompletedTask;
    }
}