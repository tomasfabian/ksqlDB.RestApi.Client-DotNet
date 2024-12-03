using System.Linq.Expressions;
using System.Reflection;
using ksqlDB.RestApi.Client.KSql.Query;
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

      IgnoreRowTime();
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
        var memberInfoKey = memberInfo.ToMemberInfoKey();
        path += memberName;

        if (!Metadata.FieldsMetadataDict.TryGetValue(memberInfoKey, out var fieldMetadata))
        {
          fieldMetadata = new FieldMetadata()
          {
            MemberInfo = memberInfo,
            Path = memberName,
            FullPath = path,
          };
        }

        switch (typeof(TProperty))
        {
          case { } type when type == typeof(decimal):
            var decimalFieldMetadata = fieldMetadata as DecimalFieldMetadata ?? new DecimalFieldMetadata(fieldMetadata);
            builder = new DecimalFieldTypeBuilder<TProperty>(decimalFieldMetadata);
            fieldMetadata = decimalFieldMetadata;
            break;
          case { } type when type == typeof(byte[]):
            var bytesArrayFieldMetadata = fieldMetadata as BytesArrayFieldMetadata ?? new BytesArrayFieldMetadata(fieldMetadata);
            builder = new BytesArrayFieldTypeBuilder<TProperty>(bytesArrayFieldMetadata);
            fieldMetadata = bytesArrayFieldMetadata;
            break;
          default:
            builder = new(fieldMetadata);
            break;
        }

        Metadata.FieldsMetadataDict[memberInfoKey] = fieldMetadata;
        path += ".";
      }

      return builder;
    }

    private readonly BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
    private readonly Type longType = typeof(long);
    private readonly string rowTime = nameof(Record.RowTime);

    internal void IgnoreRowTime()
    {
      var props = Metadata.Type.GetProperties(bindingFlags)
        .Where(p => p.Name == rowTime && p.PropertyType == longType);

      MemberInfo? propertyInfo = props.FirstOrDefault();
      if (propertyInfo != null)
      {
        AddFieldMetadata(propertyInfo, ignoreInDDL: true);
        return;
      }

      var fields = Metadata.Type.GetFields(bindingFlags)
        .Where(p => p.Name == rowTime && p.FieldType == longType);

      MemberInfo? fieldInfo = fields.FirstOrDefault();

      if (fieldInfo != null)
      {
        AddFieldMetadata(fieldInfo, ignoreInDDL: true);
      }
    }

    private void AddFieldMetadata(MemberInfo memberInfo, bool ignoreInDDL)
    {
      var memberInfoKey = memberInfo.ToMemberInfoKey();

      var fieldMetadata = new FieldMetadata
      {
        MemberInfo = memberInfo,
        Path = memberInfo.Name,
        FullPath = memberInfo.Name,
        IgnoreInDDL = ignoreInDDL
      };

      Metadata.FieldsMetadataDict[memberInfoKey] = fieldMetadata;
    }
  }
}
