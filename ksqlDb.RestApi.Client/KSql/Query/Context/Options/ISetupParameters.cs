using System.Text.Json;
using ksqlDB.RestApi.Client.KSql.Query.Options;
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
  ISetupParameters SetAutoOffsetReset(AutoOffsetReset autoOffsetReset);

  ISetupParameters SetupQuery(Action<IKSqlDbParameters> configure);
#if !NETSTANDARD
    ISetupParameters SetupQueryStream(Action<IKSqlDbParameters> configure);
#endif
  ISetupParameters SetBasicAuthCredentials(string username, string password);

  /// <summary>
  /// Interception of JsonSerializerOptions.
  /// </summary>
  /// <param name="optionsAction">Action to configure the JsonSerializerOptions for the materialization of the incoming values.</param>
  /// <returns>The original KSqlDb context options builder</returns>
  ISetupParameters SetJsonSerializerOptions(Action<JsonSerializerOptions> optionsAction);
}