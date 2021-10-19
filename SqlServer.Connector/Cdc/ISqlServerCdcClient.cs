using System.Threading.Tasks;

namespace SqlServer.Connector.Cdc
{
  public interface ISqlServerCdcClient : ICdcClient
  {
    Task CdcEnableTableAsync(string tableName, string schemaName = "dbo");
    Task CdcEnableTableAsync(CdcEnableTable cdcEnableTable);

    Task CdcDisableTableAsync(string tableName, string schemaName = "dbo", string captureInstance = "all");

    /// <summary>
    /// Has SQL Server database enabled Change Data Capture (CDC) 
    /// </summary>
    /// <param name="databaseName"></param>
    /// <returns></returns>
    Task<bool> IsCdcDbEnabledAsync(string databaseName);

    /// <summary>
    /// Has table Change Data Capture (CDC) enabled on a SQL Server database
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    Task<bool> IsCdcTableEnabledAsync(string tableName, string schemaName = "dbo", string captureInstance = null);
  }
}