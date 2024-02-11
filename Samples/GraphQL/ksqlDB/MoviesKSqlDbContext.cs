using GraphQL.Model;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;

namespace GraphQL.ksqlDB;

internal class MoviesKSqlDbContext : KSqlDBContext, IMoviesKSqlDbContext
{
  public MoviesKSqlDbContext(string ksqlDbUrl, ILoggerFactory? loggerFactory = null)
    : base(ksqlDbUrl, loggerFactory)
  {
  }

  public MoviesKSqlDbContext(KSqlDBContextOptions contextOptions, ILoggerFactory? loggerFactory = null)
    : base(contextOptions, loggerFactory)
  {
  }

  public IQbservable<Movie> Movies => CreateQueryStream<Movie>();
}
