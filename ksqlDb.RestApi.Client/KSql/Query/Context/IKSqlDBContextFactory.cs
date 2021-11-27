namespace ksqlDB.RestApi.Client.KSql.Query.Context
{
  /// <summary>
  /// A factory for creating derived KSqlDBContext instances. 
  /// </summary>
  /// <typeparam name="TContext">The type of the context.</typeparam>
  public interface IKSqlDBContextFactory<out TContext>
    where TContext : IKSqlDBContext
  {
    /// <summary>
    /// Creates a new instance of a derived context.
    /// </summary>
    /// <returns>The created context.</returns>
    TContext Create();
  }
}