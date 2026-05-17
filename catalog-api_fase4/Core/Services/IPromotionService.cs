using Core.Entity;
using Core.Input;

namespace Core.Services
{
    public interface IPromotionService : IBaseService<Promotion>
    {
        IList<PromotionDto> ObterTodosAtivosDto();
        PromotionDto? ObterPorIdDto(int id);
        IList<PromotionDto> ObterPorGameDto(int gameId);
    }
}