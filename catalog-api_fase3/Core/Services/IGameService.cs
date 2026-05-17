using Core.Entity;
using Core.Input;

namespace Core.Services
{
    public interface IGameService : IBaseService<Game>
    {
        IList<GameDto> ObterTodosDto();
        GameDto? ObterPorIdDto(int id);
        Task<List<GameRecommendationDto>> GetRecommendationsAsync(int userId, int top = 5);
    }
}