using System.Collections.Generic;

namespace Kafka.DotNet.SqlServer.Cdc.Connectors
{
  public record ConnectorMetadata
  {
    public ConnectorType ConnectorType { get; set; } = ConnectorType.Source;

    private const string NamePropertyName = "name";
    private const string ConnectorClassName = "connector.class";
    private const string KeyConverterPropertyName = "key.converter";
    private const string ValueConverterPropertyName = "value.converter";

    public string Name
    {
      get => this[NamePropertyName];
      set => this[NamePropertyName] = value;
    }

    public string ConnectorClass
    {
      get => this[ConnectorClassName];
      set => this[ConnectorClassName] = value;
    }
    
    public string KeyConverter
    {
      get => this[KeyConverterPropertyName];
      set => this[KeyConverterPropertyName] = value;
    }

    public string ValueConverter
    {
      get => this[ValueConverterPropertyName];
      set => this[ValueConverterPropertyName] = value;
    }

    private const string JsonConverter = "org.apache.kafka.connect.json.JsonConverter";

    public ConnectorMetadata SetJsonKeyConverter()
    {
      KeyConverter = JsonConverter;

      return this;
    }

    public ConnectorMetadata SetJsonValueConverter()
    {
      ValueConverter = JsonConverter;

      return this;
    }
    
    internal Dictionary<string, string> Properties { get; } = new();

    public string this[string key]
    {
      get => Properties[key];
      set => Properties[key] = value;
    }

    public ConnectorMetadata SetProperty(string key, string value)
    {
      Properties[key] = value;

      return this;
    }

    private protected bool HasValue(string propertyName)
    {
      return Properties.ContainsKey(propertyName) && !string.IsNullOrEmpty(this[propertyName]);
    }
  }
}