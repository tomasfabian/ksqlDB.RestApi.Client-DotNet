namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;

/// <summary>
/// Opt-in formatting of <see cref="Enum"/> values in insert statements.
/// Kept separate from <see cref="IValueFormatters"/> so existing implementers are not broken.
/// </summary>
public interface IEnumValueFormatter
{
  /// <summary>
  /// Returns the raw text of the enum value, without surrounding quotes (they are added by the library).
  /// Like for strings, single quotes in the text are not escaped. When null, <see cref="object.ToString"/> is used.
  /// </summary>
  public Func<Enum, string>? FormatEnumValue { get; set; }
}
