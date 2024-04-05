namespace ksqlDb.RestApi.Client.Metadata
{
  internal record DecimalFieldMetadata : FieldMetadata
  {
    public DecimalFieldMetadata(FieldMetadata fieldMetadata)
    {
      MemberInfo = fieldMetadata.MemberInfo;
      Ignore = fieldMetadata.Ignore;
      Path = fieldMetadata.Path;
      FullPath = fieldMetadata.FullPath;
    }

    public short Precision { get; internal set; }

    public short Scale { get; internal set; }
  }
}
