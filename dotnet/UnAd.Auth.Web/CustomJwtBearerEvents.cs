using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace UnAd.Auth.Web;

public class CustomJwtBearerEvents : JwtBearerEvents {

    public override async Task TokenValidated(TokenValidatedContext context) {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<CustomJwtBearerEvents>>();
        var token = context.Request.Headers["X-Forwarded-Token"]
            .ToString();
        var redis = context.HttpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
        var claims = await redis.GetDatabase()
            .HashGetAllAsync($"token:{token}:claims");
        logger.LogDebug("Got {ClaimsCount} claims from Redis", claims.Length);
        // TODO: what do we do if there are no claims?
        context.Principal?.AddIdentity(new ClaimsIdentity(
            claims.Select(c => new Claim(c.Name, c.Value))));
        logger.LogDebug("Token validated for user {User}",
            context.Principal?.FindFirst(c => c.Type == "username")?.Value ?? "anonymous");
        await base.TokenValidated(context);
    }

    public override Task MessageReceived(MessageReceivedContext context) {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<CustomJwtBearerEvents>>();
        logger.LogDebug("Got message with {Length}-length hash", context.Request.Headers["X-Forwarded-Token"].ToString().Length);
        logger.LogDebug("Got message with {Length}-length token", context.Request.Headers["Authorization"].ToString().Length);
        return base.MessageReceived(context);
    }

    public override Task AuthenticationFailed(AuthenticationFailedContext context) {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<CustomJwtBearerEvents>>();
        logger.LogWarning(context.Exception, "Authentication failed");
        return base.AuthenticationFailed(context);
    }

    public override Task Challenge(JwtBearerChallengeContext context)
        => base.Challenge(context);

    public override Task Forbidden(ForbiddenContext context)
        => base.Forbidden(context);
}


