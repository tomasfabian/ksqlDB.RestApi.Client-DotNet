namespace ksqlDB.RestApi.Client.KSql.Query.Options;

/// <summary>
/// Provides extension methods for converting between string values and <see cref="ProcessingGuarantee"/> enum,
/// as well as converting <see cref="ProcessingGuarantee"/> values to their corresponding string representation for KSql.
/// </summary>
public static class ProcessingGuaranteeExtensions
{
  internal const string AtLeastOnce = "at_least_once";
  internal const string ExactlyOnce = "exactly_once";
  internal const string ExactlyOnceV2 = "exactly_once_v2";

  /// <summary>
  /// Converts a string value to <see cref="ProcessingGuarantee"/>.
  /// </summary>
  /// <param name="processingGuaranteeValue">The string value representing the processing guarantee.</param>
  /// <returns>The corresponding <see cref="ProcessingGuarantee"/> value.</returns>
  /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided <paramref name="processingGuaranteeValue"/> is not a valid option.</exception>
  public static ProcessingGuarantee ToProcessingGuarantee(this string processingGuaranteeValue)
  {
    return processingGuaranteeValue switch
    {
      AtLeastOnce => ProcessingGuarantee.AtLeastOnce,
      ExactlyOnce => ProcessingGuarantee.ExactlyOnce,
      ExactlyOnceV2 => ProcessingGuarantee.ExactlyOnceV2,
      _ => throw new ArgumentOutOfRangeException(nameof(processingGuaranteeValue), processingGuaranteeValue, null)
    };
  }

  /// <summary>
  /// Converts a <see cref="ProcessingGuarantee"/> value to its corresponding string representation for KSql.
  /// </summary>
  /// <param name="processingGuarantee">The <see cref="ProcessingGuarantee"/> value.</param>
  /// <returns>The string representation of the <see cref="ProcessingGuarantee"/> value for KSql.</returns>
  /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided <paramref name="processingGuarantee"/> is not a valid option.</exception>
  public static string ToKSqlValue(this ProcessingGuarantee processingGuarantee)
  {      
    string guarantee = processingGuarantee switch
    {
      ProcessingGuarantee.AtLeastOnce => AtLeastOnce,
      ProcessingGuarantee.ExactlyOnce => ExactlyOnce,
      ProcessingGuarantee.ExactlyOnceV2 => ExactlyOnceV2,
      _ => throw new ArgumentOutOfRangeException(nameof(processingGuarantee), processingGuarantee, null)
    };

    return guarantee;
  }
}
