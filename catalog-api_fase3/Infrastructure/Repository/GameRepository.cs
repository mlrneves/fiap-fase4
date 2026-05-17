using Core.Entity;
using Core.Input;
using Core.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class GameRepository : EFRepository<Game>, IGameRepository
    {
        public GameRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<GameRecommendationDto>> GetRecommendationsAsync(int userId, int top = 5)
        {
            if (top <= 0)
                top = 5;

            var ownedGameIds = await _context.Purchases
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => p.GameId)
                .ToListAsync();

            var recommendations = await _context.Purchases
                .AsNoTracking()
                .Where(p => !ownedGameIds.Contains(p.GameId))
                .GroupBy(p => new
                {
                    p.GameId,
                    p.Game.Title,
                    p.Game.Genre,
                    p.Game.Developer,
                    p.Game.Price
                })
                .Select(g => new GameRecommendationDto
                {
                    GameId = g.Key.GameId,
                    Title = g.Key.Title,
                    Genre = g.Key.Genre,
                    Developer = g.Key.Developer,
                    Price = g.Key.Price,
                    TotalPurchases = g.Count()
                })
                .OrderByDescending(x => x.TotalPurchases)
                .ThenBy(x => x.Title)
                .Take(top)
                .ToListAsync();

            return recommendations;
        }
    }
}
