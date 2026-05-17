using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Core.Entity;
using Core.Repository;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Repository
{
    public class DynamoDbGameRatingRepository : IGameRatingRepository
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly string _tableName;

        public DynamoDbGameRatingRepository(IAmazonDynamoDB dynamoDb, IConfiguration configuration)
        {
            _dynamoDb = dynamoDb;
            _tableName = configuration["DynamoDB:TableName"] ?? "fcg-game-ratings";
        }

        public async Task AddAsync(GameRating rating)
        {
            var item = new Dictionary<string, AttributeValue>
            {
                ["Id"]        = new AttributeValue { S = rating.Id },
                ["GameId"]    = new AttributeValue { N = rating.GameId.ToString() },
                ["UserId"]    = new AttributeValue { N = rating.UserId.ToString() },
                ["Rating"]    = new AttributeValue { N = rating.Rating.ToString() },
                ["CreatedAt"] = new AttributeValue { S = rating.CreatedAt.ToString("O") }
            };

            if (!string.IsNullOrWhiteSpace(rating.Comment))
                item["Comment"] = new AttributeValue { S = rating.Comment };

            await _dynamoDb.PutItemAsync(new PutItemRequest
            {
                TableName = _tableName,
                Item = item
            });
        }

        public async Task<IList<GameRating>> GetByGameIdAsync(int gameId)
        {
            var response = await _dynamoDb.QueryAsync(new QueryRequest
            {
                TableName = _tableName,
                IndexName = "GameId-index",
                KeyConditionExpression = "GameId = :gameId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":gameId"] = new AttributeValue { N = gameId.ToString() }
                }
            });

            return response.Items.Select(item => new GameRating
            {
                Id        = item["Id"].S,
                GameId    = int.Parse(item["GameId"].N),
                UserId    = int.Parse(item["UserId"].N),
                Rating    = int.Parse(item["Rating"].N),
                Comment   = item.TryGetValue("Comment", out var c) ? c.S : null,
                CreatedAt = DateTime.Parse(item["CreatedAt"].S)
            }).ToList();
        }
    }
}
