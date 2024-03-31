using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using UnAd.Data.Users;

namespace UnAd.Functions.Tests.Integration;

public class ApiFactory
    : WebApplicationFactory<Program>, IAsyncLifetime {
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .Build();
    private readonly RedisContainer _redisContainer = new RedisBuilder().Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        builder.ConfigureTestServices(services => {
            services.Remove(services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<UserDbContext>))!);
            services.Remove(services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbConnection))!);
            services.AddPooledDbContextFactory<UserDbContext>(o
                => o.UseNpgsql(_postgresContainer.GetConnectionString()));

            services.Remove(services.SingleOrDefault(d =>
                           d.ServiceType == typeof(IRequestAuthorizer))!);
            services.AddSingleton<IRequestAuthorizer, FakeRequestAuthorizer>();

            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString()));
        });

        builder.UseEnvironment("Development");
    }

    public async Task InitializeAsync() {
        await _postgresContainer.StartAsync();
        await _redisContainer.StartAsync();
    }

    public new async Task DisposeAsync() {
        await _postgresContainer.DisposeAsync()
            .AsTask();
        await _redisContainer.DisposeAsync()
            .AsTask();
        await base.DisposeAsync();
    }

    private class FakeRequestAuthorizer : IRequestAuthorizer {
        public bool IsAuthorized(HttpContext context) => true;
    }
}



