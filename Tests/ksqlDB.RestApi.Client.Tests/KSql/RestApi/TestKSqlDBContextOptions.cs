using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using ksqlDb.RestApi.Client.Tests.Helpers;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi;

internal static class TestKSqlDBContextOptions
{
  public static readonly KSqlDBContextOptions Instance = new (TestParameters.KsqlDbUrl)
  {
    JsonSerializerOptions =
      KSqlDbJsonSerializerOptions.CreateInstance()
  };
}
