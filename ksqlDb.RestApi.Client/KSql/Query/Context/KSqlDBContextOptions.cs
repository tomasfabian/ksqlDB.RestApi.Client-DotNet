using System.Globalization;
using System.Text.Json;
using ksqlDB.RestApi.Client.KSql.Config;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using Microsoft.Extensions.DependencyInjection;

namespace ksqlDB.RestApi.Client.KSql.Query.Context;

public sealed class KSqlDBContextOptions : KSqlDbProviderOptions
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

  internal IServiceCollection ServiceCollection { get; set; } = new ServiceCollection();

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

  /// <summary>
  /// Interception of JsonSerializerOptions.
  /// </summary>
  /// <param name="optionsAction">Action to configure the JsonSerializerOptions for the materialization of the incoming values.</param>
  /// <returns>The original KSqlDb context options builder</returns>
  public void SetJsonSerializerOptions(Action<JsonSerializerOptions> optionsAction)
  {
    JsonSerializerOptions ??= KSqlDbJsonSerializerOptions.CreateInstance();

    optionsAction?.Invoke(JsonSerializerOptions);
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

  internal void Apply(IServiceCollection externalServicesCollection)
  {
    foreach (var service in ServiceCollection)
    {
      externalServicesCollection.Add(service);
    }
  }
}