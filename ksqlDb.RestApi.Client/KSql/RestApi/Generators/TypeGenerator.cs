using System.Text;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDb.RestApi.Client.KSql.RestApi.Parsers;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using ksqlDb.RestApi.Client.KSql.RestApi.Statements.Providers;
using static ksqlDB.RestApi.Client.KSql.RestApi.Enums.IdentifierEscaping;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Generators;

internal class TypeGenerator : CreateEntityStatement
{
  private static readonly EntityProvider EntityProvider = new();

  internal string Print<T>(TypeProperties properties)
  {
    StringBuilder stringBuilder = new();
    var typeName = EntityProvider.GetFormattedName<T>(properties, EscapeName).ToUpper();
    stringBuilder.Append($"CREATE TYPE {typeName} AS STRUCT<");

    PrintProperties<T>(stringBuilder, properties.IdentifierEscaping);

    stringBuilder.Append(">;");

    return stringBuilder.ToString();
  }

  private void PrintProperties<T>(StringBuilder stringBuilder, IdentifierEscaping escaping)
  {
    var ksqlProperties = new List<string>();

    foreach (var memberInfo in Members<T>())
    {
      var type = GetMemberType(memberInfo);

      var ksqlType = CreateEntity.KSqlTypeTranslator(type, escaping);

      var columnDefinition = $"{EscapeName(memberInfo.Name, escaping)} {ksqlType}{CreateEntity.ExploreAttributes(memberInfo, type)}";
      ksqlProperties.Add(columnDefinition);
    }

    stringBuilder.Append(string.Join(", ", ksqlProperties));
  }
  
  private static string EscapeName(string name, IdentifierEscaping escaping) =>
    (escaping, IdentifierUtil.IsValid(name)) switch
    {
      (Never, _) => name,
      (Keywords, true) => name,
      (Keywords, false) => $"`{name}`",
      (Always, _) => $"`{name}`",
      _ => throw new ArgumentOutOfRangeException(nameof(escaping), escaping, "Non-exhaustive match")
    };
}
