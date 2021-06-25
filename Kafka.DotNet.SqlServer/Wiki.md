Kafka.DotNet.SqlServer is a client API for consuming row-level table changes (CDC - Change Data Capture) from a Sql Server databases with the Debezium connector streaming platform.

### Blazor Sample 
Set docker-compose.csproj as startup project in Kafka.DotNet.InsideOut.sln.

### Nuget
```
Install-Package Kafka.DotNet.SqlServer

```

# CdcClient (v0.1.0)

```C#

```

# Cleanup
```KSQL
show connectors;

drop connector MSSQL_SENSORS_CONNECTOR;

drop stream sqlserversensors delete topic;
```

# Related sources
[Debezium](https://github.com/debezium/debezium)
[Debezium connector for Sql server](https://debezium.io/documentation/reference/connectors/sqlserver.html)
[ksqlDB](https://ksqldb.io/)

# Acknowledgements:
- [Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient/)