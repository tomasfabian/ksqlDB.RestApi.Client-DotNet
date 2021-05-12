using System.Linq.Expressions;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;

namespace Kafka.DotNet.ksqlDB.KSql.Linq
{
  public interface ISource<T> : ISource
  {
  }  
  
  public interface ISource
  {
    Expression Expression { get; }
    QueryContext QueryContext { get; set; }
  }
}