# ksqlDB.RestApi.Client.ProtoBuf
- adds support for Protobuf content type. The package uses [protobuf-net](https://github.com/protobuf-net/protobuf-net).

Install:
```
dotnet add package ksqlDb.RestApi.Client.ProtoBuf
```

Content-type
```
application/vnd.ksql.v1+protobuf
```

```C#
using System.Reactive.Linq;
using ksqlDB.Api.Client.Samples.Models.Movies;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDb.RestApi.Client.ProtoBuf.KSql.Query;
using ProtoBuf;

var ksqlDbUrl = @"http:\\localhost:8088";

await using var context = new ProtoBufKSqlDbContext(ksqlDbUrl);

var query = context.CreateQueryStream<MovieProto>("movie") // query-stream endpoint
  .Where(p => p.Title != "E.T.")
  .Where(c => c.Title.ToLower().Contains("hard".ToLower()) || c.Id == 1)
  .Select(l => new { Id = l.Id, l.Title })
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


[ProtoContract]
record MovieProto
{
  [ProtoMember(1)]
  public string Title { get; set; } = null!;

  [ProtoMember(2)]
  public int Id { get; set; }
}
```
