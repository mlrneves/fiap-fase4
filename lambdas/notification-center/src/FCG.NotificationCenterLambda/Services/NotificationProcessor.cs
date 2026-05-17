using FCG.NotificationCenterLambda.Models;

namespace FCG.NotificationCenterLambda.Services;

public sealed class NotificationProcessor(NotificationSink sink)
{
    public async Task ProcessAsync(NotificationEnvelope envelope, CancellationToken cancellationToken)
    {
        var body = BuildMessage(envelope);
        await sink.SendAsync(body, cancellationToken);
    }

    private static string BuildMessage(NotificationEnvelope envelope)
    {
        return envelope.EventType switch
        {
            "UserRegistered" => (
                $"Olá! Seu cadastro foi concluído com sucesso. UserId: {envelope.UserId}."
            ),
            "PaymentProcessed" => (
                $"A compra {envelope.PurchaseId} do jogo {envelope.GameId} teve o pagamento {NormalizeStatus(envelope.Status)}. Valor: {envelope.Amount?.ToString("0.00") ?? "0.00"}."
            ),
            _ => $"Um novo evento foi recebido e registrado pelo centro de notificações. Tipo: {envelope.EventType}."
        };
    }

    private static string NormalizeStatus(string? status)
        => string.IsNullOrWhiteSpace(status) ? "processado" : status!.Trim().ToLowerInvariant() switch
        {
            "approved" => "aprovado",
            "rejected" => "rejeitado",
            _ => status
        };
}
