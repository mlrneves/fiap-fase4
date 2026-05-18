using Core.Entity;
using Core.Input;
using Core.Services;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace Infrastructure.Services
{
    public class ElasticsearchSearchService : ISearchService
    {
        private const string IndexName = "fcg-games";
        private readonly ElasticsearchClient _client;

        public ElasticsearchSearchService(ElasticsearchClient client)
        {
            _client = client;
        }

        private async Task EnsureIndexAsync()
        {
            var exists = await _client.Indices.ExistsAsync(IndexName);
            if (exists.Exists)
                return;

            await _client.Indices.CreateAsync(IndexName, c => c
                .Mappings(m => m
                    .Properties(new Properties
                    {
                        { "title",       new TextProperty() },
                        { "description", new TextProperty() },
                        { "genre",       new TextProperty() },
                        { "developer",   new TextProperty() },
                        { "price",       new FloatNumberProperty() },
                        { "releaseDate", new DateProperty() }
                    })
                )
            );
        }

        public async Task IndexGameAsync(Game game)
        {
            await EnsureIndexAsync();

            var doc = new GameSearchDocument
            {
                Id = game.Id,
                Title = game.Title,
                Description = game.Description,
                Genre = game.Genre,
                Developer = game.Developer,
                Price = game.Price,
                ReleaseDate = game.ReleaseDate
            };

            await _client.IndexAsync(doc, i => i.Index(IndexName).Id(game.Id.ToString()));
        }

        public async Task RemoveGameAsync(int gameId)
        {
            await _client.DeleteAsync<GameSearchDocument>(
                gameId.ToString(),
                d => d.Index(IndexName));
        }

        public async Task<IList<GameDto>> SearchAsync(string query)
        {
            var response = await _client.SearchAsync<GameSearchDocument>(s => s
                .Index(IndexName)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Fields(new[] { "title^3", "description", "genre", "developer" })
                        .Query(query)
                        .Fuzziness(new Fuzziness("AUTO"))
                        .Type(TextQueryType.BestFields)
                    )
                )
            );

            if (!response.IsValidResponse)
                return new List<GameDto>();

            return response.Documents
                .Select(d => new GameDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    Description = d.Description,
                    Genre = d.Genre,
                    Developer = d.Developer,
                    Price = d.Price,
                    ReleaseDate = d.ReleaseDate
                })
                .ToList();
        }

        private sealed class GameSearchDocument
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string? Genre { get; set; }
            public string? Developer { get; set; }
            public decimal Price { get; set; }
            public DateTime? ReleaseDate { get; set; }
        }
    }
}
