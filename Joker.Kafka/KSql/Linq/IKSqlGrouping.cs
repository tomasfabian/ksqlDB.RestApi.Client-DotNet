namespace Kafka.DotNet.ksqlDB.KSql.Linq
{
  public interface IKSqlGrouping<out TKey, out TElement> : IAggregations<TElement>
  {
    TKey Key { get; }
  }
}