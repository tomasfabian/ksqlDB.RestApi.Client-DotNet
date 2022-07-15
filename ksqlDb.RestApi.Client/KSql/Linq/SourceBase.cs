using ksqlDB.RestApi.Client.KSql.Query.Windows;

namespace ksqlDB.RestApi.Client.KSql.Linq;

public class SourceBase
{
  internal Duration DurationBefore { get; set; }
  internal Duration DurationAfter { get; set; }
}