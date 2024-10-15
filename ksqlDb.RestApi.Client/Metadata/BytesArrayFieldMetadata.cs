namespace ksqlDb.RestApi.Client.Metadata
{
  internal sealed record BytesArrayFieldMetadata : FieldMetadata
  {
    public BytesArrayFieldMetadata(FieldMetadata fieldMetadata)
      : base(fieldMetadata)
    {
    }

    public string? Header { get; internal set; }
  }
}
