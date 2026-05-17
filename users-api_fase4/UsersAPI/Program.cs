using Amazon;
using Amazon.SQS;
using Core.Entity;
using Core.Repository;
using Core.Services;
using FCGApi.Infra;
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
using UsersAPI.Infra.Middleware;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("service", Environment.GetEnvironmentVariable("DD_SERVICE") ?? "fcg-users-api")
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
        Description = "Por favor, insira 'Bearer' [espaço] e o token JWT",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.SchemaFilter<EnumSchemaFilter>();
});

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
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionString"));
    options.UseLazyLoadingProxies();
}, ServiceLifetime.Scoped);

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.Configure<AwsSqsOptions>(
    builder.Configuration.GetSection("Aws:Sqs"));

builder.Services.AddSingleton<IAmazonSQS>(_ =>
{
    var regionName = builder.Configuration["Aws:Sqs:Region"];

    if (string.IsNullOrWhiteSpace(regionName))
        throw new InvalidOperationException("Aws:Sqs:Region não foi configurado.");

    return new AmazonSQSClient(RegionEndpoint.GetBySystemName(regionName));
});

builder.Services.AddScoped<IIntegrationEventPublisher, SqsIntegrationEventPublisher>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICorrelationIdGenerator, CorrelationIdGenerator>();
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

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<ApplicationDbContext>();
    var config = services.GetRequiredService<IConfiguration>();

    db.Database.Migrate();

    var adminSection = config.GetSection("AdminUser");
    var adminEmail = adminSection["Email"] ?? "admin@fcg.com";

    if (!db.Users.Any(u => u.Email == adminEmail))
    {
        var admin = new User
        {
            Name = adminSection["Name"] ?? "Admin",
            Email = adminEmail,
            Password = adminSection["Password"] ?? "Admin123!",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(admin);
        db.SaveChanges();
    }
}

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
                Url = $"{proto}://{host}/users"
            }
        };
    });
});

app.UseSwaggerUI();

app.UseCorrelationMiddleware();
app.UseLogMiddleware();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }