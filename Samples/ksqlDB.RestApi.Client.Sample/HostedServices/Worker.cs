using System;
using System.Threading;
using System.Threading.Tasks;
using ksqlDB.Api.Client.Samples.Models.Movies;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ksqlDB.Api.Client.Samples.HostedServices
{
  public class Worker : IHostedService, IDisposable
  {
    private readonly IKSqlDBContext context;
    private readonly IKSqlDbRestApiClient restApiClient;
    private readonly ILogger logger;

    public Worker(IKSqlDBContext context, IKSqlDbRestApiClient restApiClient, ILoggerFactory loggerFactory)
    {
      this.context = context ?? throw new ArgumentNullException(nameof(context));
      this.restApiClient = restApiClient ?? throw new ArgumentNullException(nameof(restApiClient));

      logger = loggerFactory.CreateLogger<Worker>();
    }

    private Subscription subscription;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      logger.LogInformation("Starting");

      subscription = await context.CreateQueryStream<Movie>()
        .WithOffsetResetPolicy(AutoOffsetReset.Earliest)
        .SubscribeAsync(
          movie =>
          {
          },
          onError: e =>
           {

           },
          onCompleted: () => { }, cancellationToken: cancellationToken);
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
      logger.LogInformation("Stopping.");

      return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
  }
}