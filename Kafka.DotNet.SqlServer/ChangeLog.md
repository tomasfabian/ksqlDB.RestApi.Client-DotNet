Kafka.DotNet.SqlServer is a client API for consuming row-level table changes (CDC - [Change Data Capture](https://docs.microsoft.com/en-us/sql/relational-databases/track-changes/about-change-data-capture-sql-server?view=sql-server-ver15)) from a Sql Server databases with the Debezium connector streaming platform.

Project [Wiki can be found here]()

### v0.1.0
- SqlServerConnectorMetadata, ConnectorMetadata, ConnectorExtensions - connector configuration for SQL Server
- KsqlDbConnect - class for creating connectors with ksqldb
- CdcClient, ICdcClient, ISqlServerCdcClient - Enables/Disables change data capture for the current database and specified source table.
- DatabaseChangeObject, DatabaseChangeObject<TEntity>, RawDatabaseChangeObject<TEntity>
- ChangeDataCaptureType enum