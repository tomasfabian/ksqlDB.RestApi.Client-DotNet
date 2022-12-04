# KSqlDbContext

### KSqlDbContextOptionsBuilder SetProcessingGuarantee
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

## Basic auth
**v1.0.0**

In ksqldb you can use the [Http-Basic authentication](https://docs.ksqldb.io/en/latest/operate-and-deploy/installation/server-config/security/#configuring-listener-for-http-basic-authenticationauthorization) mechanism:
```C#
string ksqlDbUrl = @"http:\\localhost:8088";

string userName = "fred";
string password = "letmein";

var options = ClassUnderTest.UseKSqlDb(ksqlDbUrl)
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
