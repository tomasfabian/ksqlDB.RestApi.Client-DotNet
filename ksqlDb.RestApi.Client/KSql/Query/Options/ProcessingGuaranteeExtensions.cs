using System;

namespace ksqlDB.RestApi.Client.KSql.Query.Options
{
  public static class ProcessingGuaranteeExtensions
  {
    internal const string AtLeastOnce = "at_least_once";
    internal const string ExactlyOnce = "exactly_once";

    public static ProcessingGuarantee ToProcessingGuarantee(this string processingGuaranteeValue)
    {        
      if (processingGuaranteeValue == AtLeastOnce)
        return ProcessingGuarantee.AtLeastOnce;
        
      if (processingGuaranteeValue == ExactlyOnce)
        return ProcessingGuarantee.ExactlyOnce;

      throw new ArgumentOutOfRangeException(nameof(processingGuaranteeValue), processingGuaranteeValue, null);
    }

    public static string ToKSqlValue(this ProcessingGuarantee processingGuarantee)
    {      
      string guarantee = processingGuarantee switch
      {
        ProcessingGuarantee.AtLeastOnce => AtLeastOnce,
        ProcessingGuarantee.ExactlyOnce => ExactlyOnce,
        _ => throw new ArgumentOutOfRangeException(nameof(processingGuarantee), processingGuarantee, null)
      };

      return guarantee;
    }
  }
}