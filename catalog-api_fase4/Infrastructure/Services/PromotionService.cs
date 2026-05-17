using Core.Entity;
using Core.Input;
using Core.Repository;
using Core.Services;

namespace Infrastructure.Services
{
    public class PromotionService : BaseService<Promotion>, IPromotionService
    {
        private readonly IPromotionRepository _promotionRepository;

        public PromotionService(IPromotionRepository promotionRepository) : base(promotionRepository)
        {
            _promotionRepository = promotionRepository;
        }

        public IList<PromotionDto> ObterTodosAtivosDto()
        {
            return _promotionRepository
                .ObterTodos()
                .Where(p => p.IsActive)
                .Select(MapToDto)
                .ToList();
        }

        public PromotionDto? ObterPorIdDto(int id)
        {
            var promotion = _promotionRepository.ObterPorId(id);

            if (promotion == null)
                return null;

            return MapToDto(promotion);
        }

        public IList<PromotionDto> ObterPorGameDto(int gameId)
        {
            return _promotionRepository
                .ObterTodos()
                .Where(p => p.GameId == gameId && p.IsActive)
                .Select(MapToDto)
                .ToList();
        }

        private static PromotionDto MapToDto(Promotion promotion)
        {
            return new PromotionDto
            {
                Id = promotion.Id,
                CreatedAt = promotion.CreatedAt,
                UpdatedAt = promotion.UpdatedAt,
                GameId = promotion.GameId,
                DiscountPercentage = promotion.DiscountPercentage,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                IsActive = promotion.IsActive
            };
        }
    }
}