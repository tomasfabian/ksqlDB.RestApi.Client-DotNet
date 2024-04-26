using System.Text;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

internal static class QueryParametersExtensions
{
  internal static void FillFromInternal(this IQueryParameters destination, IQueryParameters source)
  {
    destination.Sql = source.Sql;

    foreach (var entry in source.Properties)
      destination.Properties.Add(entry.Key, entry.Value);
  }

  public static string ToLogInfo(this IQueryParameters queryParameters)
  {
    var sb = new StringBuilder();

    sb.AppendLine($"Sql: {queryParameters.Sql}");
    sb.AppendLine("Parameters:");

    foreach (var entry in queryParameters.Properties)
      sb.AppendLine($"{entry.Key} = {entry.Value}");

    return sb.ToString();
  }
}
