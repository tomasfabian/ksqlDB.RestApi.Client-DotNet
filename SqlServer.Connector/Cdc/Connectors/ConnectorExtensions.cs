using System;
using System.Linq;
using System.Text;

namespace SqlServer.Connector.Cdc.Connectors
{
  public static class ConnectorExtensions
  {
    public static string ToCreateConnectorStatement(this ConnectorMetadata connectorMetadata, string connectorName, bool ifNotExists = false)
    {
      var stringBuilder = new StringBuilder();

      string connectorType = connectorMetadata.ConnectorType switch
      {
        ConnectorType.Source => "SOURCE",
        ConnectorType.Sink => "SINK",
        _ => throw new ArgumentOutOfRangeException()
      };

      string existsCondition = ifNotExists ? "IF NOT EXISTS " : string.Empty;

      string createConnector = $"CREATE {connectorType} CONNECTOR {existsCondition}{connectorName} WITH ({Environment.NewLine}";

      stringBuilder.Append(createConnector);

      var keyValuePairs = connectorMetadata.Properties.Select(c => $"\t'{c.Key}'= '{c.Value}'");

      var properties = string.Join($", {Environment.NewLine}", keyValuePairs);

      stringBuilder.AppendLine(properties);

      stringBuilder.AppendLine(");");

      return stringBuilder.ToString();
    }

    public static string ToStatement(this ConnectorMetadata connectorMetadata, string connectorName, bool ifNotExists = false)
    {
      return connectorMetadata.ToCreateConnectorStatement(connectorName, ifNotExists);
    }
  }
}