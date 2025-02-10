using System.Text.Json;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

namespace ksqlDB.RestApi.Client.KSql.Query.Context.Options;

public interface ISetupParameters : ICreateOptions
{
  /// <summary>
  /// Enable exactly-once or at_least_once semantics
  /// </summary>
  /// <param name="processingGuarantee">Type of processing guarantee.</param>
  /// <returns>Returns this instance.</returns>
  ISetupParameters SetProcessingGuarantee(ProcessingGuarantee processingGuarantee);

  /// <summary>
  /// Allows you to configure the auto.offset.reset streams property.
  /// </summary>
  /// <param name="autoOffsetReset">The auto offset reset value to set.</param>
  /// <returns>Returns this instance.</returns>
  ISetupParameters SetAutoOffsetReset(AutoOffsetReset autoOffsetReset);

  /// <summary>
  /// Configures the parameters for setting up a push query.
  /// Allows you to configure ksqlDB query parameters such as processing guarantee or 'auto.offset.reset'.
  /// </summary>
  /// <param name="configure">An action to configure the parameters using <see cref="IPushQueryParameters"/>.</param>
  /// <returns>Returns this instance.</returns>
  ISetupParameters SetupPushQuery(Action<IPushQueryParameters> configure);

  /// <summary>
  /// Configures the parameters for setting up a pull query.
  /// </summary>
  /// <param name="configure">An action to configure the parameters using <see cref="IPullQueryParameters"/>.</param>
  /// <returns>Returns this instance.</returns>
  ISetupParameters SetupPullQuery(Action<IPullQueryParameters> configure);

  /// <summary>
  /// Allows you to set basic authentication credentials for an HTTP client.
  /// </summary>
  /// <param name="username">User name</param>
  /// <param name="password">Password</param>
  /// <returns>Returns this instance.</returns>
  ISetupParameters SetBasicAuthCredentials(string username, string password);

  /// <summary>
  /// Interception of JsonSerializerOptions.
  /// </summary>
  /// <param name="optionsAction">Action to configure the JsonSerializerOptions for the materialization of the incoming values.</param>
  /// <returns>The original KSqlDb context options builder</returns>
  ISetupParameters SetJsonSerializerOptions(Action<JsonSerializerOptions> optionsAction);

  /// <summary>
  /// Sets the KSQL query endpoints when using pull and push queries.
  /// </summary>
  ISetupParameters SetEndpointType(EndpointType endpointType);

  /// <summary>
  /// Sets the identifier escaping type.
  /// As ksqlDB automatically converts all identifiers to uppercase by default, it's crucial to enclose them within backticks to maintain the desired casing.
  /// </summary>
  ISetupParameters SetIdentifierEscaping(IdentifierEscaping escaping);

  /// <summary>
  /// Allows you to set basic authentication usage through a DelegatingHandler
  /// </summary>
  /// <returns>Returns this instance.</returns>
  ISetupParameters SetBasicAuth();
}
