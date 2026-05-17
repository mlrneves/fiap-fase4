using Amazon;
using Amazon.DynamoDBv2;
using Amazon.SQS;
using CatalogAPI.Infra.Middleware;
using Core.Repository;
using Core.Services;
using Elastic.Clients.Elasticsearch;
using Infrastructure.Configuration;
using Infrastructure.CrossCutting.Correlation;
using Infrastructure.Repository;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Formatting.Compact;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("service", Environment.GetEnvironmentVariable("DD_SERVICE") ?? "fcg-catalog-api")
    .Enrich.WithProperty("env", Environment.GetEnvironmentVariable("DD_ENV") ?? "dev")
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Por favor, insira 'Bearer' [espa�o] e o token JWT",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

#region [JWT]
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

builder.Services.AddControllers();
#endregion

#region [Redis Cache]
var redisConnection = builder.Configuration["Redis:ConnectionString"]
    ?? builder.Configuration["REDIS_CONNECTION"]
    ?? "redis:6379";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "fcg:";
});
#endregion

#region [Elasticsearch / OpenSearch]
var elasticsearchUrl = builder.Configuration["Elasticsearch:Url"]
    ?? builder.Configuration["ELASTICSEARCH_URL"]
    ?? "http://localhost:9200";

var elasticsearchClient = new ElasticsearchClient(new Uri(elasticsearchUrl));
builder.Services.AddSingleton(elasticsearchClient);
builder.Services.AddScoped<ISearchService, ElasticsearchSearchService>();
#endregion

#region [DynamoDB]
builder.Services.AddSingleton<IAmazonDynamoDB>(_ =>
{
    var regionName = builder.Configuration["Aws:Sqs:Region"]
        ?? builder.Configuration["AWS_REGION"]
        ?? "us-east-2";
    return new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(regionName));
});
builder.Services.AddScoped<IAuditLogRepository, DynamoDbAuditLogRepository>();
#endregion

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionString"));
    options.UseLazyLoadingProxies();
}, ServiceLifetime.Scoped);

builder.Services.Configure<AwsSqsOptions>(
    builder.Configuration.GetSection("Aws:Sqs"));

builder.Services.AddSingleton<IAmazonSQS>(_ =>
{
    var regionName = builder.Configuration["Aws:Sqs:Region"];

    if (string.IsNullOrWhiteSpace(regionName))
        throw new InvalidOperationException("Aws:Sqs:Region n�o foi configurado.");

    return new AmazonSQSClient(RegionEndpoint.GetBySystemName(regionName));
});

#region [DI]
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IIntegrationEventPublisher, SqsIntegrationEventPublisher>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICorrelationIdGenerator, CorrelationIdGenerator>();
#endregion

builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGateway", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

var app = builder.Build();

app.UseCors("AllowGateway");

app.UseSwagger(c =>
{
    c.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        var host = httpReq.Headers["x-forwarded-host"].FirstOrDefault() ?? httpReq.Host.Value;
        var proto = httpReq.Headers["x-forwarded-proto"].FirstOrDefault() ?? httpReq.Scheme;

        swagger.Servers = new List<Microsoft.OpenApi.Models.OpenApiServer>
        {
            new()
            {
                Url = $"{proto}://{host}/games"
            }
        };
    });
});

app.UseSwaggerUI();

#region [Middler]
app.UseCorrelationMiddleware();
app.UseLogMiddleware();
#endregion

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
