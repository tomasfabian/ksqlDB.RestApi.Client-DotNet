using ksqlDB.Api.Client.Tests.Helpers;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi;

internal static class TestKSqlDBContextOptions
{
  public static readonly KSqlDBContextOptions Instance = new (TestParameters.KsqlDBUrl)
  {
    JsonSerializerOptions =
      KSqlDbJsonSerializerOptions.CreateInstance()
  };
}