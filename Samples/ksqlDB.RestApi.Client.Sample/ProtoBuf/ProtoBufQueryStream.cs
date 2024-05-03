using System.Reactive.Linq;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDb.RestApi.Client.ProtoBuf.KSql.Query.Context;
using ksqlDB.RestApi.Client.Samples.Models.Movies;

namespace ksqlDB.RestApi.Client.Samples.ProtoBuf;

public class ProtoBufQueryStream
{
  public static async Task StartAsync()
  {
    var ksqlDbUrl = "http://localhost:8088";

    await using var context = new ProtoBufKSqlDbContext(ksqlDbUrl);

    var query = context.CreatePushQuery<MovieProto>("movie")
      .Where(p => p.Title != "E.T.")
      .Where(c => c.Title.ToLower().Contains("hard".ToLower()) || c.Id == 1)
      .Select(l => new { l.Id, l.Title, ReleaseYear = l.Release_Year })
      .Take(2); // LIMIT 2    

    var ksql = query.ToQueryString();

    Console.WriteLine("Generated ksql:");
    Console.WriteLine(ksql);
    Console.WriteLine();

    using var disposable = query
      .ToObservable() // client side processing starts here lazily after subscription. Switches to Rx.NET
      .Finally(() => { Console.WriteLine("Finally"); })
      .Subscribe(onNext: movie =>
      {
        Console.WriteLine($"{nameof(Movie)}: {movie.Id} - {movie.Title}");
        Console.WriteLine();
      }, onError: error => { Console.WriteLine($"Exception: {error.Message}"); }, onCompleted: () => Console.WriteLine("Completed"));


    Console.WriteLine("Press any key to stop the subscription");

    Console.ReadKey();
  }
}
