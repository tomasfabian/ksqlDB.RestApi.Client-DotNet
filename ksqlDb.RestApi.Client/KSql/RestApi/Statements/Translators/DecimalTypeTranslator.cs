using System.Reflection;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDb.RestApi.Client.FluentAPI.Builders.Configuration;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDb.RestApi.Client.Metadata;

namespace ksqlDb.RestApi.Client.KSql.RestApi.Statements.Translators
{
  internal class DecimalTypeTranslator(IMetadataProvider modelBuilder)
  {
    internal bool TryGetDecimal(Type? parentType, MemberInfo memberInfo, out string? @decimal)
    {
      if (parentType != null)
      {
        var entityMetadata = modelBuilder.GetEntities().FirstOrDefault(c => c.Type == parentType);
        if (entityMetadata?.FieldsMetadata.FirstOrDefault(c => c.MemberInfo == memberInfo) is DecimalFieldMetadata fieldMetadata)
        {
          @decimal = GetDecimal(fieldMetadata.Precision, fieldMetadata.Scale);
          return true;
        }
      }

      var decimalMember = memberInfo.TryGetAttribute<DecimalAttribute>();

      if (decimalMember != null)
      {
        @decimal = GetDecimal(decimalMember.Precision, decimalMember.Scale);
        return true;
      }

      if (modelBuilder.Conventions.TryGetValue(typeof(decimal), out var conversion))
      {
        if (conversion is DecimalTypeConvention decimalConversion)
        {
          @decimal = GetDecimal(decimalConversion.Precision, decimalConversion.Scale);
          return true;
        }
      }

      @decimal = null;
      return false;
    }

    private string GetDecimal(short precision, short scale)
    {
      return $"({precision},{scale})";
    }
  }
}
