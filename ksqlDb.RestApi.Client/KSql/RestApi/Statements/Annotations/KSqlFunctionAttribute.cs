namespace ksqlDb.RestApi.Client.KSql.RestApi.Statements.Annotations;

#nullable enable
[AttributeUsage(AttributeTargets.Method)]
public class KSqlFunctionAttribute : Attribute
{
  public string? FunctionName { get; set; }
}
