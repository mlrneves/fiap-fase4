using CatalogAPI.Extensions;
using Core.Input;
using Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FCGApi.Controllers
{
    [ApiController]
    [Route("api/internal/purchases")]
    public class InternalPurchasesController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InternalPurchasesController> _logger;

        public InternalPurchasesController(
            IPurchaseService purchaseService,
            IConfiguration configuration,
            ILogger<InternalPurchasesController> logger)
        {
            _purchaseService = purchaseService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("payment-result")]
        public async Task<IActionResult> PaymentResult([FromBody] InternalPaymentResultInput input)
        {
            try
            {
                var expectedApiKey = _configuration["InternalApi:ApiKey"];
                var receivedApiKey = Request.GetInternalApiKey();

                if (string.IsNullOrWhiteSpace(expectedApiKey) || receivedApiKey != expectedApiKey)
                    return Unauthorized("x-internal-api-key inválida.");

                if (input.PurchaseId <= 0)
                    return BadRequest("PurchaseId inválido.");

                if (string.IsNullOrWhiteSpace(input.Status))
                    return BadRequest("Status é obrigatório.");

                await _purchaseService.ProcessPaymentResultAsync(
                    input.PurchaseId,
                    input.Status,
                    input.ProcessedAt);

                _logger.LogInformation(
                    "Resultado interno de pagamento recebido. PurchaseId: {PurchaseId}, Status: {Status}, CorrelationId: {CorrelationId}",
                    input.PurchaseId,
                    input.Status,
                    input.CorrelationId);

                return Ok(new
                {
                    message = "Resultado do pagamento processado com sucesso."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar resultado interno do pagamento.");
                return BadRequest(ex.Message);
            }
        }
    }
}