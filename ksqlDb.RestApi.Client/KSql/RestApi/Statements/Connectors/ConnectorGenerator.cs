using System.Text;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Connectors;

internal static class ConnectorGenerator
{
  public static string ToCreateConnectorStatement(this IDictionary<string, string> config, string connectorName, bool ifNotExists = false, ConnectorType connectorType = ConnectorType.Source)
  {
    var stringBuilder = new StringBuilder();

    string connectorTypeClause = connectorType switch
    {
      ConnectorType.Source => "SOURCE",
      ConnectorType.Sink => "SINK",
      _ => throw new ArgumentOutOfRangeException()
    };

    string existsCondition = ifNotExists ? "IF NOT EXISTS " : string.Empty;

    string createConnector = $"CREATE {connectorTypeClause} CONNECTOR {existsCondition}`{connectorName}` WITH ({Environment.NewLine}";

    stringBuilder.Append(createConnector);

    var keyValuePairs = config.Select(c => $"\t'{c.Key}'= '{c.Value}'");

    var properties = string.Join($", {Environment.NewLine}", keyValuePairs);

    stringBuilder.AppendLine(properties);

    stringBuilder.AppendLine(");");

    return stringBuilder.ToString();
  }
}