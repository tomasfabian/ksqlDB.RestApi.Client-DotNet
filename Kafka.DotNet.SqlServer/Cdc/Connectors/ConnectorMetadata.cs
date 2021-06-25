using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace Kafka.DotNet.SqlServer.Cdc.Connectors
{
  public record ConnectorMetadata
  {
    public ConnectorMetadata(SqlConnectionStringBuilder sqlConnectionStringBuilder)
      : this()
    {
      //TODO: check input
      FillFrom(sqlConnectionStringBuilder);
    }

    public ConnectorMetadata(string connectionString)
      : this()
    {
      //TODO: check input
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
    }

    public ConnectorMetadata()
    {
      ConnectorClass = "io.debezium.connector.sqlserver.SqlServerConnector";

      DatabasePort = "1433";
    }

    private const string ConnectorClassName = "connector.class";
    private const string DatabaseHostnameName = "database.hostname";
    private const string DatabaseHPortName = "database.port";
    private const string DatabaseUserName = "database.user";

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
      get => this[DatabaseUserName];
      set => this[DatabaseUserName] = value;
    }

    public string DatabasePort
    {
      get => this[DatabaseHPortName];
      set => this[DatabaseHPortName] = value;
    }

    internal Dictionary<string, string> Properties { get; } = new();

    public string this[string key]
    {
      get => Properties[key];
      set => Properties[key] = value;
    }
  }

}