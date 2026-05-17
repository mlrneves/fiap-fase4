using Core.Input;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCGApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PurchasesController : Controller
    {
        private readonly IPurchaseService _purchaseService;

        public PurchasesController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        [HttpGet]
        [Authorize(Policy = "Admin")]
        public IActionResult Get()
        {
            try
            {
                return Ok(_purchaseService.ObterTodosDto());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:int}")]
        public IActionResult Get([FromRoute] int id)
        {
            try
            {
                var purchase = _purchaseService.ObterPorIdDto(id);

                if (purchase == null)
                    return NotFound("Compra não encontrada.");

                return Ok(purchase);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user/{userId:int}")]
        public IActionResult GetByUser([FromRoute] int userId)
        {
            try
            {
                return Ok(_purchaseService.ObterPorUserDto(userId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PurchaseInput input)
        {
            try
            {
                var purchase = await _purchaseService.CreatePurchaseAsync(input);

                return Ok(new PurchaseDto
                {
                    Id = purchase.Id,
                    CreatedAt = purchase.CreatedAt,
                    UpdatedAt = purchase.UpdatedAt,
                    UserId = purchase.UserId,
                    GameId = purchase.GameId,
                    PurchaseDate = purchase.PurchaseDate,
                    PricePaid = purchase.PricePaid,
                    Status = purchase.Status,
                    ProcessedAt = purchase.ProcessedAt
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user/{userId:int}/library")]
        public IActionResult GetLibrary([FromRoute] int userId)
        {
            try
            {
                return Ok(_purchaseService.GetLibraryByUserDto(userId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}