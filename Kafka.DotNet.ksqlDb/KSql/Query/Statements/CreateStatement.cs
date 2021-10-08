using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.Linq.Statements;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Statements
{
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
      var options = serviceScope.ServiceProvider.GetRequiredService<KSqlDBContextOptions>();

      if (options.UseBasicAuth)
      {
        restApiClient.SetCredentials(new BasicAuthCredentials(options.BasicAuthUserName, options.BasicAuthPassword));
      }

      serviceScope.Dispose();

      var ksqlQuery = BuildKsql();

      var dBStatement = new KSqlDbStatement(ksqlQuery);

      return restApiClient?.ExecuteStatementAsync(dBStatement, cancellationToken);
    }
  }
}