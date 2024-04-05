using ksqlDb.RestApi.Client.Metadata;

namespace ksqlDb.RestApi.Client.FluentAPI.Builders
{
  public interface IFieldTypeBuilder<TProperty>
  {
    internal IFieldTypeBuilder<TProperty> Ignore();
  }

  internal class FieldTypeBuilder<TProperty>(FieldMetadata fieldMetadata)
    : IFieldTypeBuilder<TProperty>
  {
    public IFieldTypeBuilder<TProperty> Ignore()
    {
      fieldMetadata.Ignore = true;
      return this;
    }
  }
}
