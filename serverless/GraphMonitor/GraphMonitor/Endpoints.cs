using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using StackExchange.Redis;
using System.Net;

namespace GraphMonitor;

public class Authorizer(IConfiguration config) {

    [LambdaFunction]
    public APIGatewayCustomAuthorizerV2SimpleResponse Authorize(APIGatewayCustomAuthorizerV2Request request, ILambdaContext context) {
        context.Logger.LogLine("Authorizing request");
        string? apiKey = config.GetValue<string>("API_KEY");
        if (request.Headers.TryGetValue("x-api-key", out var token) && token == apiKey) {
            context.Logger.LogLine("Authorized");
            return new APIGatewayCustomAuthorizerV2SimpleResponse {
                IsAuthorized = true,
            };
        }

        context.Logger.LogLine("Unuthorized");
        return new APIGatewayCustomAuthorizerV2SimpleResponse {
            IsAuthorized = false,
        };
    }
}

public class StoreUrlFunction(IConnectionMultiplexer redis) {

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "/{name}")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> StoreUrl(string name, APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context) {
        context.Logger.LogLine("Invoked request");
        var db = redis.GetDatabase();
        if (string.IsNullOrEmpty(request.Body)) {
            return new APIGatewayHttpApiV2ProxyResponse {
                StatusCode = (int)HttpStatusCode.NotFound,
                Body = "Missing URL"
            };
        }

        await db.StringSetAsync($"graph:{name}", request.Body);

        return new APIGatewayHttpApiV2ProxyResponse {
            StatusCode = (int)HttpStatusCode.OK
        };
    }
}

public class GetUrlFunction(IConnectionMultiplexer redis) {

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/{name}")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> GetUrl(string name, APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context) {
        context.Logger.LogLine("Invoked request");
        var db = redis.GetDatabase();

        var url = await db.StringGetAsync($"graph:{name}");
        if (url.IsNullOrEmpty) {
            return new APIGatewayHttpApiV2ProxyResponse {
                StatusCode = (int)HttpStatusCode.NotFound,
                Body = "Missing URL"
            };
        }
        return new APIGatewayHttpApiV2ProxyResponse {
            StatusCode = (int)HttpStatusCode.OK,
            Body = url.ToString(),
        };
    }
}

