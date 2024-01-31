using System.Net;
using FluentAssertions;

namespace GraphMonitor.Tests.Integration;

[Collection(nameof(BasicTests))]
public sealed class BasicTests : IAsyncDisposable {
    private readonly ApiFactory _factory;

    public BasicTests(ApiFactory factory) {
        _factory = factory;
    }

    [Theory]
    [InlineData("/graph/test")]
    public async Task Get_ReturnsNotFoundWhenNoDataStored(string url) {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        response.StatusCode.Should()
            .Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("/graph/1")]
    public async Task Post_StoresWithoutFailure(string url) {
        var client = _factory.CreateClient();

        var response = await client.PostAsync(url, new StringContent("https://example.com"));

        response.StatusCode.Should()
            .Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("/graph/2")]
    public async Task Get_ReturnsWithoutFailure(string url) {
        var client = _factory.CreateClient();

        const string urlData = "https://example.com";
        await client.PostAsync(url, new StringContent(urlData));
        var response = await client.GetAsync(url);

        response.StatusCode.Should()
            .Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should()
            .Be(urlData);
    }

    public async ValueTask DisposeAsync() {
        await _factory.DisposeAsync();
    }
}



