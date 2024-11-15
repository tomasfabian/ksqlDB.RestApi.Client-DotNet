using System.Reflection;
using System.Text;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDb.RestApi.Client.KSql.RestApi.Parsers;
using static System.String;
using ksqlDb.RestApi.Client.Metadata;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements;

internal sealed class CreateEntity(IMetadataProvider metadataProvider) : EntityInfo(metadataProvider)
{
  private readonly StringBuilder stringBuilder = new();
  private readonly IMetadataProvider _metadataProvider = metadataProvider;

  internal string Print<T>(StatementContext statementContext, EntityCreationMetadata metadata, bool? ifNotExists)
  {
    stringBuilder.Clear();

    PrintCreateOrReplace<T>(statementContext, metadata);

    if (ifNotExists.HasValue && ifNotExists.Value)
      stringBuilder.Append(" IF NOT EXISTS");

    stringBuilder.Append($"{statementContext.Statement} {statementContext.EntityName}");

    stringBuilder.Append(" (" + Environment.NewLine);

    PrintProperties<T>(statementContext, metadata);

    stringBuilder.Append(')');

    string with = CreateStatements.GenerateWithClause(metadata);

    stringBuilder.Append($"{with};");

    return stringBuilder.ToString();
  }

  private void PrintProperties<T>(StatementContext statementContext, EntityCreationMetadata metadata)
  {
    var ksqlProperties = new List<string>();
    KSqlTypeTranslator<T> typeTranslator = new(_metadataProvider);

    foreach (var memberInfo in Members<T>(metadata.IncludeReadOnlyProperties))
    {
      var type = GetMemberType(memberInfo);

      var ksqlType = typeTranslator.Translate(type, memberInfo, metadata.IdentifierEscaping);

      var columnName = IdentifierUtil.Format(memberInfo, metadata.IdentifierEscaping, _metadataProvider);
      string columnDefinition = $"\t{columnName} {ksqlType}{typeTranslator.ExploreAttributes(typeof(T), memberInfo, type)}";

      columnDefinition += TryAttachKey<T>(statementContext.KSqlEntityType, memberInfo);

      ksqlProperties.Add(columnDefinition);
    }

    stringBuilder.AppendLine(Join($",{Environment.NewLine}", ksqlProperties));
  }

  private void PrintCreateOrReplace<T>(StatementContext statementContext, EntityCreationMetadata metadata)
  {
    string creationTypeText = statementContext.CreationType switch
    {
      CreationType.Create => "CREATE",
      CreationType.CreateOrReplace => "CREATE OR REPLACE",
      _ => throw new ArgumentOutOfRangeException(nameof(statementContext), $"Unknown '{nameof(CreationType)}' value {statementContext.CreationType}.")
    };

    string entityTypeText = statementContext.KSqlEntityType switch
    {
      KSqlEntityType.Table => KSqlEntityType.Table.ToString().ToUpper(),
      KSqlEntityType.Stream => KSqlEntityType.Stream.ToString().ToUpper(),
      _ => throw new ArgumentOutOfRangeException(nameof(statementContext), $"Unknown '{nameof(KSqlEntityType)}' value {statementContext.KSqlEntityType}.")
    };

    statementContext.EntityName = EntityProvider.GetFormattedName<T>(metadata);

    string source = metadata.IsReadOnly ? " SOURCE" : Empty;

    stringBuilder.Append($"{creationTypeText}{source} {entityTypeText}");
  }

  private string TryAttachKey<T>(KSqlEntityType entityType, MemberInfo memberInfo)
  {
    var entityMetadata = _metadataProvider.GetEntities().FirstOrDefault(c => c.Type == typeof(T));

    var primaryKey = entityMetadata?.PrimaryKeyMemberInfo;

    if ((primaryKey == null || primaryKey.Name != memberInfo.Name) && !memberInfo.HasKey())
      return Empty;

    string key = entityType switch
    {
      KSqlEntityType.Stream => "KEY",
      KSqlEntityType.Table => "PRIMARY KEY",
      _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null)
    };

    return $" {key}";
  }

  protected override bool IncludeMemberInfo(EntityMetadata? entityMetadata, MemberInfo memberInfo)
  {
    var fieldMetadata = entityMetadata?.GetFieldMetadataBy(memberInfo);
    if (fieldMetadata is { IgnoreInDDL: true })
      return false;

    return base.IncludeMemberInfo(entityMetadata, memberInfo) && !memberInfo.GetCustomAttributes().OfType<IgnoreInDDLAttribute>().Any();
  }
}
