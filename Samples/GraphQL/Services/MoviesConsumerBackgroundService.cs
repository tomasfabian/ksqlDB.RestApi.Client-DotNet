using System.Reactive;
using GraphQL.ksqlDB;
using GraphQL.Model;
using GraphQL.Movies;
using HotChocolate.Subscriptions;

namespace GraphQL.Services;

public class MoviesConsumerBackgroundService(IMoviesKSqlDbContext context, [Service] ITopicEventSender sender, ILogger<MoviesConsumerBackgroundService> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);

    var observer = Observer.Create<Movie>(async movie => { await sender.SendAsync(nameof(Subscription.MovieAdded), movie, stoppingToken); });
    await context.Movies.SubscribeAsync(observer, stoppingToken);
  }
}
