using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using static System.String;

namespace Kafka.DotNet.SqlServer.Cdc
{
  public class CdcClient : ICdcClient, ISqlServerCdcClient
  {
    private readonly string connectionString;

    public CdcClient(string connectionString)
    {      
      if(connectionString == null)
        throw new ArgumentNullException(nameof(connectionString));

      if (connectionString.Trim() == Empty)
        throw new ArgumentException($"{nameof(connectionString)} cannot be empty", nameof(connectionString));

      this.connectionString = connectionString;
    }

    public async Task EnableAsync(string tableName, string schemaName = "dbo")
    {
      await CdcEnableDbAsync().ConfigureAwait(false);

      await CdcEnableTable(tableName, schemaName).ConfigureAwait(false);
    }

    public Task CdcEnableTable(string tableName, string schemaName = "dbo")
    {
      string script = @"sys.sp_cdc_enable_table";

      SqlParameter[] parameters =
      {
        new("@source_schema", SqlDbType.VarChar),
        new("@source_name", SqlDbType.VarChar),
        new("@role_name", SqlDbType.VarChar),
        new("@supports_net_changes", SqlDbType.Int)
      };

      parameters[0].Value = schemaName;
      parameters[1].Value = tableName;
      parameters[2].Value = DBNull.Value;
      parameters[3].Value = 0;

      return ExecuteNonQueryAsync(script, connectionString, CommandType.StoredProcedure, parameters);
    }

    public Task CdcEnableDbAsync()
    {
      var script = @"sys.sp_cdc_enable_db";

      return ExecuteNonQueryAsync(script, connectionString, CommandType.StoredProcedure);
    }

    public async Task DisableAsync(string tableName, string schemaName = "dbo")
    {
      await CdcDisableTableAsync(tableName, schemaName).ConfigureAwait(false);

      await CdcDisableDbAsync().ConfigureAwait(false);
    }

    public Task CdcDisableDbAsync()
    {
      string script = @"sys.sp_cdc_disable_db";

      return ExecuteNonQueryAsync(script, connectionString, CommandType.StoredProcedure);
    }

    public async Task CdcDisableTableAsync(string tableName, string schemaName = "dbo")
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
      parameters[2].Value = "all";

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
  }
}