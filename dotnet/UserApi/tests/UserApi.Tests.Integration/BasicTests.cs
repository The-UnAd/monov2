using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using HotChocolate.Execution.Configuration;
using Microsoft.EntityFrameworkCore;

namespace UserApi.Tests.Integration;

[Collection(nameof(BasicTests))]
public sealed class BasicTests : IAsyncLifetime {
    private readonly ApiFactory _factory;

    public BasicTests(ApiFactory factory) {
        _factory = factory;
    }

    private async Task<User> CreateTestUser() {
        using var scope = _factory.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<UserDbContext>>();
        await using var dbContext = await factory.CreateDbContextAsync();
        var newUser = await dbContext.Users.AddAsync(new User {
            Id = "test",
            Email = "test@test.com",
            Name = "Test User",
            RoleId = "patient"
        });
        await dbContext.SaveChangesAsync();
        return newUser.Entity;
    }

    [Fact]
    public async Task GetUserReturnsUser() {
        var newUser = await CreateTestUser();

        await using var result = await _factory.ExecuteRequestAsync(r => 
            r.SetQuery($$"""
              query {
                user(id: "{{newUser.Id}}") {
                  name
                }
              }
              """));
        result.Errors.Should()
            .BeNullOrEmpty();
        result.Data
            .MatchSnapshot(extension: ".json");
    }

    [Fact]
    public async Task GetMeReturnsUser() {
        const string query = """
                             query {
                               me {
                                 name
                               }
                             }
                             """;
        await using var result = await _factory.ExecuteRequestAsync(r => r
            .AddUser(new[] { new Claim("userId", "patient1") })  // This SHOULD set the user 
            .SetQuery(query));
        
        // TODO: Why the hell is this coming back Unauthorized???
        result.Errors
            .Should().BeNullOrEmpty();
        result.Data
            .MatchSnapshot(extension: ".json");
        
    }

    [Fact]
    public async Task GetMeReturnsUser_HTTP() {
        var client = _factory.CreateClient();
        var token = "eyJraWQiOiJCbWF4a1wvUDJFRWRtbndmdzBIVXVDSG1TYXFHdU4yRkJBb09KUFRWVHM2Zz0iLCJhbGciOiJSUzI1NiJ9.eyJzdWIiOiI4MWFiNzU3MC1jMGIxLTcwMjctM2ZjNC00MDI0MjQ5OWU5ODEiLCJpc3MiOiJodHRwczpcL1wvY29nbml0by1pZHAudXMtZWFzdC0yLmFtYXpvbmF3cy5jb21cL3VzLWVhc3QtMl83SzFBRjZ4QUoiLCJjbGllbnRfaWQiOiI3MG1vZThpYWo0MnR1NXI4bW45bWk5ZjZubiIsIm9yaWdpbl9qdGkiOiJmYjE3MWY1Zi01MzZmLTRiMWItYTY2Yy00MDQ2MmVkNTRmYzMiLCJldmVudF9pZCI6ImRiMjVkNTExLTJiMjEtNDIxYi1hYTU2LTdiMTZkY2QxMjJlMCIsInRva2VuX3VzZSI6ImFjY2VzcyIsInNjb3BlIjoiYXdzLmNvZ25pdG8uc2lnbmluLnVzZXIuYWRtaW4iLCJhdXRoX3RpbWUiOjE2OTI3MzcxNDAsImV4cCI6MTY5Mjc0MDc0MCwiaWF0IjoxNjkyNzM3MTQwLCJqdGkiOiJkYzFkYjU2NC05MzA5LTQ1OGMtYjcyMS1kOWI2ZTIwM2VjOTUiLCJ1c2VybmFtZSI6IjgxYWI3NTcwLWMwYjEtNzAyNy0zZmM0LTQwMjQyNDk5ZTk4MSJ9.jygqSCaRFM95riPceZDbEkNTqDFihexZ_mkZT7DY4K5qqckmKZiZr-4zCSNILQwGYwEsN6rLTj8wZC-_I-Q87oK5R2GpndyZq6hV6-xhkafqcaBJoUhPIB5JgZde8yHtR1clkGSXeTVzy1o260QXsEGbr_iT0hGVKgop7Iv0QaPrSWIpPuoUITUKviTCul5-mTyQjStc5EVjileVCVxCpbUipcbiW6j57-K96H80lvlducx5vyNH5x7eo2a7kzLiYXfc1yodqwpTB9tqzTbaJ4Vg5Yfij00Mxmmbs_bZZswafTHlp3VQ4ykWBjaN6LNADPv9-POZKcgGByKK6eQwRg";
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        client.DefaultRequestHeaders.Add("X-Forwarded-Token", "hArLVdRwcgCQ4VK-T1-rxzi6J0PX_JF5Pnh1SLPRLTM");
        const string query = """
                             query {
                               me {
                                 name
                               }
                             }
                             """;
        var result = await client.PostAsJsonAsync("/graphql", new { query });
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await result.Content.ReadAsStringAsync();
        body.MatchSnapshot(extension: ".json");
        
    }
    
    public async Task InitializeAsync() {
        using var scope = _factory.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<UserDbContext>>();
        await using var dbContext = await factory.CreateDbContextAsync();
        dbContext.Database.SetCommandTimeout(160);
        await dbContext.Database.MigrateAsync();
        var patientRole = await dbContext.Roles.AddAsync(new Role {
            Id = "patient"
        });
        var providerRole = await dbContext.Roles.AddAsync(new Role {
            Id = "provider"
        });
        await dbContext.Users.AddAsync(new User {
            Id = "patient1",
            Email = "test@patient.com",
            Name = "Test Patient",
            RoleId = patientRole.Entity.Id
        });
        await dbContext.Users.AddAsync(new User {
            Id = "provider1",
            Email = "test@provider.com",
            Name = "Test Provider",
            RoleId = providerRole.Entity.Id
        });
        await dbContext.SaveChangesAsync();
    }

    public async Task DisposeAsync() {
        await _factory.DisposeAsync();
    }
}



