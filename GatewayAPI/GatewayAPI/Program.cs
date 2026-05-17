var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseCors("AllowAll");

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "GatewayAPI v1");
    options.SwaggerEndpoint("/users/swagger/v1/swagger.json", "UsersAPI v1");
    options.SwaggerEndpoint("/games/swagger/v1/swagger.json", "CatalogAPI v1");
    options.SwaggerEndpoint("/payments/swagger/v1/swagger.json", "PaymentsAPI v1");
});

app.MapGet("/health", () => Results.Ok(new
{
    service = "GatewayAPI",
    status = "ok",
    utc = DateTime.UtcNow
}));

app.MapReverseProxy();

app.Run();