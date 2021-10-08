using System;
using Kafka.DotNet.ksqlDB.KSql.Query.Options;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Context.Options
{
  public interface ISetupParameters : ICreateOptions
  {
    ISetupParameters SetProcessingGuarantee(ProcessingGuarantee processingGuarantee);
    ISetupParameters SetAutoOffsetReset(AutoOffsetReset autoOffsetReset);

    ISetupParameters SetupQuery(Action<IKSqlDbParameters> configure);
#if !NETSTANDARD
    ISetupParameters SetupQueryStream(Action<IKSqlDbParameters> configure);
#endif
    ISetupParameters SetBasicAuthCredentials(string username, string password);
  }
}