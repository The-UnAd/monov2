using Amazon.Lambda.Annotations;
using GraphMonitor;
using Microsoft.AspNetCore.Diagnostics;
using StackExchange.Redis;

[LambdaStartup]
public class Startup {

    public Startup() {
        Configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
    }
    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services) {
        services.AddSingleton(Configuration);
        services.Configure<ApiKeyAuthenticationOptions>(o =>
            Configuration.GetSection(nameof(ApiKeyAuthenticationOptions))
                .Bind(o));
        services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(sp.GetRequiredService<IConfiguration>()["REDIS_URL"]!));
        
        services.AddExceptionHandler(o => o.ExceptionHandler = context => {
            var exception = context.Features.Get<IExceptionHandlerFeature>()
                ?.Error;
            var logger = context.Features.Get<ILogger>();
            logger?.LogError(exception, "Unhandled exception {Message}", exception?.Message);
            return Task.CompletedTask;
        });
        services.AddAuthentication(o => o.AddScheme<ApiKeyAuthenticationHandler>("ApiKey", "API Key"));
        services.AddAuthorization();
    }
}