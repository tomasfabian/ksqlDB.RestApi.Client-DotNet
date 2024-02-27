using System.Collections.Immutable;
using System.Globalization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Enums
{
  public static class SystemColumns
  {
    public const string ROWTIME = "ROWTIME";
    public const string ROWOFFSET = "ROWOFFSET";
    public const string ROWPARTITION = "ROWPARTITION";
    public const string WINDOWSTART = "WINDOWSTART";
    public const string WINDOWEND = "WINDOWEND";

    public static ISet<string> Columns { get; } =
      ImmutableHashSet.Create(ROWTIME, ROWOFFSET, ROWPARTITION, WINDOWSTART, WINDOWEND);

    public static bool IsValid(string identifier) => !Columns.Contains(identifier.ToUpper(CultureInfo.InvariantCulture));
  }
}
