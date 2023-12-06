namespace ksqlDB.RestApi.Client.KSql.Query.Visitors
{
  internal static class BinaryOperators
  {
    internal static string Add => "+";
    internal static string Subtract => "-";
    internal static string Divide => "/";
    internal static string Multiply => "*";
    internal static string Modulo => "%";
    internal static string AndAlso => "AND";
    internal static string OrElse => "OR";
    internal static string Equal => "=";
    internal static string NotEqual => "!=";
    internal static string LessThan => "<";
    internal static string LessThanOrEqual => "<=";
    internal static string GreaterThan => ">";
    internal static string GreaterThanOrEqual => ">=";
  }
}
