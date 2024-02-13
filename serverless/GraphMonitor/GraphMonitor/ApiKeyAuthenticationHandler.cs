using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace GraphMonitor;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions {
    public string ApiKey { get; set; } = null!;
}

public class ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options,
                                         ILoggerFactory logger,
                                         UrlEncoder encoder)
    : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder) {

    protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
        if (!Request.Headers.TryGetValue("X-Api-Key", out var authHeader)) {
            return Task.FromResult(AuthenticateResult.Fail("Missing X-Api-Key Header"));
        }
        if (authHeader.Count == 0) {
            return Task.FromResult(AuthenticateResult.Fail("Missing API Key"));
        }
        if (authHeader.First() != OptionsMonitor.CurrentValue.ApiKey) {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }
        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(
                new ClaimsPrincipal(
                    new ClaimsIdentity(new[] {
                        new Claim(ClaimTypes.Authentication, "true")
                    }, Scheme.Name)), Scheme.Name)));
    }
}



