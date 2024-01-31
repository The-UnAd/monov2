using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace GraphMonitor.Tests.Integration;

public class FakePolicyEvaluator : IPolicyEvaluator {
    public virtual async Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context) {
        var principal = new ClaimsPrincipal();
        principal.AddIdentity(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.Authentication, "true")
        }, nameof(FakePolicyEvaluator)));
        return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal,
            new AuthenticationProperties(), nameof(FakePolicyEvaluator))));
    }

    public virtual async Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy,
        AuthenticateResult authenticationResult, HttpContext context, object? resource)
        => await Task.FromResult(PolicyAuthorizationResult.Success());
}

public class ApiFactory
    : WebApplicationFactory<Program>, IAsyncLifetime {
    private readonly RedisContainer _redisContainer = new RedisBuilder().Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        builder.ConfigureTestServices(services => {
            services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
            services.RemoveAll(typeof(IConnectionMultiplexer));
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString()));
        });

        builder.UseEnvironment("Development");
    }

    public Task InitializeAsync() => _redisContainer.StartAsync();

    public new async Task DisposeAsync() {
        await _redisContainer.DisposeAsync()
            .AsTask();
        await base.DisposeAsync();
    }
}



