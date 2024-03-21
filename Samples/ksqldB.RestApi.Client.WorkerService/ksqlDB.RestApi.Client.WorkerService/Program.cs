using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.WorkerService;
using ksqlDB.RestApi.Client.WorkerService.ksqlDB;

// use command ksqlDB.RestApi.Client-DotNet\Samples\ksqlDB.RestApi.Client.Sample> docker compose up -d
var ksqlDbUrl = Configuration().GetValue<string>("ksqlDbUrl") ?? throw new Exception("ksqlDbUrl is missing in appsettings.json");

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
      services.AddDbContext<IMoviesKSqlDbContext, MoviesKSqlDbContext>(
        options =>
        {
          var setupParameters = options.UseKSqlDb(ksqlDbUrl);

          setupParameters.SetAutoOffsetReset(AutoOffsetReset.Earliest);

        }, contextLifetime: ServiceLifetime.Transient, restApiLifetime: ServiceLifetime.Transient);

      services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();

static IConfiguration Configuration()
{
  return new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();
}
