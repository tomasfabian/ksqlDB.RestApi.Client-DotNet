using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Blazor.Sample.Data;
using Blazor.Sample.HostedServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor.Sample
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      var host = CreateHostBuilder(args).Build();

      await TryMigrateDatabaseAsync(host);

      await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
          .UseServiceProviderFactory(new AutofacServiceProviderFactory())
          .ConfigureAppConfiguration((context, configBuilder) =>
          {
            configBuilder.AddEnvironmentVariables();
          })
          .ConfigureServices(services => services.AddHostedService<IoTSimulatorService>())
          .ConfigureWebHostDefaults(webBuilder =>
          {
            webBuilder.UseStartup<Startup>();
          });

    private static async Task TryMigrateDatabaseAsync(IHost host)
    {
      using var scope = host.Services.CreateScope();
      var hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

      if (hostEnvironment.IsDevelopment() || hostEnvironment.IsStaging())
      {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
      }
    }
  }
}