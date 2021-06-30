using System.Text.Json.Serialization;

namespace Kafka.DotNet.SqlServer.Cdc
{
  public record Source
  {
    public string Version { get; set; }
    public string Connector { get; set; }
    public string Name { get; set; }
    [JsonPropertyName("ts_ms")]
    public long TsMs { get; set; }
    public string Snapshot { get; set; }
    public string Db { get; set; }
    public string Schema { get; set; }
    public string Table { get; set; }
    [JsonPropertyName("change_lsn")]
    public string ChangeLsn { get; set; }
    [JsonPropertyName("commit_lsn")]
    public string CommitLsn { get; set; }
    [JsonPropertyName("event_serial_no")]
    public int? EventSerialNo { get; set; }
  }
}