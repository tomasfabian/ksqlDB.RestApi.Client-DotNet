using System;
using System.Linq.Expressions;

namespace ksqlDB.RestApi.Client.KSql.Query
{
  internal abstract class KSet
  {
    public abstract Type ElementType { get; }
    public Expression Expression { get; internal set; }
  }
}