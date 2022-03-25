using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using DateTime = System.DateTime;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements
{
  internal sealed class CreateEntity : CreateEntityStatement
  {
    internal static string KSqlTypeTranslator(Type type)
    {
      var ksqlType = string.Empty;
      
      if (type == typeof(byte[]))
        ksqlType = "BYTES";
      else if (type.IsArray)
      {
        var elementType = KSqlTypeTranslator(type.GetElementType());

        ksqlType = $"ARRAY<{elementType}>";
      }
      else if (type.IsDictionary())
      {
        Type[] typeParameters = type.GetGenericArguments();

        var keyType = KSqlTypeTranslator(typeParameters[0]);
        var valueType = KSqlTypeTranslator(typeParameters[1]);

        ksqlType = $"MAP<{keyType}, {valueType}>";
      }
      else if (type == typeof(string))
        ksqlType = "VARCHAR";
      else if (type.IsOneOfFollowing(typeof(int), typeof(int?), typeof(short), typeof(short?)))
        ksqlType = "INT";
      else if (type.IsOneOfFollowing(typeof(long), typeof(long?)))
        ksqlType = "BIGINT";
      else if (type.IsOneOfFollowing(typeof(double), typeof(double?)))
        ksqlType = "DOUBLE";
      else if (type.IsOneOfFollowing(typeof(bool), typeof(bool?)))
        ksqlType = "BOOLEAN";
      else if (type == typeof(decimal))
        ksqlType = "DECIMAL";
      else if (type == typeof(DateTime))
        ksqlType = "DATE";
      else if (type == typeof(TimeSpan))
        ksqlType = "TIME";
      else if (type == typeof(DateTimeOffset))
        ksqlType = "TIMESTAMP";
      else if (!type.IsGenericType && type.TryGetAttribute<StructAttribute>() != null)
      {
        var ksqlProperties = GetProperties(type);

        ksqlType = $"STRUCT<{string.Join(", ", ksqlProperties)}>";
      }
      else if (!type.IsGenericType && (type.IsClass || type.IsStruct()))
      {
        ksqlType = type.Name.ToUpper();
      }
      else
      {
        Type elementType = null;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
          elementType = type.GetGenericArguments()[0];
        else
        {
          var enumerableInterfaces = type.GetEnumerableTypeDefinition().ToArray();

          if (enumerableInterfaces.Any())
          {
            elementType = enumerableInterfaces[0].GetGenericArguments()[0];
          }
        }

        if (elementType != null)
        {
          string ksqlElementType = KSqlTypeTranslator(elementType);

          ksqlType = $"ARRAY<{ksqlElementType}>";
        }
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

      PrintProperties<T>(statementContext, metadata);

      stringBuilder.Append(")");

      string with = CreateStatements.GenerateWithClause(metadata);

      stringBuilder.Append($"{with};");

      return stringBuilder.ToString();
    }

    internal static string ExploreAttributes(MemberInfo memberInfo, Type type)
    {
      if (type == typeof(decimal))
      {
        var decimalMember = memberInfo.TryGetAttribute<DecimalAttribute>();

        if (decimalMember != null)
          return $"({decimalMember.Precision},{decimalMember.Scale})";
      }

      if (type.IsArray)
      {
        var headersAttribute = memberInfo.TryGetAttribute<HeadersAttribute>();

        if (headersAttribute != null)
        {
          const string header = " HEADER";

          if (string.IsNullOrEmpty(headersAttribute.Key))
            return $"{header}S";

          return $"{header}('{headersAttribute.Key}')";
        }
      }

      return string.Empty;
    }

    private void PrintProperties<T>(StatementContext statementContext, EntityCreationMetadata metadata)
    {
      var ksqlProperties = new List<string>();

      foreach (var memberInfo in Members<T>(metadata?.IncludeReadOnlyProperties))
      {
        var type = GetMemberType(memberInfo);

        var ksqlType = KSqlTypeTranslator(type);

        string columnDefinition = $"\t{memberInfo.Name} {ksqlType}{ExploreAttributes(memberInfo, type)}";

        columnDefinition += TryAttachKey(statementContext.KSqlEntityType, memberInfo);

        ksqlProperties.Add(columnDefinition);
      }

      stringBuilder.AppendLine(string.Join($",{Environment.NewLine}", ksqlProperties));
    }

    internal static IEnumerable<string> GetProperties(Type type)
    {
      var ksqlProperties = new List<string>();

      foreach (var memberInfo in Members(type, false))
      {
        var memberType = GetMemberType(memberInfo);

        var ksqlType = KSqlTypeTranslator(memberType);

        string columnDefinition = $"{memberInfo.Name} {ksqlType}{ExploreAttributes(memberInfo, memberType)}";

        ksqlProperties.Add(columnDefinition);
      }

      return ksqlProperties;
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

      string source = metadata.IsReadOnly ? " SOURCE" : String.Empty;

      stringBuilder.Append($"{creationTypeText}{source} {entityTypeText}");
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