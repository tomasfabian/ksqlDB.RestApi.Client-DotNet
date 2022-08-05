namespace ksqlDb.RestApi.Client.ProtoBuf.KSql.RestApi.Responses.Query;

internal class Header : ksqlDB.RestApi.Client.KSql.RestApi.Responses.Header
{
  public string ProtoSchema { get; set; } = null!;
}