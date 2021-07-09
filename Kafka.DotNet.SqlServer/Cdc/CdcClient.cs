using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using static System.String;

namespace Kafka.DotNet.SqlServer.Cdc
{
  public class CdcClient : ISqlServerCdcClient
  {
    private readonly string connectionString;

    public CdcClient(SqlConnectionStringBuilder sqlConnectionStringBuilder)
    {
      if (sqlConnectionStringBuilder == null) throw new ArgumentNullException(nameof(sqlConnectionStringBuilder));

      connectionString = sqlConnectionStringBuilder.ConnectionString;
    }

    public CdcClient(string connectionString)
    {      
      if(connectionString == null)
        throw new ArgumentNullException(nameof(connectionString));

      if (connectionString.Trim() == Empty)
        throw new ArgumentException($"{nameof(connectionString)} cannot be null or empty", nameof(connectionString));

      this.connectionString = connectionString;
    }

    public Task CdcEnableTableAsync(string tableName, string schemaName = "dbo")
    {
      CdcEnableTable cdcEnableTable = new(tableName)
      {
        SchemaName = schemaName
      };

      return CdcEnableTableAsync(cdcEnableTable);
    }

    public Task CdcEnableTableAsync(CdcEnableTable cdcEnableTable)
    {
      string script = @"sys.sp_cdc_enable_table";
      
      SqlParameter[] parameters =
      {
        new("@source_schema", SqlDbType.VarChar),
        new("@source_name", SqlDbType.VarChar),
        new("@role_name", SqlDbType.VarChar),
        new("@supports_net_changes", SqlDbType.Int)
      };

      parameters[0].Value = cdcEnableTable.SchemaName;
      parameters[1].Value = cdcEnableTable.TableName;
      parameters[2].Value = cdcEnableTable.RoleName;
      parameters[3].Value = cdcEnableTable.SupportsNetChanges;

      //TODO: https://docs.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sys-sp-cdc-enable-table-transact-sql?view=sql-server-ver15#syntax
      
      return ExecuteNonQueryAsync(script, connectionString, CommandType.StoredProcedure, parameters);
    }

    public Task CdcEnableDbAsync()
    {
      var script = @"sys.sp_cdc_enable_db";

      return ExecuteNonQueryAsync(script, connectionString, CommandType.StoredProcedure);
    }

    public Task CdcDisableDbAsync()
    {
      string script = @"sys.sp_cdc_disable_db";

      return ExecuteNonQueryAsync(script, connectionString, CommandType.StoredProcedure);
    }

    public async Task CdcDisableTableAsync(string tableName, string schemaName = "dbo", string captureInstance = "all")
    {
      var script = @"sys.sp_cdc_disable_table";
      
      var parameters = new SqlParameter[]
      {
        new("@source_schema", SqlDbType.VarChar),
        new("@source_name", SqlDbType.VarChar),
        new("@capture_instance", SqlDbType.VarChar),
      };

      parameters[0].Value = schemaName;
      parameters[1].Value = tableName;
      parameters[2].Value = captureInstance;

      await ExecuteNonQueryAsync(script, connectionString, CommandType.StoredProcedure, parameters).ConfigureAwait(false);
    }

    private static async Task<int?> ExecuteNonQueryAsync(string script, string connectionString, CommandType commandType, params SqlParameter[] parameters)
    {
#if NETSTANDARD2_0
      using SqlConnection connection = new SqlConnection(connectionString);
      using SqlCommand command = new SqlCommand(script, connection)
      {
        CommandType = commandType
      };
#else
      await using SqlConnection connection = new SqlConnection(connectionString);
      await using SqlCommand command = new SqlCommand(script, connection)
      {
        CommandType = commandType
      };
#endif 

      command.Parameters.AddRange(parameters);

      await command.Connection.OpenAsync().ConfigureAwait(false);

      var result = await command.ExecuteNonQueryAsync().ConfigureAwait(false);

      return result;
    }
    
    /// <summary>
    /// Has SQL Server database enabled Change Data Capture (CDC) 
    /// </summary>
    /// <param name="databaseName"></param>
    /// <returns></returns>
    public Task<bool> IsCdcDbEnabledAsync(string databaseName)
    {
      return ExecuteScalarAsync($"SELECT COUNT(*) FROM sys.databases\r\nWHERE is_cdc_enabled = 1 AND name = '{databaseName}'");
    }    

    /// <summary>
    /// Has table Change Data Capture (CDC) enabled on a SQL Server database
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public Task<bool> IsCdcTableEnabledAsync(string tableName, string schemaName = "dbo")
    {
      string sql = $@"SELECT COUNT(*)
FROM sys.tables tb
INNER JOIN sys.schemas s on s.schema_id = tb.schema_id
WHERE tb.is_tracked_by_cdc = 1 AND tb.name = '{tableName}'
AND s.name = '{schemaName}'";

      return ExecuteScalarAsync(sql);
    } 
    
    private async Task<bool> ExecuteScalarAsync(string cmdText)
    {      
      using var sqlConnection = new SqlConnection(connectionString);
      
      await sqlConnection.OpenAsync().ConfigureAwait(false);

      var sqlCommand = new SqlCommand(cmdText, sqlConnection);

      var response = await sqlCommand.ExecuteScalarAsync().ConfigureAwait(false);

      bool result = (int)response > 0;
#if NETSTANDARD2_0
      sqlConnection.Close();
#else
      await sqlConnection.CloseAsync().ConfigureAwait(false);
#endif

      return result;
    }
  }
}