using System;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Functions
{
  internal static class KSqlInvocationFunctionsExtensions
  {
    /// <summary>
    /// Transform a collection by using a lambda function.
    /// </summary>
    public static U Transform<T, U>(this KSqlFunctions kSqlFunctions, T[] array, Func<T, U> selector)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }
  }
}