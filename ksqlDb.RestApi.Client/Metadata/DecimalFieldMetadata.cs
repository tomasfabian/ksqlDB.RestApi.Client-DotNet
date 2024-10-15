namespace ksqlDb.RestApi.Client.Metadata
{
  internal sealed record DecimalFieldMetadata : FieldMetadata
  {
    public DecimalFieldMetadata(FieldMetadata fieldMetadata)
     : base(fieldMetadata)
    {
    }

    public short Precision { get; internal set; }

    public short Scale { get; internal set; }
  }
}
