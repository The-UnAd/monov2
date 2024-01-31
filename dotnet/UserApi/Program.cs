using HotChocolate.AspNetCore.Serialization;
using HotChocolate.Execution.Serialization;
//using UnAd.Auth.Web;
using UnAd.Data.Users;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
//using Microsoft.IdentityModel.Logging;
using StackExchange.Redis;
using UserApi;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(b => b.FormatterName = ConsoleFormatterNames.Systemd)
    .AddDebug();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(typeof(ILogger), c => {
    var logger = c.GetRequiredService<ILoggerFactory>()
        .CreateLogger("UserApi");
    return logger;
});

builder.Services.AddTransient<IConnectionMultiplexer>(c =>
    ConnectionMultiplexer.Connect(c.GetRequiredService<IConfiguration>()["REDIS_URL"]!));

builder.Services.AddExceptionHandler(o => o.ExceptionHandler = context => {
    var exception = context.Features.Get<IExceptionHandlerFeature>()
        ?.Error;
    var logger = context.Features.Get<ILogger>();
    logger?.LogError(new EventId(), exception, "Unhandled exception: {Message}", exception?.Message);
    return Task.CompletedTask;
});

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddCognitoJwtBearer($"https://{builder.Configuration["COGNITO_ENDPOINT"]}", !builder.Environment.IsDevelopment());
//builder.Services.AddAuthorization();

builder.Services.AddPooledDbContextFactory<UserDbContext>(o
    => o.UseNpgsql($"{builder.Configuration["DB_CONNECTIONSTRING"]};Database=userdb;"));

builder.Services.AddHttpResponseFormatter(new HttpResponseFormatterOptions {
    Json = new JsonResultFormatterOptions {
        NullIgnoreCondition = JsonNullIgnoreCondition.Fields
    }
});

builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType<Query>()
    .AddTypeExtension<RoleTypeExtensions>()
    .AddTypeExtension<UserTypeExtensions>()
    .AddGlobalObjectIdentification()
    .AddProjections()
    .RegisterDbContext<UserDbContext>(DbContextKind.Pooled)
    .ModifyRequestOptions(opt =>
        opt.IncludeExceptionDetails = builder.Environment.IsDevelopment())
    .InitializeOnStartup();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();

app.UseHealthChecks("/health");

//app.UseAuthentication();
//app.UseAuthorization();

app.MapGraphQL();
    //.RequireAuthorization();

//IdentityModelEventSource.ShowPII = app.Environment.IsDevelopment();

app.RunWithGraphQLCommands(args);

public partial class Program { }



