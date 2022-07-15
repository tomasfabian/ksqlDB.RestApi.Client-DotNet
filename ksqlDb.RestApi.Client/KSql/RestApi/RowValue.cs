namespace ksqlDB.RestApi.Client.KSql.RestApi;

internal class RowValue<T>
{ 
  public RowValue(T value)
  {
    Value = value;
  }

  internal T Value { get; set; }
}