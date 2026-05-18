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
                _logger.LogInformation("CACHE HIT - Lista de jogos recuperada do Redis.");
                return JsonSerializer.Deserialize<List<GameDto>>(cached)!;
            }

            _logger.LogInformation("CACHE MISS - Lista de jogos não encontrada no Redis. Consultando banco de dados.");
            var games = _gameRepository.ObterTodos().Select(MapToDto).ToList();
            _cache.SetString(CacheKey, JsonSerializer.Serialize(games), CacheOptions);
            _logger.LogInformation("CACHE SET - Lista de jogos armazenada no Redis.");
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
            _logger.LogInformation("CACHE INVALIDATED - Cache da lista de jogos removido após alteração no catálogo.");
            _cache.Remove(CacheKey);
            _ = _searchService.IndexGameAsync(game).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    _logger.LogError(t.Exception, "SEARCH INDEX ERROR - Falha ao indexar jogo {GameId} no OpenSearch.", game.Id);
                else
                    _logger.LogInformation("SEARCH INDEX OK - Jogo {GameId} indexado no OpenSearch.", game.Id);
            });
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
            _logger.LogInformation("CACHE INVALIDATED - Cache da lista de jogos removido após alteração no catálogo.");
            _cache.Remove(CacheKey);
            _ = _searchService.IndexGameAsync(game).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    _logger.LogError(t.Exception, "SEARCH INDEX ERROR - Falha ao indexar jogo {GameId} no OpenSearch.", game.Id);
                else
                    _logger.LogInformation("SEARCH INDEX OK - Jogo {GameId} indexado no OpenSearch.", game.Id);
            });
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
            _logger.LogInformation("CACHE INVALIDATED - Cache da lista de jogos removido após alteração no catálogo.");
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
