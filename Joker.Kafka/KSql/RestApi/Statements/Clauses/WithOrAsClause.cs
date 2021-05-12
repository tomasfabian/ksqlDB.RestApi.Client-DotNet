using System;
using Kafka.DotNet.ksqlDB.KSql.Linq.Statements;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.Query.Statements;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Clauses
{
  internal sealed class WithOrAsClause : IWithOrAsClause
  {
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly StatementContext statementContext;

    public WithOrAsClause(IServiceScopeFactory serviceScopeFactory, StatementContext statementContext)
    {
      this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
      this.statementContext = statementContext ?? throw new ArgumentNullException(nameof(statementContext));

      
      string creationTypeText = statementContext.CreationType switch
      {
        CreationType.Create => "CREATE",
        CreationType.CreateOrReplace => "CREATE OR REPLACE",
      };

      string entityTypeText = statementContext.KSqlEntityType switch
      {
        KSqlEntityType.Table => KSqlEntityType.Table.ToString().ToUpper(),
        KSqlEntityType.Stream => KSqlEntityType.Stream.ToString().ToUpper(),
      };

      statementContext.Statement = @$"{creationTypeText} {entityTypeText} {statementContext.EntityName}";
    }

    public IAsClause With(CreationMetadata creationMetadata)
    {
      string withClause = CreateStatements.GenerateWithClause(creationMetadata);

      statementContext.Statement = @$"{statementContext.Statement}
{withClause}";

      return this;
    }

    public ICreateStatement<T> As<T>(string entityName = null)
    {
      if (entityName == String.Empty)
        entityName = null;

      statementContext.StreamName = entityName;

      return new CreateStatement<T>(serviceScopeFactory, statementContext);
    }
  }
}