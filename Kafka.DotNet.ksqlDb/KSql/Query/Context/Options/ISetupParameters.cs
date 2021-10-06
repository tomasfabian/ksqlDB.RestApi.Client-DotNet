using System;
using Kafka.DotNet.ksqlDB.KSql.Query.Options;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Context.Options
{
  public interface ISetupParameters : ICreateOptions
  {
    ISetupParameters SetProcessingGuarantee(ProcessingGuarantee processingGuarantee);
    ISetupParameters SetAutoOffsetReset(AutoOffsetReset autoOffsetReset);

    ISetupParameters SetupQuery(Action<IQueryOptions> configure);
#if !NETSTANDARD
    ISetupParameters SetupQueryStream(Action<IQueryOptions> configure);
#endif
  }
}