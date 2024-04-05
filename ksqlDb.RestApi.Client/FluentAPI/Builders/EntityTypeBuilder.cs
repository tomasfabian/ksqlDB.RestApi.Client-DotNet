using System.Linq.Expressions;
using ksqlDb.RestApi.Client.Metadata;

namespace ksqlDb.RestApi.Client.FluentAPI.Builders
{
  public interface IEntityTypeBuilder<TEntity>
  {
    IFieldTypeBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> getProperty);
    IEntityTypeBuilder<TEntity> HasKey<TProperty>(Expression<Func<TEntity, TProperty>> getProperty);
  }

  internal class EntityTypeBuilder
  {
    internal EntityMetadata Metadata { get; } = new();
  }

  internal sealed class EntityTypeBuilder<TEntity> : EntityTypeBuilder, IEntityTypeBuilder<TEntity>
    where TEntity : class
  {
    public EntityTypeBuilder()
    {
      Metadata.Type = typeof(TEntity);
    }

    public IEntityTypeBuilder<TEntity> HasKey<TProperty>(Expression<Func<TEntity, TProperty>> getProperty)
    {
      Metadata.PrimaryKeyMemberInfo = getProperty.GetMemberInfo();
      return this;
    }

    public IFieldTypeBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> getProperty)
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
}
