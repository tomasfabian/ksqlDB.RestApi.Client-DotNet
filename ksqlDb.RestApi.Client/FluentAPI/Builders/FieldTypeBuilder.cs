using System.Linq.Expressions;
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
    /// Marks the field as ignored, excluding it from the entity's schema, preventing it from being included in both DDL and DML statements. 
    /// </summary>
    /// <returns>The field type builder for chaining additional configuration.</returns>
    public IFieldTypeBuilder<TProperty> Ignore();

    /// <summary>
    /// Marks the field to be excluded from data manipulation operations, preventing it from being included in DML statements such as INSERT.
    /// </summary>
    /// <returns>The field type builder for chaining additional configuration.</returns>
    internal IFieldTypeBuilder<TProperty> IgnoreInDML();

    /// <summary>
    /// Marks the field to be excluded from data definition operations, preventing it from being included in DDL statements such as CREATE STREAM or TABLE.
    /// </summary>
    /// <returns>The field type builder for chaining additional configuration.</returns>
    public IFieldTypeBuilder<TProperty> IgnoreInDDL();

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

    public IFieldTypeBuilder<TProperty> IgnoreInDML()
    {
      fieldMetadata.IgnoreInDML = true;
      return this;
    }

    public IFieldTypeBuilder<TProperty> IgnoreInDDL()
    {
      fieldMetadata.IgnoreInDDL = true;
      return this;
    }

    public IFieldTypeBuilder<TProperty> WithHeaders()
    {
      fieldMetadata.HasHeaders = true;
      IgnoreInDML();
      return this;
    }
  }
}
