using System.Linq.Expressions;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Statements;
using Microsoft.Extensions.DependencyInjection;

namespace ksqlDB.RestApi.Client.KSql.Linq.Statements;

internal class CreateStatementProvider : ICreateStatementProvider
{
  private readonly IServiceScopeFactory serviceScopeFactory;
  private readonly StatementContext statementContext;

  public CreateStatementProvider(IServiceScopeFactory serviceScopeFactory, StatementContext statementContext)
  {
    this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    this.statementContext = statementContext;
  }
    
  ICreateStatement<TResult> ICreateStatementProvider.CreateStatement<TResult>(Expression expression)
  {
    return new CreateStatement<TResult>(serviceScopeFactory, expression, statementContext);
  }
}
