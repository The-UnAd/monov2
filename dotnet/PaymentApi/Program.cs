using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using PaymentApi;
using UnAd.Data.Products;
using PaymentApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddTransient<IConnectionMultiplexer>(c =>
    ConnectionMultiplexer.Connect(c.GetRequiredService<IConfiguration>().GetRedisUrl()));

builder.Services.AddPooledDbContextFactory<ProductDbContext>((s, o) =>
    o.UseNpgsql(s.GetRequiredService<IConfiguration>().GetConnectionString("ProductDb")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = builder.Configuration.GetCognitoAuthority();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateAudience = false
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
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
    .AddFiltering()
    .AddProjections()
    .AddSorting()
    .AddMutationConventions()
    .AddGlobalObjectIdentification()
    //.AddDirectiveType(typeof(DelegateDirectiveType))
    .AddQueryType<QueryType>()
    .AddMutationType<MutationType>()
    .AddTypeExtension<PlanType>()
    .AddTypeExtension<SubscriptionType>()
    .AddDiagnosticEventListener<LoggerExecutionEventListener>()
    .RegisterDbContext<ProductDbContext>(DbContextKind.Pooled)
    .RegisterService<IConnectionMultiplexer>()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true)
    .InitializeOnStartup();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseHealthChecks("/health");

if (!app.Environment.IsDevelopment())
{
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



