using System;
using System.Globalization;
using System.Linq;
using Kafka.DotNet.ksqlDB.KSql.Config;
using Kafka.DotNet.ksqlDB.KSql.Query.Options;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Context
{
  public sealed class KSqlDBContextOptions
  {
    public KSqlDBContextOptions(string url)
    {
      if(string.IsNullOrEmpty(url))
        throw new ArgumentNullException(nameof(url));

      Url = url;

      QueryParameters ??= new QueryParameters
      {
        [RestApi.Parameters.QueryParameters.AutoOffsetResetPropertyName] = AutoOffsetReset.Earliest.ToString().ToLower()
      };

      QueryStreamParameters ??= new QueryStreamParameters
      {
        [QueryStreamParameters.AutoOffsetResetPropertyName] = AutoOffsetReset.Earliest.ToString().ToLower(),
      };
    }

    public bool ShouldPluralizeFromItemName { get; set; } = true;

    public string Url { get; }

    public QueryStreamParameters QueryStreamParameters { get; internal set; }

    public IKSqlDbParameters QueryParameters { get; internal set; }

    public static NumberFormatInfo NumberFormatInfo { get; set; }

    /// <summary>
    /// Enable exactly-once or at_least_once semantics.
    /// </summary>
    /// <param name="processingGuarantee">Type of processing guarantee.</param>
    public void SetProcessingGuarantee(ProcessingGuarantee processingGuarantee)
    {
      string guarantee = processingGuarantee.ToKSqlValue();

      QueryStreamParameters[KSqlDbConfigs.ProcessingGuarantee] = guarantee; 
      QueryParameters[KSqlDbConfigs.ProcessingGuarantee] = guarantee;
    }

    public void SetAutoOffsetReset(AutoOffsetReset autoOffsetReset)
    {
      QueryParameters[RestApi.Parameters.QueryParameters.AutoOffsetResetPropertyName] =
        autoOffsetReset.ToKSqlValue();

      QueryStreamParameters[QueryStreamParameters.AutoOffsetResetPropertyName] =
        autoOffsetReset.ToKSqlValue();
    }

    internal KSqlDBContextOptions Clone()
    {
      var options = new KSqlDBContextOptions(Url)
      {
        ShouldPluralizeFromItemName = ShouldPluralizeFromItemName,
        QueryParameters = ((QueryParameters) QueryParameters).Clone(),
        QueryStreamParameters = QueryStreamParameters.Clone() as QueryStreamParameters
      };

      return options;
    }

    public bool UseBasicAuth => userName != null || password != null;

    private string userName;
    internal string BasicAuthUserName => string.IsNullOrEmpty(userName) ? "" : userName;

    private string password;
    internal string BasicAuthPassword => string.IsNullOrEmpty(password) ? "" : password;

    public void SetBasicAuthCredentials(string userName, string password)
    {
      this.userName = userName;
      this.password = password;
    }
  }
}