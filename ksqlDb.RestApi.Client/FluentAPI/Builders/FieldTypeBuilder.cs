using ksqlDb.RestApi.Client.Metadata;

namespace ksqlDb.RestApi.Client.FluentAPI.Builders
{
  /// <summary>
  /// Represents a builder for configuring field types.
  /// </summary>
  /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
  public interface IFieldTypeBuilder<TProperty>
  {
    /// <summary>
    /// Marks the field as ignored, excluding it from the entity's schema.
    /// </summary>
    /// <returns>The field type builder for chaining additional configuration.</returns>
    public IFieldTypeBuilder<TProperty> Ignore();

    /// <summary>
    /// Marks the field as HEADERS.
    /// </summary>
    /// <returns>The field type builder for chaining additional configuration.</returns>
    public IFieldTypeBuilder<TProperty> WithHeaders();

    /// <summary>
    /// Configures the column name that the property will be mapped to in the record schema.
    /// </summary>
    /// <param name="columnName">The name of the column in the record schema.</param>
    /// <returns>The same <see cref="IFieldTypeBuilder{TProperty}"/> instance so that multiple calls can be chained.</returns>
    IFieldTypeBuilder<TProperty> HasColumnName(string columnName);

    /// <summary>
    /// Marks the field as a ksqldb STRUCT type.
    /// </summary>
    /// <returns>The field type builder for chaining additional configuration.</returns>
    IFieldTypeBuilder<TProperty> AsStruct();
  }

  internal class FieldTypeBuilder<TProperty>(FieldMetadata fieldMetadata)
    : IFieldTypeBuilder<TProperty>
  {
    public IFieldTypeBuilder<TProperty> HasColumnName(string columnName)
    {
      fieldMetadata.ColumnName = columnName;
      return this;
    }

    public IFieldTypeBuilder<TProperty> AsStruct()
    {
      fieldMetadata.IsStruct = true;
      return this;
    }

    public IFieldTypeBuilder<TProperty> Ignore()
    {
      fieldMetadata.Ignore = true;
      return this;
    }

    public IFieldTypeBuilder<TProperty> WithHeaders()
    {
      fieldMetadata.HasHeaders = true;
      return this;
    }
  }
}
