using System.Linq.Expressions;
using System.Reflection;

namespace ksqlDb.RestApi.Client.Metadata
{
  internal class ModelBuilder
  {
    private readonly IDictionary<Type, EntityTypeBuilder> builders = new Dictionary<Type, EntityTypeBuilder>(); //TODO: concurrent

    public IEnumerable<EntityMetadata> GetEntities()
    {
      return builders.Values.Select(c => c.Metadata);
    }

    public EntityTypeBuilder<TEntity> Entity<TEntity>()
      where TEntity : class
    {
      if (builders.ContainsKey(typeof(TEntity)))
        return (EntityTypeBuilder<TEntity>)builders[typeof(TEntity)];

      var builder = (EntityTypeBuilder)Activator.CreateInstance(typeof(EntityTypeBuilder<>).MakeGenericType(typeof(TEntity)))!;

      builders[typeof(TEntity)] = builder ?? throw new Exception($"Failed to create entity type builder for {nameof(TEntity)}");
      return (EntityTypeBuilder<TEntity>)builder;
    }
  }

  internal class EntityTypeBuilder
  {
    public EntityMetadata Metadata { get; } = new();
  }

  internal class EntityTypeBuilder<TEntity> : EntityTypeBuilder
    where TEntity : class
  {
    public EntityTypeBuilder()
    {
      Metadata.Type = typeof(TEntity);
    }

    public EntityTypeBuilder<TEntity> HasKey<TProperty>(Expression<Func<TEntity, TProperty>> getProperty)
    {
      Metadata.PrimaryKeyMemberInfo = getProperty.GetMemberInfo();
      return this;
    }

    public FieldTypeBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> getProperty)
    {
      var members = getProperty.GetMembers().Reverse();

      FieldTypeBuilder<TProperty> builder = null!;
      string path = string.Empty;
      foreach (var (memberName, memberInfo) in members)
      {
        path += memberName;
        var fieldMetadata = new FieldMetadata()
        {
          MemberInfo = memberInfo,
          Path = memberName,
          FullPath = path,
        };
        switch (Type.GetTypeCode(typeof(TProperty)))
        {
          case TypeCode.Decimal:
            var decimalFieldMetadata = new DecimalFieldMetadata(fieldMetadata);
            builder = new DecimalFieldTypeBuilder<TProperty>(decimalFieldMetadata);
            fieldMetadata = decimalFieldMetadata;
            break;
          default:
            builder = new(fieldMetadata);
            break;
        }
        Metadata.FieldsMetadataDict[memberInfo] = fieldMetadata;
        path += ".";
      }

      return builder;
    }
  }

  internal class EntityMetadata
  {
    public Type Type { get; internal set; } = null!;
    internal MemberInfo? PrimaryKeyMemberInfo { get; set; }

    internal readonly IDictionary<MemberInfo, FieldMetadata> FieldsMetadataDict = new Dictionary<MemberInfo, FieldMetadata>();
    public IEnumerable<FieldMetadata> FieldsMetadata => FieldsMetadataDict.Values;
  }

  public record FieldMetadata
  {
    public MemberInfo MemberInfo { get; init; } = null!;
    public bool Ignore { get; internal set; }
    internal string Path { get; init; } = null!;
    internal string FullPath { get; init; } = null!;
  }

  public record DecimalFieldMetadata : FieldMetadata
  {
    public DecimalFieldMetadata(FieldMetadata fieldMetadata)
    {
      MemberInfo = fieldMetadata.MemberInfo;
      Ignore = fieldMetadata.Ignore;
      Path = fieldMetadata.Path;
      FullPath = fieldMetadata.FullPath;
    }

    public short Precision { get; internal set; }
    public short Scale { get; internal set; }
  }

  public class DecimalFieldTypeBuilder<TProperty>(DecimalFieldMetadata fieldMetadata)
    : FieldTypeBuilder<TProperty>(fieldMetadata)
  {
    internal FieldTypeBuilder PrecisionInt(short precision, short scale)
    {
      fieldMetadata.Precision = precision;
      fieldMetadata.Scale = scale;
      return this;
    }
  }

  public class FieldTypeBuilder<TProperty>(FieldMetadata fieldMetadata) : FieldTypeBuilder(fieldMetadata)
  {
  }

  public class FieldTypeBuilder(FieldMetadata fieldMetadata)
  {
    public FieldTypeBuilder Ignore()
    {
      fieldMetadata.Ignore = true;
      return this;
    }
  }

  public static class PropertyInfoExtractor
  {
    public static IEnumerable<(string, MemberInfo)> GetMembers<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> propertyExpression)
    {
      if (propertyExpression.Body is not MemberExpression memberExpression)
      {
        throw new ArgumentException("Expression is not a member access expression.", nameof(propertyExpression));
      }

      yield return (memberExpression.Member.Name, memberExpression.Member);

      while (memberExpression.Expression is MemberExpression expression)
      {
        memberExpression = expression;
        yield return (memberExpression.Member.Name, memberExpression.Member);
      }
    }

    public static MemberInfo GetMemberInfo<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> getProperty)
    {
      if (getProperty.Body is not MemberExpression memberExpression)
      {
        throw new ArgumentException("Expression is not a member expression.");
      }

      return memberExpression.Member;
    }
  }

  public static class Ext
  {
    public static FieldTypeBuilder<decimal> Precision(this FieldTypeBuilder<decimal> builder, short precision, short scale)
    {
      ((DecimalFieldTypeBuilder<decimal>)builder).PrecisionInt(precision, scale);
      return builder;
    }
  }
}
