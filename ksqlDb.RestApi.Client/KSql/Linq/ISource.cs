using System.Linq.Expressions;
using ksqlDB.RestApi.Client.KSql.Query.Context;

namespace ksqlDB.RestApi.Client.KSql.Linq;

public interface ISource<T> : ISource
{
}  
  
public interface ISource
{
  Expression Expression { get; }
  QueryContext QueryContext { get; set; }
}