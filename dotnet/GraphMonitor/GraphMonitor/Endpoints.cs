using System.Net.Mime;
using System.Text;
using StackExchange.Redis;

namespace GraphMonitor; 

public static class Endpoints {

    public static void MapEndpoints(this WebApplication app) {
        app.MapPost("/graph/{name}", StoreUrl)
            .RequireAuthorization();
        app.MapGet("/graph/{name}", GetUrl)
            .RequireAuthorization();
    }

    public static async Task<IResult> StoreUrl(string name, Stream body, IConnectionMultiplexer redis) {
        var db = redis.GetDatabase();
        using var stream = new StreamReader(body, Encoding.UTF8);
        var url = await stream.ReadToEndAsync();
        if (string.IsNullOrEmpty(url)) {
            return Results.NotFound("Missing URL");
        }

        await db.StringSetAsync($"graph:{name}", url);

        return Results.Ok();
    }

    public static async Task<IResult> GetUrl(string name, IConnectionMultiplexer redis) {
        var db = redis.GetDatabase();
        var key = $"graph:{name}";
        var url = await db.StringGetAsync(key);
        if (url.IsNullOrEmpty) {
            return Results.NotFound($"Graph {name} not found");
        }

        return Results.Content(url.ToString(), MediaTypeNames.Text.Plain, Encoding.UTF8);
    }
}



