using System;
using System.Threading;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ksqlDB.Api.Client.Samples.HostedServices
{
  public class Worker : IHostedService
  {
    private readonly IKSqlDBContextFactory<Program.IApplicationKSqlDbContext> contextFactory;
    private readonly IKSqlDBContext context;
    private readonly IKSqlDbRestApiClient restApiClient;
    private readonly ILogger logger;

    public Worker(IKSqlDBContextFactory<Program.IApplicationKSqlDbContext> contextFactory, Program.IApplicationKSqlDbContext context, IKSqlDbRestApiClient restApiClient, ILoggerFactory loggerFactory)
    {
      this.contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
      this.context = context ?? throw new ArgumentNullException(nameof(context));
      this.restApiClient = restApiClient ?? throw new ArgumentNullException(nameof(restApiClient));

      logger = loggerFactory.CreateLogger<Worker>();
    }

    private Subscription subscription;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      logger.LogInformation("Starting");

      var newContext = contextFactory.Create();

      try
      {
        subscription = await newContext.Movies
          .WithOffsetResetPolicy(AutoOffsetReset.Earliest)
          .SubscribeAsync(
            movie =>
            {
              Console.WriteLine(movie.Title);
            },
            onError: e =>
            {
              Console.WriteLine($"Error: {e.Message}");
            },
            onCompleted: () => { Console.WriteLine("Completed"); }, cancellationToken: cancellationToken);

        Console.WriteLine($"Query id {subscription.QueryId}");
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
      }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      logger.LogInformation("Stopping.");

      return Task.CompletedTask;
    }
  }
}