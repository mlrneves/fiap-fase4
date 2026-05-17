using Core.Entity;
using Core.Input;

namespace Core.Services
{
    public interface ISearchService
    {
        Task IndexGameAsync(Game game);
        Task RemoveGameAsync(int gameId);
        Task<IList<GameDto>> SearchAsync(string query);
    }
}
