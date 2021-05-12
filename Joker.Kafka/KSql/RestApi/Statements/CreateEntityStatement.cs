using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Properties;
using Pluralize.NET;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Statements
{
  internal class CreateEntityStatement
  {
    protected static readonly IPluralize EnglishPluralizationService = new Pluralizer();
    
    protected IEnumerable<MemberInfo> Members<T>()
    {
      var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);

      var properties = typeof(T)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(c => c.CanWrite).OfType<MemberInfo>()
        .Concat(fields);
      
      return properties.Where(c => !c.GetCustomAttributes().OfType<IgnoreAttribute>().Any());
    }

    protected string GetEntityName<T>(IEntityCreationProperties metadata)
    {
      string entityName = metadata?.EntityName;

      if (string.IsNullOrEmpty(entityName))
        entityName = typeof(T).Name;

      if (metadata != null && metadata.ShouldPluralizeEntityName)
        entityName = EnglishPluralizationService.Pluralize(entityName);

      return entityName;
    }

    protected static Type GetMemberType<T>(MemberInfo memberInfo)
    {
      var type = memberInfo.MemberType switch
      {
        MemberTypes.Field => ((FieldInfo) memberInfo).FieldType,
        MemberTypes.Property => ((PropertyInfo) memberInfo).PropertyType,
      };

      return type;
    }
  }
}