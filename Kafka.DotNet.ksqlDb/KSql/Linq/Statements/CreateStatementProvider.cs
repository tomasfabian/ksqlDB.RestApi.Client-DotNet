using System;
using System.Linq.Expressions;
using Kafka.DotNet.ksqlDB.KSql.Query;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.Query.Statements;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.DotNet.ksqlDB.KSql.Linq.Statements
{
  internal class CreateStatementProvider : ICreateStatementProvider
  {
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly StatementContext statementContext;

    public CreateStatementProvider(IServiceScopeFactory serviceScopeFactory, StatementContext statementContext = null)
    {
      this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
      this.statementContext = statementContext;
    }
    
    ICreateStatement<TResult> ICreateStatementProvider.CreateStatement<TResult>(Expression expression)
    {
      return new CreateStatement<TResult>(serviceScopeFactory, expression, statementContext);
    }
  }
}