### Enable CDC in SQL Server
```SQL

-- ================================
-- Enable Database for CDC Template
-- ================================
USE Sensors
GO
EXEC sys.sp_cdc_enable_db
GO

-- ===================================================
-- Enable a Table Specifying Filegroup Option Template
-- ===================================================
USE Sensors
GO

EXEC sys.sp_cdc_enable_table
	@source_schema = N'dbo',
	@source_name   = N'Sensors',
	@role_name     = NULL,
	@filegroup_name = NULL,
	@supports_net_changes = 0
GO

USE Sensors;
GO
EXEC sys.sp_cdc_help_change_data_capture
GO
```

### Create a connector
```KSQL
CREATE SOURCE CONNECTOR MSSQL_SENSORS WITH (
  'connector.class' = 'io.debezium.connector.sqlserver.SqlServerConnector',
  'database.hostname'= 'sqlserver2019', 
  'database.port'= '1433', 
  'database.user'= 'sa', 
  'database.password'= '<YourNewStrong@Passw0rd>', 
  'database.dbname'= 'Sensors', 
  'database.server.name'= 'sqlserver2019', 
  'table.include.list'= 'dbo.Sensors', 
  'table.whitelist' = 'dbo.Sensors', 
  'database.history.kafka.bootstrap.servers'= 'broker01:9092', 
  'database.history.kafka.topic'= 'dbhistory.sensors',
  'key.converter'= 'org.apache.kafka.connect.json.JsonConverter',
  'key.converter.schemas.enable'= 'false',
  'value.converter'= 'org.apache.kafka.connect.json.JsonConverter',
  'value.converter.schemas.enable'= 'false',
  'include.schema.changes'= 'false'
);
```

Connector status:
```KSQL
SHOW CONNECTORS;
```
http://localhost:8083/connectors/MSSQL_SENSORS/status