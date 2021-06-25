using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Kafka.DotNet.SqlServer.Cdc.Connectors
{
  public record ConnectorMetadata
  {
    public ConnectorMetadata(SqlConnectionStringBuilder sqlConnectionStringBuilder)
      : this()
    {
      if (sqlConnectionStringBuilder == null) throw new ArgumentNullException(nameof(sqlConnectionStringBuilder));

      FillFrom(sqlConnectionStringBuilder);
    }

    public ConnectorMetadata(string connectionString)
      : this()
    {
      if (connectionString == null)
        throw new ArgumentNullException(nameof(connectionString));

      if (connectionString.Trim() == String.Empty)
        throw new ArgumentException($"{nameof(connectionString)} cannot be empty", nameof(connectionString));

      SqlConnectionStringBuilder sqlConnectionStringBuilder = new(connectionString);

      FillFrom(sqlConnectionStringBuilder);
    }

    private void FillFrom(SqlConnectionStringBuilder sqlConnectionStringBuilder)
    {
      var dataSource = sqlConnectionStringBuilder.DataSource.Split(',');

      DatabaseHostname = dataSource[0];

      if (dataSource.Length > 1)
        DatabasePort = dataSource[1];

      DatabaseUser = sqlConnectionStringBuilder.UserID;

      DatabasePassword = sqlConnectionStringBuilder.Password;

      DatabaseDbname = sqlConnectionStringBuilder.InitialCatalog;
    }

    public ConnectorMetadata()
    {
      ConnectorClass = "io.debezium.connector.sqlserver.SqlServerConnector";

      DatabasePort = "1433";
    }

    private const string NamePropertyName = "connector.class";
    private const string ConnectorClassName = "connector.class";
    private const string DatabaseHostnameName = "database.hostname";
    private const string DatabaseHPortName = "database.port";
    private const string DatabaseUserPropertyName = "database.user";
    private const string DatabasePasswordName = "database.password";
    private const string DatabaseDbnamePropertyName = "database.dbname";
    private const string DatabaseServerNamePropertyName = "database.server.name";
    private const string KafkaBootstrapServersPropertyName = "database.history.kafka.bootstrap.servers";
    private const string DatabaseHistoryKafkaTopicPropertyName = "database.history.kafka.topic";
    private const string TableIncludeListPropertyName = "table.include.list";
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

    public string DatabaseHostname
    {
      get => this[DatabaseHostnameName];
      set => this[DatabaseHostnameName] = value;
    }

    public string DatabaseUser
    {
      get => this[DatabaseUserPropertyName];
      set => this[DatabaseUserPropertyName] = value;
    }

    public string DatabasePort
    {
      get => this[DatabaseHPortName];
      set => this[DatabaseHPortName] = value;
    }

    public string DatabasePassword
    {
      get => this[DatabasePasswordName];
      set => this[DatabasePasswordName] = value;
    }

    public string DatabaseDbname
    {
      get => this[DatabaseDbnamePropertyName];
      set => this[DatabaseDbnamePropertyName] = value;
    }

    public string DatabaseServerName
    {
      get => this[DatabaseServerNamePropertyName];
      set => this[DatabaseServerNamePropertyName] = value;
    }

    public string KafkaBootstrapServers
    {
      get => this[KafkaBootstrapServersPropertyName];
      set => this[KafkaBootstrapServersPropertyName] = value;
    }

    public string DatabaseHistoryKafkaTopic
    {
      get => this[DatabaseHistoryKafkaTopicPropertyName];
      set => this[DatabaseHistoryKafkaTopicPropertyName] = value;
    }

    public string TableIncludeList
    {
      get => this[TableIncludeListPropertyName];
      set => this[TableIncludeListPropertyName] = value;
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

    public ConnectorMetadata SetTableIncludeListPropertyName(string value)
    {
      Properties[TableIncludeListPropertyName] = value;

      return this;
    }

    public bool TrySetConnectorName()
    {
      if (HasValue(DatabaseDbnamePropertyName))
      {
        Name = $"{DatabaseDbname}-connector";

        return true;
      }

      return false;
    }

    public bool TrySetDatabaseHistoryKafkaTopic()
    {
      if (HasValue(DatabaseServerNamePropertyName))
      {
        DatabaseHistoryKafkaTopic = $"dbhistory.{DatabaseServerName}";

        return true;
      }

      return false;
    }

    private bool HasValue(string propertyName)
    {
      return Properties.ContainsKey(DatabaseDbname) && !string.IsNullOrEmpty(DatabaseDbname);
    }
  }
}