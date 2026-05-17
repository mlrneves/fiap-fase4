using Core.Entity;

namespace Core.Repository
{
    public interface IGameRatingRepository
    {
        Task AddAsync(GameRating rating);
        Task<IList<GameRating>> GetByGameIdAsync(int gameId);
    }
}
