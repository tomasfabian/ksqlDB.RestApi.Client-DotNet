using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Clauses;

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
      _ => throw new ArgumentOutOfRangeException(nameof(statementContext), $"Unknown {nameof(CreationType)} value {statementContext.CreationType}")
    };

    string entityTypeText = statementContext.KSqlEntityType switch
    {
      KSqlEntityType.Table => KSqlEntityType.Table.ToString().ToUpper(),
      KSqlEntityType.Stream => KSqlEntityType.Stream.ToString().ToUpper(),
      _ => throw new ArgumentOutOfRangeException(nameof(statementContext), $"Unknown {nameof(KSqlEntityType)} value {statementContext.KSqlEntityType}")
    };

    statementContext.Statement = $"{creationTypeText} {entityTypeText} {statementContext.EntityName}";
  }

  public IAsClause With(CreationMetadata creationMetadata)
  {
    string withClause = CreateStatements.GenerateWithClause(creationMetadata);

    statementContext.Statement = @$"{statementContext.Statement}
{withClause}";

    return this;
  }

  public ICreateStatement<T> As<T>(string? entityName = null)
  {
    if (entityName == String.Empty)
      entityName = null;

    statementContext.FromItemName = entityName;

    return new CreateStatement<T>(serviceScopeFactory, statementContext);
  }
}
