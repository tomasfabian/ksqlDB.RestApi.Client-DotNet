using Antlr4.Runtime;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;

namespace ksqlDb.RestApi.Client.KSql.RestApi.Parsers
{
  public static class IdentifierUtil
  {
    /// <summary>
    /// Check if a string is a reserved word.
    /// </summary>
    /// <param name="identifier">the identifier</param>
    /// <returns>whether or not <c>identifier</c> is a valid identifier without quotes</returns>
    public static bool IsValid(string identifier)
    {
      var sqlBaseLexer = new SqlBaseLexer(
        new CaseInsensitiveStream(CharStreams.fromString(identifier)));
      var tokenStream = new CommonTokenStream(sqlBaseLexer);
      var sqlBaseParser = new SqlBaseParser(tokenStream);

      // don't log or print anything in the case of error since this is expected for this method
      sqlBaseLexer.RemoveErrorListeners();
      sqlBaseParser.RemoveErrorListeners();

      sqlBaseParser.identifier();

      return sqlBaseParser.NumberOfSyntaxErrors == 0
             && sqlBaseParser.CurrentToken.Column == identifier.Length;
    }

    /// <summary>
    /// Format the <c>identifier</c>.
    /// </summary>
    /// <param name="identifier">the identifier</param>
    /// <param name="format">the format</param>
    /// <returns>the identifier modified based on the provided <c>format</c></returns>
    public static string Format(string identifier, IdentifierFormat format)
    {
      return format switch
      {
        IdentifierFormat.None => identifier,
        IdentifierFormat.Keywords when IsValid(identifier) && SystemColumns.IsValid(identifier) => identifier,
        IdentifierFormat.Keywords => string.Concat("`", identifier, "`"),
        IdentifierFormat.Always => string.Concat("`", identifier, "`"),
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Non-exhaustive match.")
      };
    }
  }
}
