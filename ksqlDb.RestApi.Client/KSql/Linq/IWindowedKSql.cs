namespace ksqlDB.RestApi.Client.KSql.Linq
{
  public interface IWindowedKSql<out TKey, out TElement> : IKSqlGrouping<TKey, TElement> 
  {
    long WindowStart { get; }
    long WindowEnd { get; }
  }
}