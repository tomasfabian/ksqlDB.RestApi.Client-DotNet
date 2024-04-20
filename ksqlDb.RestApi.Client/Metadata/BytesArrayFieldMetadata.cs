namespace ksqlDb.RestApi.Client.Metadata
{
  internal sealed record BytesArrayFieldMetadata : FieldMetadata
  {
    public BytesArrayFieldMetadata(FieldMetadata fieldMetadata)
    {
      MemberInfo = fieldMetadata.MemberInfo;
      Ignore = fieldMetadata.Ignore;
      Path = fieldMetadata.Path;
      FullPath = fieldMetadata.FullPath;
    }

    public string? Header { get; internal set; }
  }
}
