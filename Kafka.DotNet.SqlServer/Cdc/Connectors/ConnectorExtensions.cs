using System;
using System.Linq;
using System.Text;

namespace Kafka.DotNet.SqlServer.Cdc.Connectors
{
  public static class ConnectorExtensions
  {
    public static string ToStatement(this ConnectorMetadata connectorMetadata, string connectorName)
    {
      var stringBuilder = new StringBuilder();

      string createConnector = $"CREATE SOURCE CONNECTOR {connectorName} WITH ({Environment.NewLine}";
		
      stringBuilder.Append(createConnector);
		
      var keyValuePairs = connectorMetadata.Properties.Select(c => $"\t'{c.Key}'= '{c.Value}'");

      var properties = string.Join($", {Environment.NewLine}", keyValuePairs);

      stringBuilder.AppendLine(properties);

      stringBuilder.AppendLine(");");
		
      return stringBuilder.ToString();
    }
  }
}