namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;

public interface IValueFormatters
{
  public Func<decimal, string> FormatDecimalValue { get; set; }

  public Func<double, string> FormatDoubleValue { get; set; }
}