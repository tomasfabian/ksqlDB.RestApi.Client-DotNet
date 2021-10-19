using System;

namespace SqlServer.Connector.Cdc
{
  /// <summary>
  /// Sql parameters for sys.sp_cdc_enable_table. 
  /// </summary>
  public record CdcEnableTable
  {
    public CdcEnableTable(string tableName)
    {
      if(tableName == null)
        throw new ArgumentNullException(nameof(tableName));

      if (tableName.Trim() == String.Empty)
        throw new ArgumentException($"{nameof(tableName)} cannot be null or empty", nameof(tableName));

      TableName = tableName;
    }

    public string TableName { get; set; }
    public string SchemaName { get; set; } = "dbo";
    public object RoleName { get; set; } = DBNull.Value;

    /// <summary>
    /// Indicates whether support for querying for net changes is to be enabled for this capture instance. supports_net_changes is bit with a default of 1 if the table has a primary key or the table has a unique index that has been identified by using the @index_name parameter. Otherwise, the parameter defaults to 0.
    /// </summary>
    public int SupportsNetChanges { get; set; }

    /// <summary>
    /// Name of the capture instance in the current database. When 'all' is specified, all capture instances defined for tableName are disabled.
    /// </summary>
    public string CaptureInstance { get; set; }

    public string IndexName { get; set; }

    /// <summary>
    /// Identifies the source table columns that are to be included in the change table.
    /// </summary>
    public string CapturedColumnList { get; set; }
    
    /// <summary>
    /// Is the filegroup to be used for the change table created for the capture instance.
    /// </summary>
    public string FilegroupName { get; set; }
  }
}