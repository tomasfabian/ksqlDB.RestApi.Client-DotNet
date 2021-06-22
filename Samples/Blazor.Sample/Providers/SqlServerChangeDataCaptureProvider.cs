using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Blazor.Sample.Providers
{
  public class SqlServerChangeDataCaptureProvider : ISqlServerChangeDataCaptureProvider
  {
    private readonly string connectionString;

    public SqlServerChangeDataCaptureProvider(string connectionString)
    {      
      if(connectionString == null)
        throw new ArgumentNullException(nameof(connectionString));

      if (connectionString.Trim() == String.Empty)
        throw new ArgumentException($"{nameof(connectionString)} cannot be empty", nameof(connectionString));

      this.connectionString = connectionString;
    }

    public async Task EnableAsync(string tableName, string schemaName = "dbo")
    {
      var script = @"sys.sp_cdc_enable_db";

      await ExecuteNonQueryAsync(script, connectionString, CommandType.StoredProcedure);

      script = @"sys.sp_cdc_enable_table";

      SqlParameter[] parameters = {
        new("@source_schema", SqlDbType.VarChar),
        new("@source_name", SqlDbType.VarChar),
        new("@role_name", SqlDbType.VarChar),
        new("@supports_net_changes", SqlDbType.Int)
        };

      parameters[0].Value = schemaName;
      parameters[1].Value = tableName; //"Sensors";
      parameters[2].Value = DBNull.Value;
      parameters[3].Value = 0;

      await ExecuteNonQueryAsync(script, connectionString, CommandType.StoredProcedure, parameters);
    }

    public async Task RollbackAsync(string tableName, string schemaName = "dbo")
    {
      var script = @"sys.sp_cdc_disable_table";

      SqlParameter[] parameters = {
        new("@source_schema", SqlDbType.VarChar),
        new("@source_name", SqlDbType.VarChar),
        new("@capture_instance", SqlDbType.VarChar),
      };

      parameters[0].Value = schemaName;
      parameters[1].Value = tableName;
      parameters[2].Value = "all";

      await ExecuteNonQueryAsync(script, connectionString, CommandType.StoredProcedure, parameters);
      
      script = @"sys.sp_cdc_disable_db";

      await ExecuteNonQueryAsync(script, connectionString, CommandType.StoredProcedure);
    }

    private static async Task<int?> ExecuteNonQueryAsync(string script, string connectionString, CommandType commandType, params SqlParameter[] parameters)
    {
      try
      {
        await using SqlConnection connection = new SqlConnection(connectionString);
        await using SqlCommand command = new SqlCommand(script, connection)
        {
          CommandType = commandType,
        };

        command.Parameters.AddRange(parameters);

        await command.Connection.OpenAsync();

        var result = await command.ExecuteNonQueryAsync();

        return result;
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
      }

      return null;
    }
  }
}