using HotChocolate.AspNetCore.Serialization;
using HotChocolate.Execution.Serialization;
using HotChocolate.Utilities;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using StackExchange.Redis;
using UnAd.Data.Users;
using UserApi;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(b => b.FormatterName = ConsoleFormatterNames.Systemd)
    .AddDebug();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(typeof(ILogger), c =>
    c.GetRequiredService<ILoggerFactory>().CreateLogger("UserApi"));

builder.Services.AddTransient<IConnectionMultiplexer>(c =>
    ConnectionMultiplexer.Connect(c.GetRequiredService<IConfiguration>()["REDIS_URL"]!));

builder.Services.AddExceptionHandler(o => o.ExceptionHandler = context => {
    var exception = context.Features.Get<IExceptionHandlerFeature>()
        ?.Error;
    var logger = context.Features.Get<ILogger>();
    if (logger is not null && exception is not null) {
        LogProgramException(logger, exception.Message, exception);
    }
    return Task.CompletedTask;
});

builder.Services.AddPooledDbContextFactory<UserDbContext>((s, o) =>
    o.UseNpgsql(s.GetRequiredService<IConfiguration>().GetConnectionString("UserDb")));

builder.Services.AddHttpResponseFormatter(new HttpResponseFormatterOptions {
    Json = new JsonResultFormatterOptions {
        NullIgnoreCondition = JsonNullIgnoreCondition.Fields
    }
});

builder.Services.AddSingleton<IChangeTypeProvider, GuidFormatter>();

builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType<QueryType>()
    //.AddPaging()
    .AddFiltering()
    .AddProjections()
    .AddSorting()
    .AddMutationType<MutationType>()
    .AddMutationConventions()
    .AddTypeExtension<ClientTypeExtensions>()
    .AddGlobalObjectIdentification()
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

public partial class Program {
    private static readonly Action<ILogger, string, Exception?> LogProgramException =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(1, "UnhandledException"), "Unexpected Error: {Message}");
}



