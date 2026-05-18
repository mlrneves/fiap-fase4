using Core.Entity;
using Core.Input;
using Core.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class ElasticsearchSearchService : ISearchService
    {
        private const string IndexName = "fcg-games";
        private readonly HttpClient _httpClient;
        private readonly ILogger<ElasticsearchSearchService> _logger;

        public ElasticsearchSearchService(HttpClient httpClient, ILogger<ElasticsearchSearchService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private async Task EnsureIndexAsync()
        {
            var exists = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, $"/{IndexName}"));
            if (exists.IsSuccessStatusCode)
                return;

            var mappings = new
            {
                mappings = new
                {
                    properties = new Dictionary<string, object>
                    {
                        { "title",       new { type = "text" } },
                        { "description", new { type = "text" } },
                        { "genre",       new { type = "text" } },
                        { "developer",   new { type = "text" } },
                        { "price",       new { type = "float" } },
                        { "releaseDate", new { type = "date" } }
                    }
                }
            };

            var createResp = await _httpClient.PutAsJsonAsync($"/{IndexName}", mappings);
            if (createResp.IsSuccessStatusCode)
                _logger.LogInformation("SEARCH INDEX CREATED - Índice {Index} criado com mappings text.", IndexName);
            else
            {
                var body = await createResp.Content.ReadAsStringAsync();
                _logger.LogWarning("SEARCH INDEX CREATE FAILED - Status {Status}: {Body}", (int)createResp.StatusCode, body);
            }
        }

        public async Task IndexGameAsync(Game game)
        {
            await EnsureIndexAsync();

            var doc = new
            {
                id = game.Id,
                title = game.Title,
                description = game.Description,
                genre = game.Genre,
                developer = game.Developer,
                price = game.Price,
                releaseDate = game.ReleaseDate
            };

            var resp = await _httpClient.PutAsJsonAsync($"/{IndexName}/_doc/{game.Id}", doc);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new Exception($"OpenSearch index failed: {(int)resp.StatusCode} {body}");
            }
        }

        public async Task RemoveGameAsync(int gameId)
        {
            await _httpClient.DeleteAsync($"/{IndexName}/_doc/{gameId}");
        }

        public async Task<IList<GameDto>> SearchAsync(string query)
        {
            var searchQuery = new
            {
                query = new
                {
                    multi_match = new
                    {
                        query,
                        fields = new[] { "title^3", "description", "genre", "developer" },
                        fuzziness = "AUTO",
                        type = "best_fields"
                    }
                }
            };

            var resp = await _httpClient.PostAsJsonAsync($"/{IndexName}/_search", searchQuery);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                _logger.LogWarning("SEARCH INVALID RESPONSE - Status {Status}: {Body}", (int)resp.StatusCode, body);
                return new List<GameDto>();
            }

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var hitsRoot = doc.RootElement.GetProperty("hits");
            var total = hitsRoot.GetProperty("total").GetProperty("value").GetInt32();
            _logger.LogInformation("SEARCH RESULT - {Total} hits para query '{Query}'", total, query);

            var results = new List<GameDto>();
            foreach (var hit in hitsRoot.GetProperty("hits").EnumerateArray())
            {
                var src = hit.GetProperty("_source");
                results.Add(new GameDto
                {
                    Id          = src.GetProperty("id").GetInt32(),
                    Title       = src.GetProperty("title").GetString() ?? "",
                    Price       = src.GetProperty("price").GetDecimal(),
                    Description = src.TryGetProperty("description", out var d) && d.ValueKind != JsonValueKind.Null ? d.GetString() : null,
                    Genre       = src.TryGetProperty("genre",       out var g) && g.ValueKind != JsonValueKind.Null ? g.GetString() : null,
                    Developer   = src.TryGetProperty("developer",   out var dev) && dev.ValueKind != JsonValueKind.Null ? dev.GetString() : null,
                    ReleaseDate = src.TryGetProperty("releaseDate", out var rd) && rd.ValueKind != JsonValueKind.Null ? rd.GetDateTime() : null
                });
            }

            return results;
        }
    }
}
