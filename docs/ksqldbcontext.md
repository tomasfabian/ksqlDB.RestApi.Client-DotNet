# KSqlDbContext

**KSqlDbContext** provides **querying** capabilities, allowing developers to express complex queries against the ksql database using a higher-level query language.
This enables retrieval of specific data based on filtering conditions, limiting, and other criteria in a more compile type safe way.
It also exposes a method to perform operations to **create** records.

### Creating query streams
**v1.0.0**

Within the `ksqlDB.RestApi.Client` .NET client library, the `KSqlDBContext` class is responsible for handling the sending of requests to the `/query-stream` endpoint in `ksqlDB` using the **HTTP 2.0 protocol**.

[Executing pull or push queries](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-rest-api/streaming-endpoint/#executing-pull-or-push-queries)
```JSON
POST /query-stream HTTP/2.0
Accept: application/vnd.ksqlapi.delimited.v1
Content-Type: application/vnd.ksqlapi.delimited.v1

{
  "sql": "SELECT * FROM movies EMIT CHANGES;",
  "properties": {
    "auto.offset.reset": "earliest"
  }
}
```

```C#
using System;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.Sample.Models.Movies;

var ksqlDbUrl = @"http://localhost:8088";
var contextOptions = new KSqlDBContextOptions(ksqlDbUrl);

await using var context = new KSqlDBContext(contextOptions);

using var disposable = context.CreateQueryStream<Movie>()
  .Subscribe(onNext: movie =>
  {
    Console.WriteLine($"{nameof(Movie)}: {movie.Id} - {movie.Title} - {movie.RowTime}");
    Console.WriteLine();
  }, onError: error => { Console.WriteLine($"Exception: {error.Message}"); }, onCompleted: () => Console.WriteLine("Completed"));
```

### Creating queries
**v1.0.0**

To **post queries** to `ksqlDB`, you can use the **ksqlDB REST API**.
The process of posting queries to `ksqlDB` is encapsulated within the `KSqlDBContext` in the `ksqlDB.RestApi.Client` .NET client library.

[Post a query](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-rest-api/query-endpoint/#post-query)
```JSON
POST /query HTTP/1.1
Accept: application/vnd.ksql.v1+json
Content-Type: application/vnd.ksql.v1+json

{
  "ksql": "SELECT * FROM movies EMIT CHANGES;",
  "streamsProperties": {
    "ksql.streams.auto.offset.reset": "earliest"
  }
}
```
```C#
using System;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.Sample.Models.Movies;

var ksqlDbUrl = @"http://localhost:8088";
var contextOptions = new KSqlDBContextOptions(ksqlDbUrl);

await using var context = new KSqlDBContext(contextOptions);

using var disposable = context.CreateQuery<Movie>()
  .Subscribe(onNext: movie =>
  {
    Console.WriteLine($"{nameof(Movie)}: {movie.Id} - {movie.Title} - {movie.RowTime}");
    Console.WriteLine();
  }, onError: error => { Console.WriteLine($"Exception: {error.Message}"); }, onCompleted: () => Console.WriteLine("Completed"));
```

# TFM netstandard 2.0 (.Net Framework, NetCoreApp 2.0 etc.)
The lack of support for **HTTP 2.0** in netstandard 2.0 prevents the exposure of `IKSqlDBContext.CreateQueryStream<TEntity>` in the current version. To address this limitation, `IKSqlDBContext.CreateQuery<TEntity>` was introduced as an alternative solution utilizing **HTTP 1.1** to provide the same functionality.

## Basic auth
**v1.0.0**

In `ksqlDB` you can use the [Http-Basic authentication](https://docs.ksqldb.io/en/latest/operate-and-deploy/installation/server-config/security/#configuring-listener-for-http-basic-authenticationauthorization) mechanism:
```C#
using ksqlDB.RestApi.Client.KSql.Query.Context.Options;

string ksqlDbUrl = @"http://localhost:8088";

string userName = "fred";
string password = "letmein";

var options = new KSqlDbContextOptionsBuilder()
  .UseKSqlDb(ksqlDbUrl)
  .SetBasicAuthCredentials(userName, password)
  .Options;

await using var context = new KSqlDBContext(options);
```

See also how to [intercept http requests](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/wiki/Interception-of-HTTP-requests-in--ksqlDB.RestApi.Client-DotNet---Authentication)

### KSqlDbServiceCollectionExtensions - AddDbContext and AddDbContextFactory
**v1.4.0**

- `AddDbContext` - registers the given ksqlDB context as a service in the `IServiceCollection`
- `AddDbContextFactory` - registers the given ksqlDB context factory as a service in the `IServiceCollection`

```C#
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
```

```C#
var serviceCollection = new ServiceCollection();

var ksqlDbUrl = @"http://localhost:8088";

serviceCollection.AddDbContext<ApplicationKSqlDbContext, IApplicationKSqlDbContext>(options =>
  options.UseKSqlDb(ksqlDbUrl), contextLifetime: ServiceLifetime.Transient);

serviceCollection.AddDbContextFactory<IApplicationKSqlDbContext>(factoryLifetime: ServiceLifetime.Scoped);
```

```C#
internal class ApplicationKSqlDbContext : KSqlDBContext, Program.IApplicationKSqlDbContext
{
  public ApplicationKSqlDbContext(string ksqlDbUrl, ILoggerFactory loggerFactory = null)
    : base(ksqlDbUrl, loggerFactory)
  {
  }

  public ApplicationKSqlDbContext(KSqlDBContextOptions contextOptions, ILoggerFactory loggerFactory = null)
    : base(contextOptions, loggerFactory)
  {
  }

  public ksqlDB.RestApi.Client.KSql.Linq.IQbservable<Movie> Movies => CreateQueryStream<Movie>();
}

public interface IApplicationKSqlDbContext : IKSqlDBContext
{
  ksqlDB.RestApi.Client.KSql.Linq.IQbservable<Movie> Movies { get; }
}
```

```C#
public record Movie
{
  public int Id { get; set; }
  public string Title { get; set; } = null!;
  public int Release_Year { get; set; }
}
```

### IKSqlDBContextFactory
**v1.4.0**

A factory for creating derived `KSqlDBContext` instances.

```C#
var contextFactory = serviceCollection.BuildServiceProvider().GetRequiredService<IKSqlDBContextFactory<IKSqlDBContext>>();

var context = contextFactory.Create();
```


### Logging info and ConfigureKSqlDb
**v1.2.0**

Bellow code demonstrates two new concepts. Logging and registration of services.

In this example, we use the `Microsoft.Extensions.Logging` library to add **console and debug logging providers**. You can also add additional providers like file-based logging or third-party providers.

In .NET, the `ConfigureServices` extension method is a commonly used method to configure services, including 3rd party services like `KSqlDbContext`, in the **dependency injection container**.
The `ConfigureKSqlDb` extension method is used to **register** ksqlDB-related service implementations with the `IServiceCollection`.

`KSqlDbServiceCollectionExtensions.ConfigureKSqlDb` - registers the following dependencies:

- `IKSqlDBContext` with **Scoped** ServiceLifetime. Can be altered with `contextLifetime` parameter.
- `IKSqlDbRestApiClient` with **Scoped** ServiceLifetime.
- `IHttpClientFactory` with **Singleton** ServiceLifetime.
- `KSqlDBContextOptions` with **Singleton** ServiceLifetime.

```XML
<PackageReference Include="Microsoft.Extensions.Hosting" Version="5.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="5.0.1" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="5.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Console" Version="5.0.0" />
```

```C#
using System.Threading.Tasks;
using ksqlDB.Api.Client.Samples.HostedServices;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ksqlDB.Api.Client.Samples
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      await CreateHostBuilder(args).RunConsoleAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .ConfigureLogging((hostingContext, logging) =>
                          {
                            logging.AddConsole();
                            logging.AddDebug();
                          })
        .ConfigureServices((hostContext, serviceCollection) =>
                           {
                             var ksqlDbUrl = @"http://localhost:8088";

                             var setupAction = setupParameters =>
                                               {
                                                   setupParameters.SetAutoOffsetReset(AutoOffsetReset.Earliest);
                                               };

                             serviceCollection.ConfigureKSqlDb(ksqlDbUrl, setupAction);

                             serviceCollection.AddHostedService<Worker>();
                           });
  }
}
```

appsettings.json

```JSON
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "ksqlDb.RestApi.Client": "Information" // "Debug"
    }
  }
}
```

In .NET, a **hosted service** is a long-running background task or service that is managed by the .NET runtime environment. It is typically used for performing asynchronous or recurring operations, such as processing queues, executing scheduled tasks, or handling background data processing.

The example demonstrates how to inject dependencies related to `ksqlDB` operations.

```C#
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
        onNext: movie => { },
        onError: e => { },
        onCompleted: () => { },
        cancellationToken: cancellationToken);
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
```

### Add and SaveChangesAsync
**v1.3.0**

By leveraging the methods `Add` and `SaveChangesAsync` provided by the `IKSqlDBContext` interface, you have the capability to add multiple entities to the context and asynchronously save them in a single request, also known as **batch inserts**.
It's important to note that internally, this functionality does not include an **entity change tracker**.
Instead, it **caches snapshots** of the insert statements, which are executed when the `SaveChangesAsync` method is invoked.

```C#
private static async Task AddAndSaveChangesAsync(IKSqlDBContext context)
{
  context.Add(new Movie { Id = 1 });
  context.Add(new Movie { Id = 2 });

  var saveResponse = await context.SaveChangesAsync();
}
```

### Include read-only properties for inserts
**v1.3.1**

- Inserts - include read-only properties configuration

The default convention is to include **all public instance properties and fields** that are writable when generating the "INSERT INTO" statement.

```C#
public record Foo
{
  public Foo(string name)
  {
    Name = name;
  }

  public string Name { get; }
  public int Count { get; set; }
}
```

```C#
var insertProperties = new InsertProperties
                       {
                         IncludeReadOnlyProperties = true
                       };

await using KSqlDBContext context = new KSqlDBContext(@"http://localhost:8088");

var model = new Foo("Bar") {
  Count = 3
};

context.Add(model, insertProperties);

var responseMessage = await context.SaveChangesAsync();
```

### KSqlDbContextOptionsBuilder

`KSqlDbContextOptionsBuilder` provides a fluent API that allows you to configure various aspects of the `ksqlDB` context, such as the connection string, processing guarantee, and other options.

> ⚠When creating `KSqlDBContextOptions` using a constructor or through `KSqlDbContextOptionsBuilder`, the default behavior is to set the `auto.offset.reset` property to "earliest."

```C#
public static KSqlDBContextOptions CreateQueryStreamOptions(string ksqlDbUrl)
{
  var contextOptions = new KSqlDbContextOptionsBuilder()
    .UseKSqlDb(ksqlDbUrl)
    .SetupQueryStream(options =>
    {
    })
    .SetupQuery(options =>
    {
      options.Properties[QueryParameters.AutoOffsetResetPropertyName] = AutoOffsetReset.Latest.ToString().ToLower();
    })
    .Options;

  return contextOptions;
}
```

### Setting processing guarantee with KSqlDbContextOptionsBuilder
**v1.0.0**

When using the `ksqlDB.RestApi.Client`, you have the ability to configure the **processing guarantee** for your queries.
This can be done by making use of the `SetProcessingGuarantee` method from the `KSqlDbContextOptionsBuilder` class, which allows you to configure the `processing.guarantee` streams property.
Enable `exactly_once_v2` or `at_least_once` semantics:

```C#
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.Query.Options;
```
```C#
var ksqlDbUrl = @"http://localhost:8088";

var contextOptions = new KSqlDbContextOptionsBuilder()
  .UseKSqlDb(ksqlDbUrl)
  .SetProcessingGuarantee(ProcessingGuarantee.AtLeastOnce)
  .Options;

await using var context = new KSqlDBContext(contextOptions);
```

The `ProcessingGuarantee` enum offers three options: **ExactlyOnce**, **ExactlyOnceV2** and **AtLeastOnce**.


### Setting formatting for identifiers
**v3.5.0**

When using the `ksqlDB.RestApi.Client`, you have the ability to configure the formatting for identifiers in your queries.
This can be done by making use of the `SetIdentifierFormat` method from the `KSqlDbContextOptionsBuilder` class.
You can choose from these options:
* `None` - the default option where identifiers are not modified
* `Keywords` - when an identifier is a reserved keyword it is escaped using **backticks** `` ` ``
* `Always` - all identifiers are escaped using **backticks** `` ` ``
```C#
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.Query.Options;
```
```C#
var ksqlDbUrl = @"http://localhost:8088";

var contextOptions = new KSqlDbContextOptionsBuilder()
  .UseKSqlDb(ksqlDbUrl)
  .SetIdentitifierFormat(IdentifierFormat.Always)
  .Options;

await using var context = new KSqlDBContext(contextOptions);
```
