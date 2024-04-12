using System.Linq.Expressions;
using ksqlDb.RestApi.Client.Metadata;

namespace ksqlDb.RestApi.Client.FluentAPI.Builders
{
  /// <summary>
  /// Represents a builder for configuring entity types.
  /// </summary>
  /// <typeparam name="TEntity">The type of entity being configured.</typeparam>
  public interface IEntityTypeBuilder<TEntity>
  {
    /// <summary>
    /// Configures a property of the entity.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="getProperty">An expression representing the property to configure.</param>
    /// <returns>The field type builder for further configuration.</returns>
    IFieldTypeBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> getProperty);

    /// <summary>
    /// Specifies the key property for the entity.
    /// </summary>
    /// <typeparam name="TProperty">The type of the key property.</typeparam>
    /// <param name="getProperty">An expression representing the key property.</param>
    /// <returns>The entity type builder for further configuration.</returns>
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
