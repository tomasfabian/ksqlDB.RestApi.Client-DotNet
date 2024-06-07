using System.Linq.Expressions;
using System.Reflection;
using Antlr4.Runtime;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using static ksqlDB.RestApi.Client.KSql.RestApi.Enums.IdentifierEscaping;

namespace ksqlDb.RestApi.Client.KSql.RestApi.Parsers
{
  public static class IdentifierUtil
  {
    /// <summary>
    /// Check if a string is a reserved word.
    /// </summary>
    /// <param name="identifier">the identifier</param>
    /// <returns>whether <c>identifier</c> is a valid identifier without quotes</returns>
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
    /// <param name="escaping">the format</param>
    /// <returns>the identifier modified based on the provided <c>format</c></returns>
    public static string Format(string identifier, IdentifierEscaping escaping)
    {
      return escaping switch
      {
        Never => identifier,
        Keywords when IsValid(identifier) && SystemColumns.IsValid(identifier) => identifier,
        Keywords => string.Concat("`", identifier, "`"),
        Always => string.Concat("`", identifier, "`"),
        _ => throw new ArgumentOutOfRangeException(nameof(escaping), escaping, "Non-exhaustive match.")
      };
    }

    internal static string Format(MemberExpression memberExpression, IdentifierEscaping escaping, IMetadataProvider? metadataProvider = null)
    {
      return escaping switch
      {
        Never => memberExpression.GetMemberName(metadataProvider),
        Keywords when memberExpression.Member.GetCustomAttribute<PseudoColumnAttribute>() != null => memberExpression.Member.Name,
        Keywords when IsValid(memberExpression.GetMemberName(metadataProvider)) && SystemColumns.IsValid(memberExpression.GetMemberName(metadataProvider)) => memberExpression.GetMemberName(metadataProvider),
        Keywords => string.Concat("`", memberExpression.GetMemberName(metadataProvider), "`"),
        Always when memberExpression.Member.GetCustomAttribute<PseudoColumnAttribute>() != null => memberExpression.Member.Name,
        Always => string.Concat("`", memberExpression.GetMemberName(metadataProvider), "`"),
        _ => throw new ArgumentOutOfRangeException(nameof(escaping), escaping, "Non-exhaustive match.")
      };
    }

    internal static string Format(MemberInfo memberInfo, IdentifierEscaping escaping, IMetadataProvider? modelBuilder = null)
    {
      return Format(memberInfo, escaping, modelBuilder as ModelBuilder);
    }

    /// <summary>
    /// Format the <c>identifier</c>, except when it is a <c>PseudoColumn</c>.
    /// </summary>
    /// <param name="memberInfo">the memberInfo with the identifier</param>
    /// <param name="escaping">the format</param>
    /// <param name="modelBuilder">the model builder</param>
    /// <returns>the identifier modified based on the provided <c>format</c></returns>
    public static string Format(MemberInfo memberInfo, IdentifierEscaping escaping, ModelBuilder? modelBuilder = null)
    {
      return escaping switch
      {
        Never => memberInfo.GetMemberName(modelBuilder),
        Keywords when memberInfo.GetCustomAttribute<PseudoColumnAttribute>() != null => memberInfo.Name,
        Keywords when IsValid(memberInfo.GetMemberName(modelBuilder)) && SystemColumns.IsValid(memberInfo.GetMemberName(modelBuilder)) => memberInfo.GetMemberName(modelBuilder),
        Keywords => string.Concat("`", memberInfo.GetMemberName(modelBuilder), "`"),
        Always when memberInfo.GetCustomAttribute<PseudoColumnAttribute>() != null => memberInfo.Name,
        Always => string.Concat("`", memberInfo.GetMemberName(modelBuilder), "`"),
        _ => throw new ArgumentOutOfRangeException(nameof(escaping), escaping, "Non-exhaustive match.")
      };
    }
  }
}
