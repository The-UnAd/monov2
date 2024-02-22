using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace UnAd.Functions; 

internal class Authorizer(IConfiguration config) {

    [LambdaFunction]
    public APIGatewayCustomAuthorizerV2SimpleResponse Authorize(APIGatewayCustomAuthorizerV2Request request, ILambdaContext context) {
        context.Logger.LogLine("Authorizing request");
        string? apiKey = config.GetValue<string>("API_KEY");
        if (request.QueryStringParameters.TryGetValue("code", out var code) && code == apiKey) {
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
