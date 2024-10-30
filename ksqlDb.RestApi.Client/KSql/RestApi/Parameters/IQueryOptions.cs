using System.Text.Json.Nodes;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

public interface IQueryOptions
{
  Dictionary<string, JsonValue> Properties { get; }
}
