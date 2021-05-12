namespace Kafka.DotNet.ksqlDB.KSql.Query.Functions
{
  public static class KSql
  {
    public static KSqlFunctions Functions => F;

    public static KSqlFunctions F => KSqlFunctions.Instance;
  }
}