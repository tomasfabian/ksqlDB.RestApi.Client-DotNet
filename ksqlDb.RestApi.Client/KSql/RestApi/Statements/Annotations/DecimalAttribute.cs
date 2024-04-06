namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public sealed class DecimalAttribute(byte precision, byte scale) : Attribute
{
  public byte Precision { get; set; } = precision;
  public byte Scale { get; set; } = scale;
}
