using System;

namespace Kafka.DotNet.SqlServer.Cdc
{
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
    public string SchemaName { get; set; }
    public object RoleName { get; set; } = DBNull.Value;
    public int SupportsNetChanges { get; set; }
  }
}