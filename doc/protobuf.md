# ksqlDB.RestApi.Client.ProtoBuf

- adds support for Protobuf content type. The package uses [protobuf-net](https://github.com/protobuf-net/protobuf-net).

Install:
```
dotnet add package ksqlDb.RestApi.Client.ProtoBuf
```
or with .NET CLI
```
dotnet add package ksqlDb.RestApi.Client.ProtoBuf
```
This adds a `<PackageReference>` to your csproj file, similar to the following:
```XML
<PackageReference Include="ksqlDb.RestApi.Client.ProtoBuf" Version="2.0.0" />
```

Content-type
```
application/vnd.ksql.v1+protobuf
```

```C#
using ProtoBuf;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDb.RestApi.Client.ProtoBuf.KSql.Query.Context;

var ksqlDbUrl = @"http://localhost:8088";

await using var context = new ProtoBufKSqlDbContext(ksqlDbUrl);

var query = context.CreateQueryStream<MovieProto>("movie")
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
