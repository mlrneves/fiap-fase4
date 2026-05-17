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
    public class GamesController : Controller
    {
        private readonly IGameService _gameService;

        public GamesController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok(_gameService.ObterTodosDto());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:int}")]
        public IActionResult Get(int id)
        {
            try
            {
                var game = _gameService.ObterPorIdDto(id);

                if (game == null)
                    return NotFound("Jogo não encontrado.");

                return Ok(game);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = "Admin")]
        public IActionResult Post([FromBody] GameInput input)
        {
            try
            {
                var game = new Game
                {
                    Title = input.Title,
                    Description = input.Description,
                    Price = input.Price,
                    Genre = input.Genre,
                    Developer = input.Developer,
                    ReleaseDate = input.ReleaseDate
                };

                _gameService.Cadastrar(game);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(Policy = "Admin")]
        public IActionResult Put([FromBody] GameUpdateInput input)
        {
            try
            {
                var game = _gameService.ObterPorId(input.Id);

                game.Title = input.Title;
                game.Description = input.Description;
                game.Price = input.Price;
                game.Genre = input.Genre;
                game.Developer = input.Developer;
                game.ReleaseDate = input.ReleaseDate;

                _gameService.Alterar(game);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "Admin")]
        public IActionResult Delete(int id)
        {
            try
            {
                _gameService.Deletar(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("recommendations/{userId:int}")]
        public async Task<IActionResult> GetRecommendations([FromRoute] int userId, [FromQuery] int top = 5)
        {
            var result = await _gameService.GetRecommendationsAsync(userId, top);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, [FromServices] Core.Services.ISearchService searchService)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Parâmetro 'q' é obrigatório.");

            var results = await searchService.SearchAsync(q);
            return Ok(results);
        }
    }
}