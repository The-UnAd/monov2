namespace UnAd.Functions;

public interface IRequestAuthorizer {
    bool IsAuthorized(HttpContext context);
}

public class RequestAuthorizer : IRequestAuthorizer {

    public bool IsAuthorized(HttpContext context) => context.Request.Query.TryGetValue("code", out var value) &&
        value == context.RequestServices.GetRequiredService<IConfiguration>().GetValue<string>("API_KEY");
}
