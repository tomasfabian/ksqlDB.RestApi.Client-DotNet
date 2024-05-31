using System.Reflection;

namespace ksqlDb.RestApi.Client.Metadata
{
  internal record FieldMetadata
  {
    internal MemberInfo MemberInfo { get; init; } = null!;
    public bool Ignore { get; internal set; }
    public bool HasHeaders { get; internal set; }
    internal string Path { get; init; } = null!;
    internal string FullPath { get; init; } = null!;
    public string? ColumnName { get; set; }
  }
}
