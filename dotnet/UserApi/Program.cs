using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using Stripe;
using Twilio;
using UnAd.Data.Users;
using UserApi;
using UserApi.TypeExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddTransient<IConnectionMultiplexer>(c =>
    ConnectionMultiplexer.Connect(c.GetRequiredService<IConfiguration>().GetRedisUrl()));

builder.Services.AddPooledDbContextFactory<UserDbContext>((s, o) =>
    o.UseNpgsql(s.GetRequiredService<IConfiguration>().GetConnectionString("UserDb")));

builder.Services.AddSingleton<IStripeClient>(s =>
    new StripeClient(s.GetRequiredService<IConfiguration>().GetStripeApiKey()));

builder.Services.AddSingleton(() => {
    TwilioClient.Init(builder.Configuration.GetTwilioAccountSid(),
       builder.Configuration.GetTwilioAuthToken());
    return TwilioClient.GetRestClient();
});

builder.Services.AddSingleton<IMessageSender, MessageSender>();

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.Authority = builder.Configuration["Cognito:Authority"];
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuerSigningKey = true,
        ValidateAudience = false
    };
    options.Events = new JwtBearerEvents {
        OnAuthenticationFailed = context => {
            var logger = context.Request.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
            logger.LogAuthFailure(context.Exception);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType<QueryType>()
    .AddDiagnosticEventListener<LoggerExecutionEventListener>()
    .AddFiltering()
    .AddProjections()
    .AddSorting()
    .AddMutationType<MutationType>()
    .AddMutationConventions()
    .AddTypeExtension<ClientTypeExtensions>()
    .AddTypeExtension<SubscriberTypeExtensions>()
    .AddGlobalObjectIdentification()
    .RegisterDbContext<UserDbContext>(DbContextKind.Pooled)
    .RegisterService<IConnectionMultiplexer>()
    .RegisterService<IStripeClient>()
    .RegisterService<IMessageSender>()
    // TODO: this would be nice, but it breaks the stitching
    //.AddQueryFieldToMutationPayloads()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true)
    .InitializeOnStartup();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseHealthChecks("/health");

if (!app.Environment.IsDevelopment()) {
    app.UseAuthentication();
    app.UseAuthorization();

}
app.MapGraphQL()
    .RequireAuthorization(
    !builder.Environment.IsDevelopment()
    ? new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()
    : new AuthorizationPolicyBuilder().RequireAssertion(_ => true).Build());

IdentityModelEventSource.ShowPII = app.Environment.IsDevelopment();

app.RunWithGraphQLCommands(args);

public partial class Program { }



