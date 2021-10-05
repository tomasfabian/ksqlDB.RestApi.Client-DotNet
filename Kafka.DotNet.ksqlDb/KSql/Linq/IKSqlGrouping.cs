namespace Kafka.DotNet.ksqlDB.KSql.Linq
{
  public interface IKSqlGrouping<out TKey, out TElement> : IAggregations<TElement>
  {
    /// <summary>
    /// The type of the key of the IKSqlGrouping.
    /// </summary>
    TKey Key { get; }
    TElement Source { get; }
  }
}