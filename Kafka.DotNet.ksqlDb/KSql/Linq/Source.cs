using System.Linq.Expressions;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;

namespace Kafka.DotNet.ksqlDB.KSql.Linq
{
  public class Source<T> : ISource<T>
  {
    public QueryContext QueryContext { get; set; }

    public Source(QueryContext queryContext)
    {
      Expression = Expression.Constant(this);

      QueryContext = queryContext;
    }
    
    public Expression Expression { get; }
  }
  public static class Source
  {
    public static ISource<T> Of<T>(string streamName = null)
    {      
      var queryStreamContext = new QueryContext
      {
        StreamName = streamName
      };

      return new Source<T>(queryStreamContext);
    }
  }
}