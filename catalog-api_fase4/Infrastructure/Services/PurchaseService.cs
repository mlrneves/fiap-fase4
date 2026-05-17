using Core.Entity;
using Core.Events;
using Core.Input;
using Core.Repository;
using Core.Services;
using Infrastructure.CrossCutting.Correlation;

namespace Infrastructure.Services
{
    public class PurchaseService : BaseService<Purchase>, IPurchaseService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IPromotionRepository _promotionRepository;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;
        private readonly ICorrelationIdGenerator _correlationIdGenerator;

        public PurchaseService(
            IPurchaseRepository purchaseRepository,
            IGameRepository gameRepository,
            IPromotionRepository promotionRepository,
            IIntegrationEventPublisher integrationEventPublisher,
            ICorrelationIdGenerator correlationIdGenerator)
            : base(purchaseRepository)
        {
            _purchaseRepository = purchaseRepository;
            _gameRepository = gameRepository;
            _promotionRepository = promotionRepository;
            _integrationEventPublisher = integrationEventPublisher;
            _correlationIdGenerator = correlationIdGenerator;
        }

        public IList<PurchaseDto> ObterTodosDto()
        {
            return _purchaseRepository
                .ObterTodos()
                .Select(MapToDto)
                .ToList();
        }

        public PurchaseDto? ObterPorIdDto(int id)
        {
            var purchase = _purchaseRepository.ObterPorId(id);

            if (purchase == null)
                return null;

            return MapToDto(purchase);
        }

        public IList<PurchaseDto> ObterPorUserDto(int userId)
        {
            return _purchaseRepository
                .ObterTodos()
                .Where(p => p.UserId == userId)
                .Select(MapToDto)
                .ToList();
        }

        public IList<UserLibraryDto> GetLibraryByUserDto(int userId)
        {
            return GetLibraryByUser(userId)
                .Select(MapLibraryToDto)
                .ToList();
        }

        public async Task<Purchase> CreatePurchaseAsync(PurchaseInput input)
        {
            var game = _gameRepository.ObterPorId(input.GameId);

            if (game == null)
                throw new Exception("Jogo não encontrado.");

            var alreadyExists = _purchaseRepository
                .ObterTodos()
                .Any(p => p.UserId == input.UserId && p.GameId == input.GameId);

            if (alreadyExists)
                throw new InvalidOperationException("O usuário já comprou este jogo.");

            var now = DateTime.UtcNow;

            var promotion = _promotionRepository
                .ObterTodos()
                .FirstOrDefault(p =>
                    p.GameId == input.GameId &&
                    p.IsActive &&
                    p.StartDate <= now &&
                    p.EndDate >= now);

            var finalPrice = game.Price;

            if (promotion != null)
                finalPrice = game.Price - (game.Price * promotion.DiscountPercentage / 100);

            var purchase = new Purchase
            {
                UserId = input.UserId,
                GameId = input.GameId,
                PricePaid = finalPrice,
                PurchaseDate = now,
                Status = "Pending"
            };

            var created = base.Cadastrar(purchase);

            var correlationId = _correlationIdGenerator.Get();

            var purchaseCreatedEvent = new PurchaseCreatedEvent
            {
                PurchaseId = created.Id,
                UserId = created.UserId,
                GameId = created.GameId,
                Price = created.PricePaid,
                CorrelationId = correlationId
            };

            await _integrationEventPublisher.PublishAsync(purchaseCreatedEvent);

            return created;
        }

        public async Task ProcessPaymentResultAsync(int purchaseId, string status, DateTime? processedAt = null)
        {
            var purchase = _purchaseRepository.ObterPorId(purchaseId);

            if (purchase == null)
                throw new Exception("Compra não encontrada.");

            purchase.Status = status;
            purchase.ProcessedAt = processedAt ?? DateTime.UtcNow;

            base.Alterar(purchase);

            await Task.CompletedTask;
        }

        public IEnumerable<UserLibrary> GetLibraryByUser(int userId)
        {
            return _purchaseRepository
                .ObterTodos()
                .Where(p => p.UserId == userId && p.Status == "Approved")
                .Select(p => new UserLibrary
                {
                    UserId = p.UserId,
                    GameId = p.GameId,
                    PurchaseId = p.Id,
                    AddedAt = p.ProcessedAt ?? p.PurchaseDate
                })
                .ToList();
        }

        private static PurchaseDto MapToDto(Purchase purchase)
        {
            return new PurchaseDto
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
            };
        }

        private static UserLibraryDto MapLibraryToDto(UserLibrary library)
        {
            return new UserLibraryDto
            {
                Id = library.Id,
                CreatedAt = library.CreatedAt,
                UpdatedAt = library.UpdatedAt,
                UserId = library.UserId,
                GameId = library.GameId,
                PurchaseId = library.PurchaseId,
                AddedAt = library.AddedAt
            };
        }
    }
}