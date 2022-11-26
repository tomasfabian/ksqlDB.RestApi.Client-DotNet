using Microsoft.Extensions.DependencyInjection;

namespace ksqlDB.RestApi.Client.KSql.Query.Context;

/// <summary>
/// A factory for creating derived KSqlDBContext instances. 
/// </summary>
/// <typeparam name="TContext">The type of the context.</typeparam>
internal class KSqlDBContextFactory<TContext> : IKSqlDBContextFactory<TContext> 
  where TContext : IKSqlDBContext
{
  private readonly IServiceProvider serviceProvider;

  public KSqlDBContextFactory(IServiceProvider serviceProvider)
  {
    this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
  }

  /// <summary>
  /// Creates a new instance of a derived context.
  /// </summary>
  /// <returns>The created context.</returns>
  public TContext Create()
  {
    return serviceProvider.GetRequiredService<TContext>();
  }
}