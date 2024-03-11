using ksqlDb.RestApi.Client.KSql.RestApi.Generators.Asserts;
using ksqlDb.RestApi.Client.KSql.RestApi.Responses.Asserts;

namespace ksqlDB.RestApi.Client.KSql.RestApi;

#nullable enable
public interface IKSqlDbAssertionsRestApiClient
{
  /// <summary>
  /// Asserts that a topic exists or does not exist. ksqldb v 0.27.1
  /// </summary>
  /// <param name="options">The assert topic options such as topic name and timeout.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>Assert topic responses. If the assertion fails, then an error will be returned.</returns>
  Task<AssertTopicResponse[]> AssertTopicExistsAsync(AssertTopicOptions options, CancellationToken cancellationToken = default);

  /// <summary>
  /// Asserts that a topic exists or does not exist. ksqldb v 0.27.1
  /// </summary>
  /// <param name="options">The assert topic options such as topic name and timeout.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>Assert topic responses. If the assertion fails, then an error will be returned.</returns>
  Task<AssertTopicResponse[]> AssertTopicNotExistsAsync(AssertTopicOptions options, CancellationToken cancellationToken = default);

  /// <summary>
  /// Asserts that a schema exists or does not exist. ksqldb v 0.27.1
  /// </summary>
  /// <param name="options">The assert schema options such as subject name, id and timeout.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>Assert schema responses. If the assertion fails, then an error will be returned.</returns>
  Task<AssertSchemaResponse[]> AssertSchemaExistsAsync(AssertSchemaOptions options, CancellationToken cancellationToken = default);

  /// <summary>
  /// Asserts that a schema exists or does not exist. ksqldb v 0.27.1
  /// </summary>
  /// <param name="options">The assert schema options such as subject name, id and timeout.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>Assert schema responses. If the assertion fails, then an error will be returned.</returns>
  Task<AssertSchemaResponse[]> AssertSchemaNotExistsAsync(AssertSchemaOptions options, CancellationToken cancellationToken = default);
}
