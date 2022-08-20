using Blazor.Sample.Data;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using Microsoft.EntityFrameworkCore;
using ksqlDb.RestApi.Client.DependencyInjection;

namespace Blazor.Sample.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
  public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddRazorPages();
    services.AddServerSideBlazor();

    services.AddDbContext<IKSqlDBContext, KSqlDBContext>(options =>
    {
      string ksqlDbUrl = configuration["ksqlDb:Url"];

      var setupParameters = options.UseKSqlDb(ksqlDbUrl);

    }, contextLifetime: ServiceLifetime.Transient, restApiLifetime: ServiceLifetime.Transient);

    ConfigureEntityFramework(services, configuration);
  }

  public static void ConfigureEntityFramework(this IServiceCollection services, IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    services.AddDbContextFactory<ApplicationDbContext>(options => { options.UseSqlServer(connectionString); });

    services.AddScoped(p =>
      p.GetRequiredService<IDbContextFactory<ApplicationDbContext>>()
        .CreateDbContext());
  }
}