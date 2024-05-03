using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDb.RestApi.Client.ProtoBuf.KSql.RestApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
#if !NETSTANDARD
using KSqlDbQueryStreamProvider = ksqlDb.RestApi.Client.ProtoBuf.KSql.RestApi.KSqlDbQueryStreamProvider;
#endif

namespace ksqlDb.RestApi.Client.ProtoBuf.KSql.Query.Context;

public class ProtoBufKSqlDbContext : KSqlDBContext
{
  /// <summary>
  /// Initializes a new instance of the ProtoBufKSqlDbContext class with the specified ksqlDB server URL.
  /// </summary>
  /// <param name="ksqlDbUrl">The URL of the ksqlDB server.</param>
  /// <param name="loggerFactory">An optional logger factory to use for logging (defaults to null).</param>
  public ProtoBufKSqlDbContext(string ksqlDbUrl, ILoggerFactory? loggerFactory = null)
    : this(new KSqlDBContextOptions(ksqlDbUrl), loggerFactory)
  {
  }

  /// <summary>
  /// Initializes a new instance of the ProtoBufKSqlDbContext class with the specified ksqlDB server URL.
  /// </summary>
  /// <param name="ksqlDbUrl">The URL of the ksqlDB server.</param>
  /// <param name="modelBuilder">The model builder.</param>
  /// <param name="loggerFactory">An optional logger factory to use for logging (defaults to null).</param>
  public ProtoBufKSqlDbContext(string ksqlDbUrl, ModelBuilder modelBuilder, ILoggerFactory? loggerFactory = null)
    : this(new KSqlDBContextOptions(ksqlDbUrl), modelBuilder, loggerFactory)
  {
  }

  /// <summary>
  /// Initializes a new instance of the ProtoBufKSqlDbContext class with the specified context options.
  /// </summary>
  /// <param name="contextOptions">The options for configuring the KSqlDBContext.</param>
  /// <param name="loggerFactory">An optional logger factory to use for logging (defaults to null).</param>
  public ProtoBufKSqlDbContext(KSqlDBContextOptions contextOptions, ILoggerFactory? loggerFactory = null)
    : base(contextOptions, loggerFactory)
  {
  }

  /// <summary>
  /// Initializes a new instance of the ProtoBufKSqlDbContext class with the specified context options.
  /// </summary>
  /// <param name="contextOptions">The options for configuring the KSqlDBContext.</param>
  /// <param name="modelBuilder">The model builder.</param>
  /// <param name="loggerFactory">An optional logger factory to use for logging (defaults to null).</param>
  public ProtoBufKSqlDbContext(KSqlDBContextOptions contextOptions, ModelBuilder modelBuilder, ILoggerFactory? loggerFactory = null)
    : base(contextOptions, modelBuilder, loggerFactory)
  {
  }

  protected override void OnConfigureServices(IServiceCollection serviceCollection, KSqlDBContextOptions contextOptions)
  {
    switch (contextOptions.EndpointType)
    {
      case EndpointType.Query:
        serviceCollection.TryAddScoped<IKSqlDbProvider, KSqlDbQueryProvider>();
        break;
      case EndpointType.QueryStream:
        serviceCollection.TryAddScoped<IKSqlDbProvider, KSqlDbQueryStreamProvider>();

        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(contextOptions.EndpointType), contextOptions.EndpointType, "Non-exhaustive match");
    }

    base.OnConfigureServices(serviceCollection, contextOptions);
  }
}
