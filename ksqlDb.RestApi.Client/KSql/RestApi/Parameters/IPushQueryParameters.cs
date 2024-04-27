using ksqlDB.RestApi.Client.KSql.Query.Options;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

/// <summary>
/// Represents parameters for a push query.
/// </summary>
public interface IPushQueryParameters : IKSqlDbParameters
{
  AutoOffsetReset AutoOffsetReset { get; set; }
}
