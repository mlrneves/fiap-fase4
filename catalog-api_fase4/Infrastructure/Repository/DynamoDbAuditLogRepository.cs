using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Core.Entity;
using Core.Repository;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Repository
{
    public class DynamoDbAuditLogRepository : IAuditLogRepository
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly string _tableName;

        public DynamoDbAuditLogRepository(IAmazonDynamoDB dynamoDb, IConfiguration configuration)
        {
            _dynamoDb = dynamoDb;
            _tableName = configuration["DynamoDB:TableName"] ?? "fcg-audit-logs";
        }

        public async Task AddAsync(AuditLog log)
        {
            var item = new Dictionary<string, AttributeValue>
            {
                ["Id"]          = new AttributeValue { S = log.Id.ToString() },
                ["EntityName"]  = new AttributeValue { S = log.EntityName },
                ["EntityId"]    = new AttributeValue { S = log.EntityId },
                ["Action"]      = new AttributeValue { S = log.Action },
                ["CreatedAtUtc"] = new AttributeValue { S = log.CreatedAtUtc.ToString("O") }
            };

            if (!string.IsNullOrWhiteSpace(log.UserId))
                item["UserId"] = new AttributeValue { S = log.UserId };

            if (!string.IsNullOrWhiteSpace(log.OldValues))
                item["OldValues"] = new AttributeValue { S = log.OldValues };

            if (!string.IsNullOrWhiteSpace(log.NewValues))
                item["NewValues"] = new AttributeValue { S = log.NewValues };

            if (!string.IsNullOrWhiteSpace(log.CorrelationId))
                item["CorrelationId"] = new AttributeValue { S = log.CorrelationId };

            await _dynamoDb.PutItemAsync(new PutItemRequest
            {
                TableName = _tableName,
                Item = item
            });
        }

        public async Task<IList<AuditLog>> GetByEntityTypeAsync(string entityName, int limit = 50)
        {
            var response = await _dynamoDb.QueryAsync(new QueryRequest
            {
                TableName = _tableName,
                IndexName = "EntityName-index",
                KeyConditionExpression = "EntityName = :entityName",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":entityName"] = new AttributeValue { S = entityName }
                },
                Limit = limit
            });

            return response.Items.Select(item => new AuditLog
            {
                Id            = Guid.Parse(item["Id"].S),
                EntityName    = item["EntityName"].S,
                EntityId      = item["EntityId"].S,
                Action        = item["Action"].S,
                CreatedAtUtc  = DateTime.Parse(item["CreatedAtUtc"].S),
                UserId        = item.TryGetValue("UserId", out var u) ? u.S : null,
                OldValues     = item.TryGetValue("OldValues", out var ov) ? ov.S : null,
                NewValues     = item.TryGetValue("NewValues", out var nv) ? nv.S : null,
                CorrelationId = item.TryGetValue("CorrelationId", out var c) ? c.S : null
            }).ToList();
        }
    }
}
