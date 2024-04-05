using System.Linq.Expressions;

namespace ksqlDb.RestApi.Client.Metadata
{
  public interface IEntityTypeBuilder<TEntity>
  {
    IFieldTypeBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> getProperty);
    IEntityTypeBuilder<TEntity> HasKey<TProperty>(Expression<Func<TEntity, TProperty>> getProperty);
  }

  internal class EntityTypeBuilder
  {
    public EntityMetadata Metadata { get; } = new();
  }

  internal class EntityTypeBuilder<TEntity> : EntityTypeBuilder, IEntityTypeBuilder<TEntity>
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
