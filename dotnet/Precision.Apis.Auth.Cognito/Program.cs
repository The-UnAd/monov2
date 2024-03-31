using System.Text.Json.Serialization;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using StackExchange.Redis;
using UnAd.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>();

builder.Services.AddTransient<IConnectionMultiplexer>(c =>
    ConnectionMultiplexer.Connect(
        c.GetRequiredService<IConfiguration>().GetValue<string>("REDIS_URL")!));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapPost("/login", async (Credentials credentials, IConfiguration config, IConnectionMultiplexer redis) => {
    var provider = new AmazonCognitoIdentityProviderClient(
        FallbackCredentialsFactory.GetCredentials(),
        FallbackRegionFactory.GetRegionEndpoint());
    var userPool = new CognitoUserPool(
        config.GetValue<string>("COGNITO_USER_POOL_ID"),
        config.GetValue<string>("COGNITO_CLIENT_ID"),
        provider);

    try {
        var user = new CognitoUser(credentials.Username, config.GetValue<string>("COGNITO_CLIENT_ID"), userPool, provider);

        var authResponse = await user.StartWithSrpAuthAsync(new InitiateSrpAuthRequest() {
            Password = credentials.Password
        }).ConfigureAwait(false);
        if (authResponse.AuthenticationResult == null) {
            return Results.BadRequest(new { Message = "Who are you?" });
        }

        var tokenId = Guid.NewGuid().ToString("N");
        var db = redis.GetDatabase();
        db.StoreUserToken(tokenId, authResponse.AuthenticationResult.AccessToken, authResponse.AuthenticationResult.ExpiresIn);

        return Results.Json(new LoginResult(tokenId), LoginResultTypeSerializerContext.Default);
    } catch (Exception ex) {
        return Results.BadRequest(new { ex.Message });
    }
});

app.MapPost("/logout", (HttpContext context, IConnectionMultiplexer redis) => {
    var db = redis.GetDatabase();
    var tokenId = context.Request.Headers.Authorization.FirstOrDefault()?.Split(' ').Last() ?? string.Empty;
    db.DeleteUserToken(tokenId);
    return Results.Ok();
});

app.MapHealthChecks("/health");

await app.RunAsync();


internal record Credentials(string Username, string Password);

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Credentials))]
internal partial class CredentialsSerializerContext : JsonSerializerContext { }

internal record LoginResult(string TokenId);

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(LoginResult))]
internal partial class LoginResultTypeSerializerContext : JsonSerializerContext { }

internal record WhoAmIResult(string Name);

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(WhoAmIResult))]
internal partial class WhoAmIResultTypeSerializerContext : JsonSerializerContext { }
