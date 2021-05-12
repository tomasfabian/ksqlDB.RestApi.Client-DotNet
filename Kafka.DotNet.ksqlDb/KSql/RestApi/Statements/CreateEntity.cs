using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Kafka.DotNet.ksqlDB.Infrastructure.Extensions;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Enums;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Statements
{
  internal sealed class CreateEntity : CreateEntityStatement
  {
    internal string KSqlTypeTranslator(Type type)
    {
      var ksqlType = string.Empty;

      if (type == typeof(string))
        ksqlType = "VARCHAR";
      if (type.IsOneOfFollowing(typeof(int), typeof(int?), typeof(short), typeof(short?)))
        ksqlType = "INT";
      if (type.IsOneOfFollowing(typeof(long), typeof(long?)))
        ksqlType = "BIGINT";
      if (type.IsOneOfFollowing(typeof(double), typeof(double?)))
        ksqlType = "DOUBLE";
      if (type.IsOneOfFollowing(typeof(bool), typeof(bool?)))
        ksqlType = "BOOLEAN";

      if (type == typeof(decimal))
        ksqlType = "DECIMAL";

        if (type.IsArray)
      {
        var elementType = KSqlTypeTranslator(type.GetElementType());

        ksqlType = $"ARRAY<{elementType}>";
      }

      if (type.IsDictionary())
      {
        Type[] typeParameters = type.GetGenericArguments();

        var keyType = KSqlTypeTranslator(typeParameters[0]);
        var valueType = KSqlTypeTranslator(typeParameters[1]);

        ksqlType = $"MAP<{keyType}, {valueType}>";
      }

      return ksqlType;
    }

    private readonly StringBuilder stringBuilder = new();

    internal string Print<T>(StatementContext statementContext, EntityCreationMetadata metadata, bool? ifNotExists)
    {
      stringBuilder.Clear();

      PrintCreateOrReplace<T>(statementContext, metadata);

      if (ifNotExists.HasValue && ifNotExists.Value)
        stringBuilder.Append(" IF NOT EXISTS");

      stringBuilder.Append($"{statementContext.Statement} {statementContext.EntityName}");

      stringBuilder.Append(" (" + Environment.NewLine);

      PrintProperties<T>(statementContext);

      stringBuilder.Append(")");

      string with = CreateStatements.GenerateWithClause(metadata);

      stringBuilder.Append($"{with};");

      return stringBuilder.ToString();
    }

    private string ExploreAttributes(MemberInfo memberInfo, Type type)
    {
      if (type == typeof(decimal))
      {
        var decimalMember = memberInfo.GetCustomAttributes().OfType<DecimalAttribute>().FirstOrDefault();

        if(decimalMember != null)
          return $"({decimalMember.Precision},{decimalMember.Scale})";
      }

      return string.Empty;
    }

    private void PrintProperties<T>(StatementContext statementContext)
    {
      var ksqlProperties = new List<string>();

      foreach (var memberInfo in Members<T>())
      {
        var type = GetMemberType<T>(memberInfo);

        var ksqlType = KSqlTypeTranslator(type);

        string columnDefinition = $"\t{memberInfo.Name} {ksqlType}{ExploreAttributes(memberInfo, type)}";

        columnDefinition += TryAttachKey(statementContext.KSqlEntityType, memberInfo);

        ksqlProperties.Add(columnDefinition);
      }

      stringBuilder.AppendLine(string.Join($",{Environment.NewLine}", ksqlProperties));
    }

    private void PrintCreateOrReplace<T>(StatementContext statementContext, EntityCreationMetadata metadata)
    {
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

      statementContext.EntityName = GetEntityName<T>(metadata);

      stringBuilder.Append($"{creationTypeText} {entityTypeText}");
    }

    private string TryAttachKey(KSqlEntityType entityType, MemberInfo memberInfo)
    {
      if (!memberInfo.HasKey())
        return string.Empty;

      string key = entityType switch
      {
        KSqlEntityType.Stream => "KEY",
        KSqlEntityType.Table => "PRIMARY KEY",
        _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null)
      };

      return $" {key}";
    }
  }
}