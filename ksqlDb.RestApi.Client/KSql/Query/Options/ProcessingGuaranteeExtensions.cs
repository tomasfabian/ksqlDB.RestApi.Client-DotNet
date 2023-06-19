namespace ksqlDB.RestApi.Client.KSql.Query.Options;

public static class ProcessingGuaranteeExtensions
{
  internal const string AtLeastOnce = "at_least_once";
  internal const string ExactlyOnce = "exactly_once";
  internal const string ExactlyOnceV2 = "exactly_once_v2";

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
