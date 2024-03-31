using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UnAd.Data.Users;
using UnAd.Data.Users.Models;

namespace UnAd.Functions.Tests.Integration;

[Collection(nameof(MessageHandlerTests))]
public sealed class MessageHandlerTests(ApiFactory factory) : IAsyncLifetime {
    private readonly ApiFactory _factory = factory;

    private async Task<Client> CreateClient(string name, string phoneNumber) {
        using var scope = _factory.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<UserDbContext>>();
        await using var dbContext = await factory.CreateDbContextAsync();
        var testClient = await dbContext.Clients.AddAsync(new Client {
            Name = name,
            PhoneNumber = phoneNumber,
            Locale = "en-US"
        });
        await dbContext.SaveChangesAsync();
        return testClient.Entity;
    }

    [Fact]
    public async Task MessageHandlerCommandsReturnsAMessage() {
        await CreateClient("test1", "+19045550000");

        using var client = _factory.CreateClient();
        var result = await client.PostAsync("/api/MessageHandler",
            new FormUrlEncodedContent([
                new KeyValuePair<string, string>("Body", "COMMANDS"),
                new KeyValuePair<string, string>("From", "+19045550000")
            ]));
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Content.Headers.ContentType?.MediaType.Should().Be("text/xml");
        var body = await result.Content.ReadAsStringAsync();
        body.Should().Contain("Available commands");
    }

    public async Task InitializeAsync() {
        using var scope = _factory.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<UserDbContext>>();
        await using var dbContext = await factory.CreateDbContextAsync();
        dbContext.Database.SetCommandTimeout(160);
        await dbContext.Database.MigrateAsync();
        var testClient = await dbContext.Clients.AddAsync(new Client {
            Name = "UnAd",
            PhoneNumber = "+15555555555",
            Locale = "en-US",
        });
        await dbContext.SaveChangesAsync();
    }

    public async Task DisposeAsync() => await _factory.DisposeAsync();
}



