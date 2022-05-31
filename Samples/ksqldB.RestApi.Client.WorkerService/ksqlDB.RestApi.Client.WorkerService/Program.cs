using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.WorkerService;
using ksqlDB.RestApi.Client.WorkerService.ksqlDB;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
      var ksqlDbUrl = @"http:\\localhost:8088"; // for local dev. provide your IP address
      ksqlDbUrl = @"http:\\172.30.48.1:8088";

      services.AddDbContext<IMoviesKSqlDbContext, MoviesKSqlDbContext>(
        options =>
        {
          var setupParameters = options.UseKSqlDb(ksqlDbUrl);

          setupParameters.SetAutoOffsetReset(AutoOffsetReset.Earliest);

        }, ServiceLifetime.Transient);

      services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();