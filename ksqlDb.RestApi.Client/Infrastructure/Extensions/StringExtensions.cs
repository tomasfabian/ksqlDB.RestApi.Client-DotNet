using System.Linq;
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
  public static bool IsNotNullOrEmpty(this string text)
  {
    return !string.IsNullOrEmpty(text);
  }	
}