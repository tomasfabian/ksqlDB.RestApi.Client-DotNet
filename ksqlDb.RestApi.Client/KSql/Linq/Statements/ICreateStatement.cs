using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ksqlDB.RestApi.Client.KSql.Linq.Statements;

public interface ICreateStatement
{    
  /// <summary>
  /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of ICreateStatement is executed.
  /// </summary>
  Type ElementType { get; }

  /// <summary>
  /// Gets the expression tree that is associated with the instance of ICreateStatement.
  /// </summary>
  Expression Expression { get; }

  /// <summary>
  /// Gets the create statement provider that is associated with this data source.
  /// </summary>
  ICreateStatementProvider Provider { get; }

  Task<HttpResponseMessage> ExecuteStatementAsync(CancellationToken cancellationToken = default);
}

public interface ICreateStatement<out T> : ICreateStatement
{
}