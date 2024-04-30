using Aggregations.Model;
using ksqlDB.RestApi.Client.KSql.Query.Context;

namespace Aggregations.KSqlDbContext;

internal class ApplicationKSqlDbContext : KSqlDBContext, IApplicationKSqlDbContext
{
  public ApplicationKSqlDbContext(string ksqlDbUrl, ILoggerFactory? loggerFactory = null)
    : base(ksqlDbUrl, loggerFactory)
  {
  }

  public ApplicationKSqlDbContext(KSqlDBContextOptions contextOptions, ILoggerFactory? loggerFactory = null)
    : base(contextOptions, loggerFactory)
  {
  }

  public ksqlDB.RestApi.Client.KSql.Linq.IQbservable<Tweet> Tweets => CreatePushQuery<Tweet>();
}

public interface IApplicationKSqlDbContext : IKSqlDBContext
{
  ksqlDB.RestApi.Client.KSql.Linq.IQbservable<Tweet> Tweets { get; }
}
