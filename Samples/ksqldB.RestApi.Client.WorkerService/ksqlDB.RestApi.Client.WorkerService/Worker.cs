using System.Reactive.Linq;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.WorkerService.ksqlDB;
using ksqlDB.RestApi.Client.WorkerService.Models;

namespace ksqlDB.RestApi.Client.WorkerService;

public class Worker : IHostedService
{
  private readonly IMoviesKSqlDbContext context;
  private readonly ILogger<Worker> logger;

  public Worker(IMoviesKSqlDbContext context, ILogger<Worker> logger)
  {
    this.context = context;
    this.logger = logger;
  }

  public  Task StartAsync(CancellationToken cancellationToken)
  {
    logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);

    SubscribeToMovies();

    return Task.CompletedTask;
  }

  private IDisposable subscription = null!;

  private void SubscribeToMovies()
  {
    subscription = context.CreateQueryStream<Movie>()
      .Subscribe(onNext: movie =>
      {
        Console.WriteLine($"{nameof(Movie)}: {movie.Id} - {movie.Title} - {movie.RowTime}");
        Console.WriteLine();
      }, onError: error => { Console.WriteLine($"Exception: {error.Message}"); }, onCompleted: () => Console.WriteLine("Completed"));
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    logger.LogInformation("Stopping.");

    using (subscription)
    {
    }

    return Task.CompletedTask;
  }
}