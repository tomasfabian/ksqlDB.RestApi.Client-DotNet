# KSqlDbContext

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
