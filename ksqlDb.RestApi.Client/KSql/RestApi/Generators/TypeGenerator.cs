using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Generators;

internal class TypeGenerator : CreateEntityStatement
{
  internal string Print<T>(string typeName = null)
  {
    StringBuilder stringBuilder = new();
    var type = typeof(T);

    if(string.IsNullOrEmpty(typeName))
    {
      typeName = type.ExtractTypeName();

      typeName = typeName.ToUpper();
    }

    stringBuilder.Append($"CREATE TYPE {typeName} AS STRUCT<");

    PrintProperties<T>(stringBuilder);

    stringBuilder.Append(">;");

    return stringBuilder.ToString();
  }

  private void PrintProperties<T>(StringBuilder stringBuilder)
  {
    var ksqlProperties = new List<string>();

    foreach (var memberInfo in Members<T>())
    {
      var type = GetMemberType(memberInfo);

      var ksqlType = CreateEntity.KSqlTypeTranslator(type);

      string columnDefinition = $"{memberInfo.Name} {ksqlType}{CreateEntity.ExploreAttributes(memberInfo, type)}";

      ksqlProperties.Add(columnDefinition);
    }

    stringBuilder.Append(string.Join(", ", ksqlProperties));
  }
}
