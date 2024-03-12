using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ksqlDB.RestApi.Client.Infrastructure.Extensions;

internal static class StringExtensions
{
  public static string ToKSqlFunctionName(this string functionName)
  {      
    var words = 
      Regex.Matches(functionName, @"([A-Z][a-z]+)")
        .Cast<Match>()
        .Select(m => m.Value);

    var ksqlFunctionName = string.Join("_", words).ToUpper();

    return ksqlFunctionName.ToUpper();
  }

#if NETSTANDARD2_0
  public static bool IsNotNullOrEmpty(this string? text)
#else
  public static bool IsNotNullOrEmpty([NotNullWhen(false)] this string? text)
#endif
  {
    return !string.IsNullOrEmpty(text);
  }	
}
