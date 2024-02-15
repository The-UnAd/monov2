using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using StackExchange.Redis;
using System.Net;

namespace GraphMonitor;

public class Authorizer(IConfiguration config) {

    [LambdaFunction]
    public APIGatewayCustomAuthorizerResponse Authorize(APIGatewayCustomAuthorizerRequest request, ILambdaContext context) {

        var apiKey = string.IsNullOrEmpty(request.AuthorizationToken) ? request.QueryStringParameters?["token"] : string.Empty;
        if (string.IsNullOrEmpty(apiKey)) {
            return new APIGatewayCustomAuthorizerResponse {
                UsageIdentifierKey = "Unauthorized"
            };
        }
        if (apiKey != config.GetValue<string>("API_KEY")) {
            return new APIGatewayCustomAuthorizerResponse {
                UsageIdentifierKey = "Unauthorized"
            };
        }

        return new APIGatewayCustomAuthorizerResponse {
            PrincipalID = "user",
            PolicyDocument = new APIGatewayCustomAuthorizerPolicy {
                Version = "2012-10-17",
                Statement = [
                    new() {
                        Action = ["execute-api:Invoke"],
                        Effect = "Allow",
                        Resource = ["*"]
                    }
                ]
            }
        };
    }
}

public static class RequestExtensions {

}

public class StoreUrlFunction(IConnectionMultiplexer redis) {

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "/{name}")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> StoreUrl(string name, APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context) {
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

