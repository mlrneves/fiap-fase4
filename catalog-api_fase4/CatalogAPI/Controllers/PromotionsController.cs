using Core.Entity;
using Core.Input;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCGApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PromotionsController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionsController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok(_promotionService.ObterTodosAtivosDto());
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
                var promotion = _promotionService.ObterPorIdDto(id);

                if (promotion == null)
                    return NotFound("Promoção não encontrada.");

                return Ok(promotion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("game/{gameId:int}")]
        public IActionResult GetByGame([FromRoute] int gameId)
        {
            try
            {
                return Ok(_promotionService.ObterPorGameDto(gameId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = "Admin")]
        public IActionResult Post([FromBody] PromotionInput input)
        {
            try
            {
                var promotion = new Promotion
                {
                    GameId = input.GameId,
                    DiscountPercentage = input.DiscountPercentage,
                    StartDate = input.StartDate,
                    EndDate = input.EndDate,
                    IsActive = true
                };

                _promotionService.Cadastrar(promotion);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(Policy = "Admin")]
        public IActionResult Put([FromBody] PromotionUpdateInput input)
        {
            try
            {
                var promotion = _promotionService.ObterPorId(input.Id);

                promotion.GameId = input.GameId;
                promotion.DiscountPercentage = input.DiscountPercentage;
                promotion.StartDate = input.StartDate;
                promotion.EndDate = input.EndDate;
                promotion.IsActive = input.IsActive;

                _promotionService.Alterar(promotion);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "Admin")]
        public IActionResult Delete([FromRoute] int id)
        {
            try
            {
                var promotion = _promotionService.ObterPorId(id);

                promotion.IsActive = false;
                _promotionService.Alterar(promotion);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}