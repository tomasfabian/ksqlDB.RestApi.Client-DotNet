using System;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Context.Options
{
  public interface ISetupParameters : ICreateOptions
  {
    ISetupParameters SetupQuery(Action<IQueryOptions> configure);
#if !NETSTANDARD
    ISetupParameters SetupQueryStream(Action<IQueryOptions> configure);
#endif
  }
}