using GraphMonitor;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions();

builder.Services.Configure<ApiKeyAuthenticationOptions>(o =>
    builder.Configuration.GetSection(nameof(ApiKeyAuthenticationOptions))
        .Bind(o));

builder.Services.AddAuthentication(o => o.AddScheme<ApiKeyAuthenticationHandler>("ApiKey", "API Key"));
builder.Services.AddAuthorization();

builder.Services.AddHealthChecks();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(sp.GetRequiredService<IConfiguration>()["REDIS_URL"]!));

var app = builder.Build();

app.MapHealthChecks("/health");

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();

public partial class Program { }



