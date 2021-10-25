# SqlServer.Connector

SqlServer.Connector is a client API for consuming row-level table changes (CDC - [Change Data Capture](https://docs.microsoft.com/en-us/sql/relational-databases/track-changes/about-change-data-capture-sql-server?view=sql-server-ver15)) from SQL Server databases with the Debezium connector streaming platform.

Project [Wiki can be found here](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/SqlServer.Connector/Wiki.md)

### v0.3.0
# ⚠ Package had to be renamed to SqlServer.Connector

# Kafka.DotNet.SqlServer

### v0.3.0-rc.1
- ConnectRestApiClient, IConnectRestApiClient
- Connector, CreateConnectorResponse, ConnectorTask
- added interface `IDatabaseChangeObject<TEntity>`

### v0.2.0
KsqlDbConnect:
- CreateConnectorIfNotExistsAsync - Create a new connector in the Kafka Connect cluster with the configuration passed in the connectorMetadata parameter. If the IF NOT EXISTS clause is present, the statement does not fail if a connector with the supplied name already exists
CcdClient:
- IsCdcDbEnabledAsync - Has SQL Server database enabled Change Data Capture (CDC). 
- IsCdcTableEnabledAsync - Has table Change Data Capture (CDC) enabled on a SQL Server database.
- CdcDisableTableAsync added optional parameter 'captureInstance'

CdcEnableTable:
Sql parameters for [sys.sp_cdc_enable_table]. The following optional arguments were added:
- IndexName, CaptureInstance, CapturedColumnList, FilegroupName properties
- SchemaName - default value is set to "dbo"

KsqlDbConnect:
- GetConnectorsAsync - List all connectors in the Connect cluster.
- DropConnectorAsync, DropConnectorIfExistsAsync - Drop a connector and delete it from the Connect cluster.

- ConnectorType enum - Sink or Source

### v0.1.0
- SqlServerConnectorMetadata, ConnectorMetadata, ConnectorExtensions - connector configuration for SQL Server
- KsqlDbConnect - class for creating connectors with ksqldb
- CdcClient, ICdcClient, ISqlServerCdcClient - Enables/Disables change data capture for the current database and specified source table.
- DatabaseChangeObject, `DatabaseChangeObject<TEntity>`, `RawDatabaseChangeObject<TEntity>` - POCOs
- ChangeDataCaptureType enum