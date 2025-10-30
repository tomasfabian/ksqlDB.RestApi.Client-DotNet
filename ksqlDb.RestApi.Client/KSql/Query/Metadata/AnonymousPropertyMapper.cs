using System.Linq.Expressions;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;

namespace ksqlDb.RestApi.Client.KSql.Query.Metadata;

internal class AnonymousPropertyMapper(KSqlQueryMetadata queryMetadata)
{
  internal KSqlQueryMetadata QueryMetadata => queryMetadata;

  internal void AddLambda(LambdaExpression selector)
  {
    if (selector.Body is not NewExpression newExpression)
    {
      return;
    }

    foreach (var argument in newExpression.Arguments)
    {
      if (argument is not MemberExpression {Expression: not null} memberExpression ||
          memberExpression.Expression.Type.IsAnonymousType())
      {
        continue;
      }

      if (memberExpression.Expression is not ParameterExpression parameterExpression)
      {
        continue;
      }

      var propertyName = memberExpression.Member.Name;
      var declaringType = memberExpression.Member.DeclaringType;

      var anonymousTypeMapping = new AnonymousTypeMapping
      {
        DeclaringType = declaringType,
        PropertyName = propertyName,
        ParameterName = parameterExpression.Name
      };
      if (queryMetadata.NewAnonymousTypeMappings.TryGetValue(propertyName, out var mappings))
      {
        mappings.Add(anonymousTypeMapping);
      }
      else
      {
        queryMetadata.NewAnonymousTypeMappings.Add(propertyName, [anonymousTypeMapping]);
      }
    }
  }
}
