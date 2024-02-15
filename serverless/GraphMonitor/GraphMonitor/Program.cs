using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

var builder = WebApplication.CreateBuilder(args);

new Startup().ConfigureServices(builder.Services);

var app = builder.Build();

app.Run();

public partial class Program { }



