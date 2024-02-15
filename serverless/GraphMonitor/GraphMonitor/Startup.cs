using Amazon.Lambda.Annotations;
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
        services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(sp.GetRequiredService<IConfiguration>()["REDIS_URL"]!));
    }
}