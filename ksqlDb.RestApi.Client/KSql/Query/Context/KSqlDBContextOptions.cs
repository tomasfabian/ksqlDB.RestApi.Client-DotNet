using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using ksqlDB.RestApi.Client.KSql.Config;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using Microsoft.Extensions.DependencyInjection;

namespace ksqlDB.RestApi.Client.KSql.Query.Context;

/// <summary>
/// Options class for ksqlDB context configuration.
/// </summary>
public sealed class KSqlDBContextOptions : KSqlDbProviderOptions
{
  /// <summary>
  /// Creates a new instance of <see cref="KSqlDBContextOptions"/> with the specified ksqlDB REST API URL.
  /// </summary>
  /// <param name="url">The ksqlDB REST API URL.</param>
  public KSqlDBContextOptions(string url)
  {
    if(string.IsNullOrEmpty(url))
      throw new ArgumentNullException(nameof(url));

    Url = url;

    QueryStreamParameters = new QueryStreamParameters();
    QueryStreamParameters.Set(QueryStreamParameters.AutoOffsetResetPropertyName, AutoOffsetReset.Earliest);

    PullQueryParameters = new PullQueryParameters();

#if !NETSTANDARD
    EndpointType = EndpointType.QueryStream;
#endif
  }

  internal IServiceCollection ServiceCollection { get; set; } = new ServiceCollection();

  /// <summary>
  /// Gets or sets a value indicating whether table or stream name should be pluralized from the item name (by default: true).
  /// </summary>
  public bool ShouldPluralizeFromItemName { get; set; } = true;

  /// <summary>
  /// Gets the ksqlDB REST API URL.
  /// </summary>
  public string Url { get; }

  /// <summary>
  /// Gets or sets the query stream parameters.
  /// </summary>
  public QueryStreamParameters QueryStreamParameters { get; internal set; }

  /// <summary>
  /// Gets or sets the pull query parameters.
  /// </summary>
  public IPullQueryParameters PullQueryParameters { get; internal set; }

  public static NumberFormatInfo? NumberFormatInfo { get; set; }

  /// <summary>
  /// Enable exactly-once or at_least_once semantics.
  /// </summary>
  /// <param name="processingGuarantee">Type of processing guarantee.</param>
  public void SetProcessingGuarantee(ProcessingGuarantee processingGuarantee)
  {
    QueryStreamParameters.Set(KSqlDbConfigs.ProcessingGuarantee, processingGuarantee);
  }

  /// <summary>
  /// Sets the auto offset reset using <see cref="AutoOffsetReset"/>.
  /// </summary>
  /// <param name="autoOffsetReset">The type of auto offset reset.</param>
  public void SetAutoOffsetReset(AutoOffsetReset autoOffsetReset)
  {
    QueryStreamParameters.Set(QueryStreamParameters.AutoOffsetResetPropertyName, autoOffsetReset);
  }

  /// <summary>
  /// Interception of JsonSerializerOptions.
  /// </summary>
  /// <param name="optionsAction">Action to configure the JsonSerializerOptions for the materialization of the incoming values.</param>
  /// <returns>The original ksqlDB context options builder</returns>
  public void SetJsonSerializerOptions(Action<JsonSerializerOptions> optionsAction)
  {
    optionsAction.Invoke(JsonSerializerOptions);
  }

  public bool UseBasicAuth => userName != null || password != null;

  private string? userName;
  internal string? BasicAuthUserName => string.IsNullOrEmpty(userName) ? string.Empty : userName;

  private string? password;
  internal string? BasicAuthPassword => string.IsNullOrEmpty(password) ? string.Empty : password;

  /// <summary>
  /// Gets or sets the identifier escaping type.
  /// As ksqlDB automatically converts all identifiers to uppercase by default, it's crucial to enclose them within backticks to maintain the desired casing.
  /// </summary>
  public IdentifierEscaping IdentifierEscaping { get; set; }

  public EndpointType EndpointType { get; set; }

  /// <summary>
  /// Sets the basic authentication credentials.
  /// Note: Credentials are stored only when using <see cref="UseBasicAuth"/>.
  /// </summary>
  /// <param name="userName">The username.</param>
  /// <param name="password">The password.</param>
  public void SetBasicAuthCredentials(string userName, string password)
  {
    this.userName = userName;
    this.password = password;
  }
}
