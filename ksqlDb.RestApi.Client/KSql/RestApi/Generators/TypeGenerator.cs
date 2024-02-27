using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDb.RestApi.Client.KSql.RestApi.Parsers;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Generators;

internal class TypeGenerator : CreateEntityStatement
{
  internal string Print<T>(string typeName = null, IdentifierFormat format = IdentifierFormat.None)
  {
    StringBuilder stringBuilder = new();
    var type = typeof(T);

    if(string.IsNullOrEmpty(typeName))
    {
      typeName = type.ExtractTypeName();

      typeName = typeName.ToUpper();
    }

    stringBuilder.Append($"CREATE TYPE {typeName} AS STRUCT<");

    PrintProperties<T>(stringBuilder, format);

    stringBuilder.Append(">;");

    return stringBuilder.ToString();
  }

  private void PrintProperties<T>(StringBuilder stringBuilder, IdentifierFormat format)
  {
    var ksqlProperties = new List<string>();

    foreach (var memberInfo in Members<T>())
    {
      var type = GetMemberType(memberInfo);

      var ksqlType = CreateEntity.KSqlTypeTranslator(type, format);

      string columnDefinition;
      switch (format, IdentifierUtil.IsValid(memberInfo.Name))
      {
        case (IdentifierFormat.None, _):
        case (IdentifierFormat.Keywords, true):
          columnDefinition = $"{memberInfo.Name} {ksqlType}{CreateEntity.ExploreAttributes(memberInfo, type)}";
          ksqlProperties.Add(columnDefinition);
          break;
        case (IdentifierFormat.Keywords, false):
        case (IdentifierFormat.Always, _):
          columnDefinition = $"`{memberInfo.Name}` {ksqlType}{CreateEntity.ExploreAttributes(memberInfo, type)}";
          ksqlProperties.Add(columnDefinition);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(format), format, "Non-exhaustive match");
      }

    }

    stringBuilder.Append(string.Join(", ", ksqlProperties));
  }
}
