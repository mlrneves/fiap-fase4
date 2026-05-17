using Core.Entity;
using Core.Input;

namespace Core.Repository
{
    public interface IGameRepository : IRepository<Game>
    {
        Task<List<GameRecommendationDto>> GetRecommendationsAsync(int userId, int top = 5);
    }
}
