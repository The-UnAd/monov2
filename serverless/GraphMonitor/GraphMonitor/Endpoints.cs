using System.Net;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using StackExchange.Redis;

namespace GraphMonitor;

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

