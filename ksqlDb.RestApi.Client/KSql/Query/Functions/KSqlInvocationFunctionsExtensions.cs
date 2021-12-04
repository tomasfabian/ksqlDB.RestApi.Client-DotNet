using System;
using System.Collections.Generic;

namespace ksqlDB.RestApi.Client.KSql.Query.Functions
{
  public static class KSqlInvocationFunctionsExtensions
  {
    /// <summary>
    /// Apply a function to each element in a collection. 
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <typeparam name="TResult">The type of the value returned by <paramref name="selector" />.</typeparam>
    /// <param name="kSqlFunctions"></param>
    /// <param name="array">The array</param>
    /// <param name="selector">Value selector</param>
    /// <returns>The transformed collection is returned.</returns>
    public static TResult[] Transform<TSource, TResult>(this KSqlFunctions kSqlFunctions, TSource[] array, Func<TSource, TResult> selector)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }

    /// <summary>
    /// Apply a function to each element in a collection. 
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <typeparam name="TResult">The type of the value returned by <paramref name="selector" />.</typeparam>
    /// <param name="kSqlFunctions"></param>
    /// <param name="source">The enumerable source.</param>
    /// <param name="selector">Value selector</param>
    /// <returns>The transformed collection is returned.</returns>
    public static TResult[] Transform<TSource, TResult>(this KSqlFunctions kSqlFunctions, IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }

    /// <summary>
    /// Apply a function to each element in a collection. 
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <typeparam name="TResult">The type of the value returned by <paramref name="selector" />.</typeparam>
    /// <param name="source">The enumerable source.</param>
    /// <param name="selector">Value selector</param>
    /// <returns>The transformed collection is returned.</returns>
    public static TResult[] Transform<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }

    /// <summary>
    /// When transforming a map, two functions must be provided. For each map entry, the first function provided
    /// will be applied to the key and the second one applied to the value. Each function must have two
    /// arguments. The two arguments for each function are in order: the key and then the value. The transformed
    /// map is returned.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TKeyResult"></typeparam>
    /// <typeparam name="TValueResult"></typeparam>
    /// <param name="kSqlFunctions"></param>
    /// <param name="source"></param>
    /// <param name="keySelector"></param>
    /// <param name="valueSelector"></param>
    /// <returns>Apply a function to each element in a collection. The transformed collection is returned.</returns>
    public static IDictionary<TKeyResult, TValueResult> Transform<TKey, TValue, TKeyResult, TValueResult>(this KSqlFunctions kSqlFunctions, IDictionary<TKey, TValue> source, Func<TKey, TValue, TKeyResult> keySelector, Func<TKey, TValue, TValueResult> valueSelector)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }

    /// <summary>
    /// When transforming a map, two functions must be provided. For each map entry, the first function provided
    /// will be applied to the key and the second one applied to the value. Each function must have two
    /// arguments. The two arguments for each function are in order: the key and then the value. The transformed
    /// map is returned.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TKeyResult"></typeparam>
    /// <typeparam name="TValueResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector"></param>
    /// <param name="valueSelector"></param>
    /// <returns>Apply a function to each element in a collection. The transformed collection is returned.</returns>
    public static IDictionary<TKeyResult, TValueResult> Transform<TKey, TValue, TKeyResult, TValueResult>(this IDictionary<TKey, TValue> source, Func<TKey, TValue, TKeyResult> keySelector, Func<TKey, TValue, TValueResult> valueSelector)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }

    /// <summary>
    /// Filter a collection with a lambda function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="array" />.</typeparam>
    /// <param name="kSqlFunctions"></param>
    /// <param name="array">The array.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>Filter the input collection through a given lambda function. The filtered collection is returned.</returns>
    public static TSource[] Filter<TSource>(this KSqlFunctions kSqlFunctions, TSource[] array, Func<TSource, bool> predicate)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }

    /// <summary>
    /// Filter a collection with a lambda function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="kSqlFunctions"></param>
    /// <param name="source">The enumerable source.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>Filter the input collection through a given lambda function. The filtered collection is returned.</returns>
    public static TSource[] Filter<TSource>(this KSqlFunctions kSqlFunctions, IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }

    /// <summary>
    /// Filter a collection with a lambda function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">The enumerable source.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>Filter the input collection through a given lambda function. The filtered collection is returned.</returns>
    public static TSource[] Filter<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }

    /// <summary>
    /// When filtering a map, the function provided must have a boolean result. For each map entry, the function
    /// will be applied to the key and value arguments in that order. The filtered map is returned.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="kSqlFunctions"></param>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>Filter the input collection through a given lambda function. The filtered collection is returned.</returns>
    public static IDictionary<TKey, TValue> Filter<TKey, TValue>(this KSqlFunctions kSqlFunctions, IDictionary<TKey, TValue> dictionary, Func<TKey, TValue, bool> predicate)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }

    /// <summary>
    /// When filtering a map, the function provided must have a boolean result. For each map entry, the function
    /// will be applied to the key and value arguments in that order. The filtered map is returned.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="kSqlFunctions"></param>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>Filter the input collection through a given lambda function. The filtered collection is returned.</returns>
    public static IDictionary<TKey, TValue> Filter<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<TKey, TValue, bool> predicate)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }

    /// <summary>
    /// Reduce a collection starting from an initial state.
    /// </summary>
    /// <param name="kSqlFunctions"></param>
    /// <param name="array">The array.</param>
    /// <param name="state">The initial state.</param>
    /// <param name="accumulator">The reduce function.</param>
    /// <returns>The accumulated state.</returns>
    public static TState Reduce<TSource, TState>(this KSqlFunctions kSqlFunctions, TSource[] array, TState state, Func<TState, TSource, TState> accumulator)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }

    /// <summary>
    /// Reduce a collection starting from an initial state.
    /// </summary>
    /// <param name="array">The array.</param>
    /// <param name="state">The initial state.</param>
    /// <param name="accumulator">The reduce function.</param>
    /// <returns>The accumulated state.</returns>
    public static TState Reduce<TSource, TState>(this IEnumerable<TSource> array, TState state, Func<TState, TSource, TState> accumulator)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }

    /// <summary>
    /// Reduce a collection starting from an initial state.
    /// </summary>
    /// <param name="kSqlFunctions"></param>
    /// <param name="source">The enumerable source.</param>
    /// <param name="state">The initial state.</param>
    /// <param name="accumulator">The reduce function.</param>
    /// <returns>The accumulated state.</returns>
    public static TState Reduce<TSource, TState>(this KSqlFunctions kSqlFunctions, IEnumerable<TSource> source, TState state, Func<TState, TSource, TState> accumulator)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }

    /// <summary>
    /// Reduce the input collection down to a single value using an initial state and a function. The initial
    /// state (s) is passed into the scope of the function. Each invocation returns a new value for s, which the
    /// next invocation will receive. The final value for s is returned.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <param name="kSqlFunctions"></param>
    /// <param name="dictionary">The dictionary</param>
    /// <param name="state">The initial state.</param>
    /// <param name="accumulator"></param>
    /// <returns>When reducing a map, the reduce function must have three arguments. The three arguments for the reduce function are in order: the state, the key, and the value. The final state is returned.</returns>
    public static TState Reduce<TKey, TSource, TState>(this KSqlFunctions kSqlFunctions, IDictionary<TKey, TSource> dictionary, TState state, Func<TState, TKey, TSource, TState> accumulator)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }

    /// <summary>
    /// Reduce the input collection down to a single value using an initial state and a function. The initial
    /// state (s) is passed into the scope of the function. Each invocation returns a new value for s, which the
    /// next invocation will receive. The final value for s is returned.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <param name="dictionary">The dictionary</param>
    /// <param name="state">The initial state.</param>
    /// <param name="accumulator"></param>
    /// <returns>When reducing a map, the reduce function must have three arguments. The three arguments for the reduce function are in order: the state, the key, and the value. The final state is returned.</returns>
    public static TState Reduce<TKey, TSource, TState>(this IDictionary<TKey, TSource> dictionary, TState state, Func<TState, TKey, TSource, TState> accumulator)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }
  }
}