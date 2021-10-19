using System;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

namespace ksqlDB.RestApi.Client.KSql.Query.Context.Options
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