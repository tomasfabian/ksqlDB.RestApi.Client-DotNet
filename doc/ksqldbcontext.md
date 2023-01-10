# KSqlDbContext

### Creating query streams
**v1.0.0**

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

var ksqlDbUrl = @"http:\\localhost:8088";
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

[Run a query](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-rest-api/query-endpoint/#post-query)
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

var ksqlDbUrl = @"http:\\localhost:8088";
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
netstandard 2.0 does not support Http 2.0. Due to this ```IKSqlDBContext.CreateQueryStream<TEntity>``` is not exposed at the current version. 
For these reasons ```IKSqlDBContext.CreateQuery<TEntity>``` was introduced to provide the same functionality via Http 1.1. 

## Basic auth
**v1.0.0**

In ksqldb you can use the [Http-Basic authentication](https://docs.ksqldb.io/en/latest/operate-and-deploy/installation/server-config/security/#configuring-listener-for-http-basic-authenticationauthorization) mechanism:
```C#
using ksqlDB.RestApi.Client.KSql.Query.Context.Options;

string ksqlDbUrl = @"http:\\localhost:8088";

string userName = "fred";
string password = "letmein";

var options = new KSqlDbContextOptionsBuilder()
  .UseKSqlDb(ksqlDbUrl)
  .SetBasicAuthCredentials(userName, password)
  .Options;

await using var context = new KSqlDBContext(options);
```

See also how to [intercept http requests](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/wiki/Interception-of-HTTP-requests-in-ksqlDB.RestApi.Client---Authentication)

### KSqlDbServiceCollectionExtensions - AddDbContext and AddDbContextFactory
**v1.4.0**

- AddDbContext - Registers the given ksqldb context as a service in the IServiceCollection
- AddDbContextFactory - Registers the given ksqldb context factory as a service in the IServiceCollection

```C#
using ksqlDB.Api.Client.Samples;
using ksqlDB.Api.Client.Samples.Models.Movies;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
```

```C#
var serviceCollection = new ServiceCollection();

var ksqlDbUrl = @"http:\\localhost:8088";

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

### IKSqlDBContextFactory
**v1.4.0**

A factory for creating derived KSqlDBContext instances.

```C#
var contextFactory = serviceCollection.BuildServiceProvider().GetRequiredService<IKSqlDBContextFactory<IKSqlDBContext>>();

var context = contextFactory.Create();
```


### Logging info and ConfigureKSqlDb
**v1.2.0**

Bellow code demonstrates two new concepts. Logging and registration of services.

`KSqlDbServiceCollectionExtensions.ConfigureKSqlDb` - registers the following dependencies:

- IKSqlDBContext with Scoped ServiceLifetime. Can be altered with `contextLifetime` parameter.
- IKSqlDbRestApiClient with Scoped ServiceLifetime.
- IHttpClientFactory with Singleton ServiceLifetime.
- KSqlDBContextOptions with Singleton ServiceLifetime.

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
                             var ksqlDbUrl = @"http:\\localhost:8088";

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

With IKSqlDBContext.Add and IKSqlDBContext.SaveChangesAsync you can add multiple entities to the context and save them asynchronously in one request (as "batch inserts").
Internally it doesn't provide an entity change tracker, but merely caches insert statement snapshots which will be executed during `SaveChanges`;

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

The initial convention is that all writeable public instance properties and fields are taken into account during the Insert into statement generation.

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

await using KSqlDBContext context = new KSqlDBContext(@"http:\\localhost:8088");

var model = new Foo("Bar") {
  Count = 3
};

context.Add(model, insertProperties);

var responseMessage = await context.SaveChangesAsync();
```

### KSqlDbContextOptionsBuilder
> ⚠ KSqlDBContextOptions created with a constructor or by KSqlDbContextOptionsBuilder sets auto.offset.reset to earliest by default.
> This was changed in version 2.0.0

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

Enable exactly-once or at_least_once semantics

```C#
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.Query.Options;
```
```C#
var ksqlDbUrl = @"http:\\localhost:8088";

var contextOptions = new KSqlDbContextOptionsBuilder()
  .UseKSqlDb(ksqlDbUrl)
  .SetProcessingGuarantee(ProcessingGuarantee.AtLeastOnce)
  .Options;

await using var context = new KSqlDBContext(contextOptions);
```
