using Core.Input;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using PaymentsAPI.Extensions;

namespace PaymentsAPI.Controllers
{
    [ApiController]
    [Route("api/payments/internal")]
    public class InternalPaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InternalPaymentsController> _logger;

        public InternalPaymentsController(
            IPaymentService paymentService,
            IConfiguration configuration,
            ILogger<InternalPaymentsController> logger)
        {
            _paymentService = paymentService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessInternal([FromBody] InternalPaymentProcessInput input)
        {
            try
            {
                var expectedApiKey = _configuration["InternalApi:ApiKey"];
                var receivedApiKey = Request.GetInternalApiKey();

                if (string.IsNullOrWhiteSpace(expectedApiKey) || receivedApiKey != expectedApiKey)
                    return Unauthorized("x-internal-api-key inválida.");

                if (input.PurchaseId <= 0)
                    return BadRequest("PurchaseId inválido.");

                if (input.UserId <= 0)
                    return BadRequest("UserId inválido.");

                if (input.Amount <= 0)
                    return BadRequest("Amount deve ser maior que zero.");

                var payment = await _paymentService.ProcessPaymentAsync(new PaymentInput
                {
                    PurchaseId = input.PurchaseId,
                    UserId = input.UserId,
                    Amount = input.Amount,
                    GameId = input.GameId
                });

                var result = new InternalPaymentProcessResult
                {
                    PaymentId = payment.Id,
                    PurchaseId = payment.PurchaseId,
                    UserId = payment.UserId,
                    Amount = payment.Amount,
                    Status = payment.Status,
                    ProcessedAt = DateTime.UtcNow,
                    TransactionId = $"TXN-{payment.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    CorrelationId = input.CorrelationId
                };

                _logger.LogInformation(
                    "Pagamento interno processado. PaymentId: {PaymentId}, PurchaseId: {PurchaseId}, Status: {Status}, CorrelationId: {CorrelationId}",
                    result.PaymentId,
                    result.PurchaseId,
                    result.Status,
                    result.CorrelationId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pagamento interno.");
                return BadRequest(ex.Message);
            }
        }
    }
}