using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.WorkerService.Models;

namespace ksqlDB.RestApi.Client.WorkerService.ksqlDB;

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

  public IQbservable<Movie> Movies => CreatePushQuery<Movie>();
}
