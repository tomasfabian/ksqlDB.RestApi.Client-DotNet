using Microsoft.Data.SqlClient;

namespace SqlServer.Connector.Cdc.Connectors
{
  public record SqlServerConnectorMetadata : ConnectorMetadata
  {
    public SqlServerConnectorMetadata(SqlConnectionStringBuilder sqlConnectionStringBuilder)
      : this()
    {
      if (sqlConnectionStringBuilder == null) throw new ArgumentNullException(nameof(sqlConnectionStringBuilder));

      FillFrom(sqlConnectionStringBuilder);
    }

    public SqlServerConnectorMetadata(string connectionString)
      : this()
    {
      if (connectionString == null)
        throw new ArgumentNullException(nameof(connectionString));

      if (connectionString.Trim() == String.Empty)
        throw new ArgumentException($"{nameof(connectionString)} cannot be empty", nameof(connectionString));

      SqlConnectionStringBuilder sqlConnectionStringBuilder = new(connectionString);

      FillFrom(sqlConnectionStringBuilder);
    }

    public SqlServerConnectorMetadata()
    {
      ConnectorClass = "io.debezium.connector.sqlserver.SqlServerConnector";

      DatabasePort = "1433";
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

    private const string DatabaseHostnameName = "database.hostname";
    private const string DatabaseHPortName = "database.port";
    private const string DatabaseUserPropertyName = "database.user";
    private const string DatabasePasswordName = "database.password";
    private const string DatabaseDbnamePropertyName = "database.dbname";
    private const string DatabaseServerNamePropertyName = "database.server.name";
    private const string KafkaBootstrapServersPropertyName = "database.history.kafka.bootstrap.servers";
    private const string DatabaseHistoryKafkaTopicPropertyName = "database.history.kafka.topic";
    private const string TableIncludeListPropertyName = "table.include.list";

    
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

    public SqlServerConnectorMetadata SetTableIncludeListPropertyName(string value)
    {
      Properties[TableIncludeListPropertyName] = value;

      return this;
    }

    public bool TrySetConnectorName()
    {
      if (!HasValue(DatabaseDbnamePropertyName)) 
        return false;
      
      Name = $"{DatabaseDbname}-connector";

      return true;
    }

    public bool TrySetDatabaseHistoryKafkaTopic()
    {
      if (!HasValue(DatabaseServerNamePropertyName)) 
        return false;

      DatabaseHistoryKafkaTopic = $"dbhistory.{DatabaseServerName}";

      return true;
    }
  }
}
