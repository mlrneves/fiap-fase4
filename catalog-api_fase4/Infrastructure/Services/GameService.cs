using Core.Entity;
using Core.Input;
using Core.Repository;
using Core.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class GameService : BaseService<Game>, IGameService
    {
        private const string CacheKey = "fcg:games:all";
        private static readonly DistributedCacheEntryOptions CacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        private readonly IGameRepository _gameRepository;
        private readonly IDistributedCache _cache;
        private readonly ISearchService _searchService;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ILogger<GameService> _logger;

        public GameService(
            IGameRepository gameRepository,
            IDistributedCache cache,
            ISearchService searchService,
            IAuditLogRepository auditLogRepository,
            ILogger<GameService> logger) : base(gameRepository)
        {
            _gameRepository = gameRepository;
            _cache = cache;
            _searchService = searchService;
            _auditLogRepository = auditLogRepository;
            _logger = logger;
        }

        public IList<GameDto> ObterTodosDto()
        {
            var cached = _cache.GetString(CacheKey);
            if (cached is not null)
            {
                _logger.LogInformation("[Cache HIT] Jogos retornados do cache. Key={CacheKey}", CacheKey);
                return JsonSerializer.Deserialize<List<GameDto>>(cached)!;
            }

            _logger.LogInformation("[Cache MISS] Cache não encontrado. Consultando banco de dados. Key={CacheKey}", CacheKey);
            var games = _gameRepository.ObterTodos().Select(MapToDto).ToList();
            _cache.SetString(CacheKey, JsonSerializer.Serialize(games), CacheOptions);
            _logger.LogInformation("[Cache SET] {Count} jogo(s) armazenados no cache. Key={CacheKey} TTL=5min", games.Count, CacheKey);
            return games;
        }

        public GameDto? ObterPorIdDto(int id)
        {
            var game = _gameRepository.ObterPorId(id);
            return game is null ? null : MapToDto(game);
        }

        public override Game Cadastrar(Game game)
        {
            base.Cadastrar(game);
            _logger.LogInformation("[Cache INVALIDADO] Jogo criado. Cache removido. GameId={GameId} Key={CacheKey}", game.Id, CacheKey);
            _cache.Remove(CacheKey);
            _ = _searchService.IndexGameAsync(game);
            _ = _auditLogRepository.AddAsync(new AuditLog
            {
                EntityName = "Game",
                EntityId   = game.Id.ToString(),
                Action     = "Created",
                NewValues  = JsonSerializer.Serialize(MapToDto(game))
            });
            return game;
        }

        public override Game Alterar(Game game)
        {
            base.Alterar(game);
            _logger.LogInformation("[Cache INVALIDADO] Jogo atualizado. Cache removido. GameId={GameId} Key={CacheKey}", game.Id, CacheKey);
            _cache.Remove(CacheKey);
            _ = _searchService.IndexGameAsync(game);
            _ = _auditLogRepository.AddAsync(new AuditLog
            {
                EntityName = "Game",
                EntityId   = game.Id.ToString(),
                Action     = "Updated",
                NewValues  = JsonSerializer.Serialize(MapToDto(game))
            });
            return game;
        }

        public override void Deletar(int id)
        {
            base.Deletar(id);
            _logger.LogInformation("[Cache INVALIDADO] Jogo removido. Cache removido. GameId={GameId} Key={CacheKey}", id, CacheKey);
            _cache.Remove(CacheKey);
            _ = _searchService.RemoveGameAsync(id);
            _ = _auditLogRepository.AddAsync(new AuditLog
            {
                EntityName = "Game",
                EntityId   = id.ToString(),
                Action     = "Deleted"
            });
        }

        public Task<List<GameRecommendationDto>> GetRecommendationsAsync(int userId, int top = 5)
            => _gameRepository.GetRecommendationsAsync(userId, top);

        private static GameDto MapToDto(Game game) => new()
        {
            Id = game.Id,
            CreatedAt = game.CreatedAt,
            UpdatedAt = game.UpdatedAt,
            Title = game.Title,
            Description = game.Description,
            Price = game.Price,
            Genre = game.Genre,
            Developer = game.Developer,
            ReleaseDate = game.ReleaseDate
        };
    }
}
