namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

#nullable enable
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class HeadersAttribute : Attribute
{
  public HeadersAttribute(string? key = null)
  {
    Key = key;
  }

  public string? Key { get; }
}
