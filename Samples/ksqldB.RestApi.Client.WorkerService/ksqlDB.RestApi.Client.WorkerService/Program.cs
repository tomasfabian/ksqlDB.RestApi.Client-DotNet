using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.WorkerService;
using ksqlDB.RestApi.Client.WorkerService.ksqlDB;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
      var ksqlDbUrl = @"http://ksqldb-server:8088"; // use command ksqlDB.RestApi.Client-DotNet\Samples\ksqlDB.RestApi.Client.Sample> docker-compose up -d

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
