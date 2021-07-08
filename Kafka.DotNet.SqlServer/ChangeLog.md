Kafka.DotNet.SqlServer is a client API for consuming row-level table changes (CDC - [Change Data Capture](https://docs.microsoft.com/en-us/sql/relational-databases/track-changes/about-change-data-capture-sql-server?view=sql-server-ver15)) from SQL Server databases with the Debezium connector streaming platform.

Project [Wiki can be found here](https://github.com/tomasfabian/Kafka.DotNet.ksqlDB/blob/main/Kafka.DotNet.SqlServer/Wiki.md)

### v0.2.0-rc.1
CcdClient:
- IsCdcDbEnabledAsync
- IsCdcTableEnabledAsync

### v0.1.0
- SqlServerConnectorMetadata, ConnectorMetadata, ConnectorExtensions - connector configuration for SQL Server
- KsqlDbConnect - class for creating connectors with ksqldb
- CdcClient, ICdcClient, ISqlServerCdcClient - Enables/Disables change data capture for the current database and specified source table.
- DatabaseChangeObject, DatabaseChangeObject<TEntity>, RawDatabaseChangeObject<TEntity> - POCOs
- ChangeDataCaptureType enum