# ksqlDB.RestApi.Client.ProtoBuf

- adds support for **Protobuf** content type. The package uses [protobuf-net](https://github.com/protobuf-net/protobuf-net).
- [Protocol Buffers](https://protobuf.dev/), which is a language-agnostic data serialization format developed by Google.
- It is designed to efficiently and reliably serialize structured data for communication between different systems or for storing data.
 
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

The '/query' and '/query-stream' endpoints in `ksqlDB` support a **PROTOBUF content type**, which allows querying rows serialized in the PROTOBUF format.
The `ProtoBufKSqlDbContext` utilizes this serialization format by specifying it in the **Accept header**:

Content-type
```
application/vnd.ksql.v1+protobuf
```

In this case, the entity type is annotated with the `ProtoContract` attribute, while its properties are annotated with the `ProtoMember` attribute.

```C#
using ProtoBuf;

[ProtoContract]
record MovieProto
{
  [ProtoMember(1)]
  public string Title { get; set; } = null!;

  [ProtoMember(2)]
  public int Id { get; set; }
}
```

The **querying mechanism** remains unchanged when using the PROTOBUF serialization format, similar to the JSON serialization format.

```C#
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDb.RestApi.Client.ProtoBuf.KSql.Query.Context;

var ksqlDbUrl = @"http://localhost:8088";

await using var context = new ProtoBufKSqlDbContext(ksqlDbUrl);

var query = context.CreateQueryStream<MovieProto>("movie")
  .Where(p => p.Title != "E.T.")
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
```
