using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace UnAd.Data.Migrator;

public class Program
{
    public static void Main(string[] args)
        => CreateHostBuilder(args).Build().Run();

    public static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(c => c.AddEnvironmentVariables())
        .ConfigureServices((context, services)
            =>
        {
            services.AddDbContext<Users.UserDbContext>(o
            => o.UseNpgsql($"{context.Configuration["DB_CONNECTIONSTRING"]};Database=userdb"));
            services.AddDbContext<Comms.CommsDbContext>(o
            => o.UseNpgsql($"{context.Configuration["DB_CONNECTIONSTRING"]};Database=commsdb"));
            services.AddDbContext<Patients.PatientDbContext>(o
            => o.UseNpgsql($"{context.Configuration["DB_CONNECTIONSTRING"]};Database=patientdb"));
        });
}


