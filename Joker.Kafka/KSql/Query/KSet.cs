using System;
using System.Linq.Expressions;

namespace Kafka.DotNet.ksqlDB.KSql.Query
{
  internal abstract class KSet
  {
    public abstract Type ElementType { get; }
    public Expression Expression { get; internal set; }
  }
}