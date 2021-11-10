using System.Collections.Generic;
using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Generators
{
  internal class TypeGenerator : CreateEntityStatement
  {
    private readonly StringBuilder stringBuilder = new();

    internal string Print<T>(string typeName = null)
    {
      stringBuilder.Clear();

      var type = typeof(T);
      
      if(string.IsNullOrEmpty(typeName))
      {
        typeName = type.ExtractTypeName();

        typeName = typeName.ToUpper();
      }

      stringBuilder.Append($"CREATE TYPE {typeName} AS STRUCT<");

      PrintProperties<T>();

      stringBuilder.Append(">;");

      return stringBuilder.ToString();
    }

    private void PrintProperties<T>()
    {
      var ksqlProperties = new List<string>();

      foreach (var memberInfo in Members<T>())
      {
        var type = GetMemberType<T>(memberInfo);

        var ksqlType = CreateEntity.KSqlTypeTranslator(type);

        string columnDefinition = $"{memberInfo.Name} {ksqlType}{CreateEntity.ExploreAttributes(memberInfo, type)}";

        ksqlProperties.Add(columnDefinition);
      }

      stringBuilder.Append(string.Join(", ", ksqlProperties));
    }
  }
}