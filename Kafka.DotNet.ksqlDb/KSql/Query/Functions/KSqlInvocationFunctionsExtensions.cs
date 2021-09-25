using System;
using System.Collections.Generic;
using System.Linq;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Functions
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
    /// <typeparam name="TSource">The type of the elements of <paramref name="array" />.</typeparam>
    /// <param name="kSqlFunctions"></param>
    /// <param name="source">The enumerable source.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>Filter the input collection through a given lambda function. The filtered collection is returned.</returns>
    public static TSource[] Filter<TSource>(this KSqlFunctions kSqlFunctions, IEnumerable<TSource> source, Func<TSource, bool> predicate)
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
    public static TResult Reduce<TSource, TResult>(this KSqlFunctions kSqlFunctions, TSource[] array, TResult state, Func<TSource, TResult, TResult> accumulator)
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
    public static TResult Reduce<TSource, TResult>(this KSqlFunctions kSqlFunctions, IEnumerable<TSource> source, TResult state, Func<TSource, TResult, TResult> accumulator)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }
  }
}