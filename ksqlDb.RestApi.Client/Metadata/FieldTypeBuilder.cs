namespace ksqlDb.RestApi.Client.Metadata
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
