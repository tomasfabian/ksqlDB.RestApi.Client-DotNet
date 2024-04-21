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
  }

  internal class FieldTypeBuilder<TProperty>(FieldMetadata fieldMetadata)
    : IFieldTypeBuilder<TProperty>
  {
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
