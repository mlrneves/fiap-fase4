using Core.Input;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PaymentsAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        try
        {
            return Ok(_paymentService.GetAll());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        try
        {
            var payment = _paymentService.GetById(id);

            if (payment == null)
                return NotFound("Pagamento não encontrado.");

            return Ok(payment);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("purchase/{purchaseId:int}")]
    public IActionResult GetByPurchaseId(int purchaseId)
    {
        try
        {
            var payment = _paymentService.GetByPurchaseId(purchaseId);

            if (payment == null)
                return NotFound("Pagamento não encontrado para a compra informada.");

            return Ok(payment);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] PaymentInput input)
    {
        try
        {
            var payment = await _paymentService.ProcessPaymentAsync(input);
            return Ok(payment);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}