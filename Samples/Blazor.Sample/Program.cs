using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Blazor.Sample.HostedServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor.Sample
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
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
  }
}