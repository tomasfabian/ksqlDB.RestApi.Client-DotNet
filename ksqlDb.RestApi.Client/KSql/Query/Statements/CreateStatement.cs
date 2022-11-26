using System.Linq.Expressions;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using Microsoft.Extensions.DependencyInjection;

namespace ksqlDB.RestApi.Client.KSql.Query.Statements;

internal class CreateStatement<TEntity> : KSet, ICreateStatement<TEntity>
{
  private readonly IServiceScopeFactory serviceScopeFactory;
  private IServiceScope serviceScope;
    
  internal StatementContext StatementContext { get; set; }

  internal CreateStatement(IServiceScopeFactory serviceScopeFactory, StatementContext statementContext = null)
  {
    this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
      
    StatementContext = statementContext;

    Provider = new CreateStatementProvider(serviceScopeFactory, statementContext);
      
    Expression = Expression.Constant(this);
  }

  internal CreateStatement(IServiceScopeFactory serviceScopeFactory, Expression expression, StatementContext statementContext = null)
  {
    this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
      
    StatementContext = statementContext;

    Provider = new CreateStatementProvider(serviceScopeFactory, statementContext);

    Expression = expression ?? throw new ArgumentNullException(nameof(expression));
  }

  public override Type ElementType => typeof(TEntity);

  public ICreateStatementProvider Provider { get; internal set; }
    
  internal string BuildKsql()
  {
    serviceScope = serviceScopeFactory.CreateScope();
      
    var dependencies = serviceScope.ServiceProvider.GetService<IKStreamSetDependencies>();

    var ksqlQuery = dependencies.KSqlQueryGenerator?.BuildKSql(Expression, StatementContext);
      
    ksqlQuery = @$"{StatementContext.Statement}
AS {ksqlQuery}";

    serviceScope.Dispose();

    return ksqlQuery;
  }

  public Task<HttpResponseMessage> ExecuteStatementAsync(CancellationToken cancellationToken = default)
  {
    serviceScope = serviceScopeFactory.CreateScope();
      
    cancellationToken.Register(() => serviceScope?.Dispose());
      
    var restApiClient = serviceScope.ServiceProvider.GetRequiredService<IKSqlDbRestApiClient>();

    serviceScope.Dispose();

    var ksqlQuery = BuildKsql();

    var dBStatement = new KSqlDbStatement(ksqlQuery);

    return restApiClient?.ExecuteStatementAsync(dBStatement, cancellationToken);
  }
}