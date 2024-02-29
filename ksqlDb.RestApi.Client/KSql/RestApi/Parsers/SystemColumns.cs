using System.Collections.Immutable;
using System.Globalization;

namespace ksqlDb.RestApi.Client.KSql.RestApi.Parsers
{
  internal static class SystemColumns
  {
    internal const string ROWTIME = "ROWTIME";
    internal const string ROWOFFSET = "ROWOFFSET";
    internal const string ROWPARTITION = "ROWPARTITION";
    internal const string WINDOWSTART = "WINDOWSTART";
    internal const string WINDOWEND = "WINDOWEND";

    private static ISet<string> Columns { get; } =
      ImmutableHashSet.Create(ROWTIME, ROWOFFSET, ROWPARTITION, WINDOWSTART, WINDOWEND);

    public static bool IsValid(string identifier) => !Columns.Contains(identifier.ToUpper(CultureInfo.InvariantCulture));
  }
}
