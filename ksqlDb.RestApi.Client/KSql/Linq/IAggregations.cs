using System;
using System.Collections.Generic;

namespace ksqlDB.RestApi.Client.KSql.Linq
{
  public interface IAggregations
  {     
    /// <summary>
    /// The count returned will be the total number of rows.
    /// </summary>
    /// <returns></returns>
    int Count(); 
    /// <summary>
    /// The count returned will be the total number of rows.
    /// </summary>
    /// <returns></returns>
    long LongCount();
  }

  public interface IAggregations<out TSource> : IAggregations
  {
    #region Avg

    /// <summary>
    /// Returns the average value of the column computed as the sum divided by the count. Applicable only to numeric types.
    /// </summary>
    int Avg(Func<TSource, int?> selector);

    /// <summary>
    /// Returns the average value of the column computed as the sum divided by the count. Applicable only to numeric types.
    /// </summary>
    long Avg(Func<TSource, long?> selector);

    /// <summary>
    /// Returns the average value of the column computed as the sum divided by the count. Applicable only to numeric types.
    /// </summary>
    decimal Avg(Func<TSource, decimal?> selector);

    /// <summary>
    /// Returns the average value of the column computed as the sum divided by the count. Applicable only to numeric types.
    /// </summary>
    decimal Avg(Func<TSource, float?> selector);

    /// <summary>
    /// Returns the average value of the column computed as the sum divided by the count. Applicable only to numeric types.
    /// </summary>
    decimal Avg(Func<TSource, double?> selector);

    /// <summary>
    /// Returns the average value of the column computed as the sum divided by the count. Applicable only to numeric types.
    /// </summary>
    /// <returns>Computed average of column with type int</returns>
    int Avg(Func<TSource, int> selector);

    /// <summary>
    /// Returns the average value of the column computed as the sum divided by the count. Applicable only to numeric types.
    /// </summary>
    /// <returns>Computed average of column with type long</returns>
    long Avg(Func<TSource, long> selector);

    /// <summary>
    /// Returns the average value of the column computed as the sum divided by the count. Applicable only to numeric types.
    /// </summary>
    decimal Avg(Func<TSource, float> selector);

    /// <summary>
    /// Returns the average value of the column computed as the sum divided by the count. Applicable only to numeric types.
    /// </summary>
    /// <returns>Computed average of column with type double</returns>
    decimal Avg(Func<TSource, double> selector);

    /// <summary>
    /// Returns the average value of the column computed as the sum divided by the count. Applicable only to numeric types.
    /// </summary>
    decimal Avg(Func<TSource, decimal> selector);
    

    #endregion

    #region Count

    /// <summary>
    /// Count the number of rows. When col1 is specified, the count returned will be the number of rows where col1 is non-null.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    int Count(Func<TSource, object> selector);
    /// <summary>
    /// Count the number of rows. When col1 is specified, the count returned will be the number of rows where col1 is non-null.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    long LongCount(Func<TSource, object> selector);
    
    #endregion

    #region CountDistinct

    /// <summary>
    /// Returns the approximate number of unique values of col1 in a group.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    int CountDistinct(Func<TSource, object> selector);
    /// <summary>
    /// Returns the approximate number of unique values of col1 in a group.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    long LongCountDistinct(Func<TSource, object> selector);

    #endregion

    #region CollectList

    /// <summary>
    /// Gather all of the values from an input grouping into a single Array field.
    /// Although this aggregate works on both Stream and Table inputs, the order of entries in the result array is
    /// not guaranteed when working on Table input data.
    /// This version limits the size of the resultant Array to 1000 entries, beyond which any further values will
    /// be silently ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns>collect values of a Boolean field into a single Array</returns>
    bool[] CollectList(Func<TSource, bool> selector);
    
    /// <summary>
    /// Gather all of the values from an input grouping into a single Array field.
    /// Although this aggregate works on both Stream and Table inputs, the order of entries in the result array is
    /// not guaranteed when working on Table input data.
    /// This version limits the size of the resultant Array to 1000 entries, beyond which any further values will
    /// be silently ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns>collect values of a String/Varchar field into a single Array</returns>
    string[] CollectList(Func<TSource, string> selector);

    /// <summary>
    /// Gather all of the values from an input grouping into a single Array field.
    /// Although this aggregate works on both Stream and Table inputs, the order of entries in the result array is
    /// not guaranteed when working on Table input data.
    /// This version limits the size of the resultant Array to 1000 entries, beyond which any further values will
    /// be silently ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns>collect values of a Int field into a single Array</returns>
    int[] CollectList(Func<TSource, int> selector);

    /// <summary>
    /// Gather all of the values from an input grouping into a single Array field.
    /// Although this aggregate works on both Stream and Table inputs, the order of entries in the result array is
    /// not guaranteed when working on Table input data.
    /// This version limits the size of the resultant Array to 1000 entries, beyond which any further values will
    /// be silently ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns>collect values of a BigInt field into a single Array</returns>int[] CollectList(Func<TSource, int> selector);
    long[] CollectList(Func<TSource, long> selector);

    /// <summary>
    /// Gather all of the values from an input grouping into a single Array field.
    /// Although this aggregate works on both Stream and Table inputs, the order of entries in the result array is
    /// not guaranteed when working on Table input data.
    /// This version limits the size of the resultant Array to 1000 entries, beyond which any further values will
    /// be silently ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns>collect values of a Double field into a single Array</returns>float[] CollectList(Func<TSource, float> selector);
    double[] CollectList(Func<TSource, double> selector);

    [Obsolete]
    decimal[] CollectList(Func<TSource, decimal> selector);

    #endregion

    #region CollectSet

    /// <summary>
    /// Gather all of the distinct values from an input grouping into a single Array.
    /// Not available for aggregating values from an input Table.
    /// This version limits the size of the resultant Array to 1000 entries, beyond which any further values will
    /// silently ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    bool[] CollectSet(Func<TSource, bool> selector);

    /// <summary>
    /// Gather all of the distinct values from an input grouping into a single Array.
    /// Not available for aggregating values from an input Table.
    /// This version limits the size of the resultant Array to 1000 entries, beyond which any further values will
    /// silently ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    string[] CollectSet(Func<TSource, string> selector);
    /// <summary>
    /// Gather all of the distinct values from an input grouping into a single Array.
    /// Not available for aggregating values from an input Table.
    /// This version limits the size of the resultant Array to 1000 entries, beyond which any further values will
    /// silently ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    short[] CollectSet(Func<TSource, short> selector);
    /// <summary>
    /// Gather all of the distinct values from an input grouping into a single Array.
    /// Not available for aggregating values from an input Table.
    /// This version limits the size of the resultant Array to 1000 entries, beyond which any further values will
    /// silently ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    int[] CollectSet(Func<TSource, int> selector);
    /// <summary>
    /// Gather all of the distinct values from an input grouping into a single Array.
    /// Not available for aggregating values from an input Table.
    /// This version limits the size of the resultant Array to 1000 entries, beyond which any further values will
    /// silently ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    long[] CollectSet(Func<TSource, long> selector);
    /// <summary>
    /// Gather all of the distinct values from an input grouping into a single Array.
    /// Not available for aggregating values from an input Table.
    /// This version limits the size of the resultant Array to 1000 entries, beyond which any further values will
    /// silently ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    float[] CollectSet(Func<TSource, float> selector);
    /// <summary>
    /// Gather all of the distinct values from an input grouping into a single Array.
    /// Not available for aggregating values from an input Table.
    /// This version limits the size of the resultant Array to 1000 entries, beyond which any further values will
    /// silently ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    double[] CollectSet(Func<TSource, double> selector);
    /// <summary>
    /// Gather all of the distinct values from an input grouping into a single Array.
    /// Not available for aggregating values from an input Table.
    /// This version limits the size of the resultant Array to 1000 entries, beyond which any further values will
    /// silently ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    decimal[] CollectSet(Func<TSource, decimal> selector);

    #endregion

    #region Min

    /// <summary>
    /// Computes the minimum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    int Min(Func<TSource, int?> selector);
    /// <summary>
    /// Computes the minimum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    long Min(Func<TSource, long?> selector);
    /// <summary>
    /// Computes the minimum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    decimal Min(Func<TSource, decimal?> selector);
    /// <summary>
    /// Computes the minimum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    decimal Min(Func<TSource, float?> selector);
    /// <summary>
    /// Computes the minimum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    decimal Min(Func<TSource, double?> selector);
    /// <summary>
    /// Computes the minimum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    int Min(Func<TSource, int> selector);
    /// <summary>
    /// Computes the minimum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    long Min(Func<TSource, long> selector);
    /// <summary>
    /// Computes the minimum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    decimal Min(Func<TSource, float> selector);
    /// <summary>
    /// Computes the minimum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    decimal Min(Func<TSource, double> selector);
    /// <summary>
    /// Computes the minimum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    decimal Min(Func<TSource, decimal> selector);
    
    #endregion

    #region Max

    /// <summary>
    /// Computes the maximum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    int Max(Func<TSource, int?> selector);
    /// <summary>
    /// Computes the maximum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    long Max(Func<TSource, long?> selector);
    /// <summary>
    /// Computes the maximum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    decimal Max(Func<TSource, decimal?> selector);
    /// <summary>
    /// Computes the maximum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    decimal Max(Func<TSource, float?> selector);
    /// <summary>
    /// Computes the maximum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    decimal Max(Func<TSource, double?> selector);
    /// <summary>
    /// Computes the maximum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    int Max(Func<TSource, int> selector);
    /// <summary>
    /// Computes the maximum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    long Max(Func<TSource, long> selector);
    /// <summary>
    /// Computes the maximum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    decimal Max(Func<TSource, float> selector);
    /// <summary>
    /// Computes the maximum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    decimal Max(Func<TSource, double> selector);
    /// <summary>
    /// Computes the maximum integer value for a key.
    /// </summary>
    /// <param name="selector">the value to aggregate</param>
    /// <returns></returns>
    decimal Max(Func<TSource, decimal> selector);
    
    #endregion

    #region Histogram

    /// <summary>
    /// Build a value-to-count histogram of input Strings.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns>Returns a map of each distinct String from the input Stream or Table and how many times each occurs.
    /// This version limits the size of the resultant Map to 1000 entries. Any entries added beyond this limit
    /// will be ignored.</returns>
    IDictionary<string, long> Histogram(Func<TSource, string> selector);
    
    #endregion

    #region Sum

    /// <summary>
    /// Computes the sum for a key.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    int Sum(Func<TSource, int?> selector);
    /// <summary>
    /// Computes the sum for a key.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    long Sum(Func<TSource, long?> selector);
    /// <summary>
    /// Computes the sum for a key.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    decimal Sum(Func<TSource, decimal?> selector);
    /// <summary>
    /// Computes the sum for a key.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    decimal Sum(Func<TSource, float?> selector);
    /// <summary>
    /// Computes the sum for a key.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    decimal Sum(Func<TSource, double?> selector);
    /// <summary>
    /// Computes the sum for a key.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    int Sum(Func<TSource, int> selector);
    /// <summary>
    /// Computes the sum for a key.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    long Sum(Func<TSource, long> selector);
    /// <summary>
    /// Computes the sum for a key.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    decimal Sum(Func<TSource, float> selector);
    /// <summary>
    /// Computes the sum for a key.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    decimal Sum(Func<TSource, double> selector);
    /// <summary>
    /// Computes the sum of decimal values for a key, resulting in a decimal with the same precision and scale.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    decimal Sum(Func<TSource, decimal> selector);

    #endregion

    #region TopK

    /// <summary>
    /// Calculates the TopK value for a column, per key.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="k"></param>
    /// <returns>Return the Top K values for the given column and window Rows that have col1 set to null are ignored.</returns>
    short[] TopK(Func<TSource, short> selector, int k);
    /// <summary>
    /// Calculates the TopK value for a column, per key.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="k"></param>
    /// <returns>Return the Top K values for the given column and window Rows that have col1 set to null are ignored.</returns>
    int[] TopK(Func<TSource, int> selector, int k);
    /// <summary>
    /// Calculates the TopK value for a column, per key.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="k"></param>
    /// <returns>Return the Top K values for the given column and window Rows that have col1 set to null are ignored.</returns>
    long[] TopK(Func<TSource, long> selector, int k);
    /// <summary>
    /// Calculates the TopK value for a column, per key.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="k"></param>
    /// <returns>Return the Top K values for the given column and window Rows that have col1 set to null are ignored.</returns>
    float[] TopK(Func<TSource, float> selector, int k);
    /// <summary>
    /// Calculates the TopK value for a column, per key.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="k"></param>
    /// <returns>Return the Top K values for the given column and window Rows that have col1 set to null are ignored.</returns>
    double[] TopK(Func<TSource, double> selector, int k);
    /// <summary>
    /// Calculates the TopK value for a column, per key.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="k"></param>
    /// <returns>Return the Top K values for the given column and window Rows that have col1 set to null are ignored.</returns>
    decimal[] TopK(Func<TSource, decimal> selector, int k);

    #endregion

    #region TopKDistinct

    /// <summary>
    /// Calculates the Topk distinct values for a column, per key.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="k"></param>
    /// <returns>Return the distinct Top K values for the given column and window Rows that have col1 set to null are ignored.</returns>
    short[] TopKDistinct(Func<TSource, short> selector, int k);

    /// <summary>
    /// Calculates the Topk distinct values for a column, per key.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="k"></param>
    /// <returns>Return the distinct Top K values for the given column and window Rows that have col1 set to null are ignored.</returns>
    int[] TopKDistinct(Func<TSource, int> selector, int k);

    /// <summary>
    /// Calculates the Topk distinct values for a column, per key.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="k"></param>
    /// <returns>Return the distinct Top K values for the given column and window Rows that have col1 set to null are ignored.</returns>
    long[] TopKDistinct(Func<TSource, long> selector, int k);

    /// <summary>
    /// Calculates the Topk distinct values for a column, per key.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="k"></param>
    /// <returns>Return the distinct Top K values for the given column and window Rows that have col1 set to null are ignored.</returns>
    float[] TopKDistinct(Func<TSource, float> selector, int k);

    /// <summary>
    /// Calculates the Topk distinct values for a column, per key.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="k"></param>
    /// <returns>Return the distinct Top K values for the given column and window Rows that have col1 set to null are ignored.</returns>
    double[] TopKDistinct(Func<TSource, double> selector, int k);

    /// <summary>
    /// Calculates the Topk distinct values for a column, per key.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="k"></param>
    /// <returns>Return the distinct Top K values for the given column and window Rows that have col1 set to null are ignored.</returns>
    decimal[] TopKDistinct(Func<TSource, decimal> selector, int k);

    #endregion

    #region EarliestByOffset

    /// <summary>
    /// Return the earliest value for the specified column. The earliest value in the partition. Null values are ignored.
    /// has the lowest offset.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    string EarliestByOffset(Func<TSource, string> selector);
    /// <summary>
    /// Return the earliest value for the specified column. The earliest value in the partition. Null values are ignored.
    /// has the lowest offset.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    int EarliestByOffset(Func<TSource, int> selector);
    /// <summary>
    /// Return the earliest value for the specified column. The earliest value in the partition. Null values are ignored.
    /// has the lowest offset.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    long EarliestByOffset(Func<TSource, long> selector);
    /// <summary>
    /// Return the earliest value for the specified column. The earliest value in the partition. Null values are ignored.
    /// has the lowest offset.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    float EarliestByOffset(Func<TSource, float> selector);
    /// <summary>
    /// Return the earliest value for the specified column. The earliest value in the partition. Null values are ignored.
    /// has the lowest offset.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    double EarliestByOffset(Func<TSource, double> selector);
    /// <summary>
    /// Return the earliest value for the specified column. The earliest value in the partition. Null values are ignored.
    /// has the lowest offset.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    decimal EarliestByOffset(Func<TSource, decimal> selector);

    #endregion

    #region EarliestByOffsetAllowNulls

    /// <summary>
    /// Return the earliest value for the specified column. The earliest value in the partition. Includes NULL values.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    string EarliestByOffsetAllowNulls(Func<TSource, string> selector);
    /// <summary>
    /// Return the earliest value for the specified column. The earliest value in the partition. Includes NULL values.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    int? EarliestByOffsetAllowNulls(Func<TSource, int?> selector);
    /// <summary>
    /// Return the earliest value for the specified column. The earliest value in the partition. Includes NULL values.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    long? EarliestByOffsetAllowNulls(Func<TSource, long?> selector);
    /// <summary>
    /// Return the earliest value for the specified column. The earliest value in the partition. Includes NULL values.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    float? EarliestByOffsetAllowNulls(Func<TSource, float?> selector);
    /// <summary>
    /// Return the earliest value for the specified column. The earliest value in the partition. Includes NULL values.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    double? EarliestByOffsetAllowNulls(Func<TSource, double?> selector);
    /// <summary>
    /// Return the earliest value for the specified column. The earliest value in the partition. Includes NULL values.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    decimal? EarliestByOffsetAllowNulls(Func<TSource, decimal?> selector);

    #endregion

    #region EarliestByOffset

    /// <summary>
    /// Return the earliest N values for the specified column as an ARRAY. The earliest values in the partition have the lowest offsets. Null values are ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="earliestN"></param>
    /// <returns></returns>
    string[] EarliestByOffset(Func<TSource, string> selector, int earliestN);
    /// <summary>
    /// Return the earliest N values for the specified column as an ARRAY. The earliest values in the partition have the lowest offsets. Null values are ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="earliestN"></param>
    /// <returns></returns>
    int[] EarliestByOffset(Func<TSource, int> selector, int earliestN);
    /// <summary>
    /// Return the earliest N values for the specified column as an ARRAY. The earliest values in the partition have the lowest offsets. Null values are ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="earliestN"></param>
    /// <returns></returns>
    long[] EarliestByOffset(Func<TSource, long> selector, int earliestN);
    /// <summary>
    /// Return the earliest N values for the specified column as an ARRAY. The earliest values in the partition have the lowest offsets. Null values are ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="earliestN"></param>
    /// <returns></returns>
    float[] EarliestByOffset(Func<TSource, float> selector, int earliestN);
    /// <summary>
    /// Return the earliest N values for the specified column as an ARRAY. The earliest values in the partition have the lowest offsets. Null values are ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="earliestN"></param>
    /// <returns></returns>
    double[] EarliestByOffset(Func<TSource, double> selector, int earliestN);
    /// <summary>
    /// Return the earliest N values for the specified column as an ARRAY. The earliest values in the partition have the lowest offsets. Null values are ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="earliestN"></param>
    /// <returns></returns>
    decimal[] EarliestByOffset(Func<TSource, decimal> selector, int earliestN);

    #endregion

    #region EarliestByOffsetAllowNulls

    /// <summary>
    /// Return the earliest N values for the specified column as an ARRAY. The earliest values in the partition have the lowest offsets. Includes NULL values.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="earliestN"></param>
    /// <returns></returns>
    string[] EarliestByOffsetAllowNulls(Func<TSource, string> selector, int earliestN);
    /// <summary>
    /// Return the earliest N values for the specified column as an ARRAY. The earliest values in the partition have the lowest offsets. Includes NULL values.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="earliestN"></param>
    /// <returns></returns>
    int?[] EarliestByOffsetAllowNulls(Func<TSource, int?> selector, int earliestN);
    /// <summary>
    /// Return the earliest N values for the specified column as an ARRAY. The earliest values in the partition have the lowest offsets. Includes NULL values.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="earliestN"></param>
    /// <returns></returns>
    long?[] EarliestByOffsetAllowNulls(Func<TSource, long?> selector, int earliestN);
    /// <summary>
    /// Return the earliest N values for the specified column as an ARRAY. The earliest values in the partition have the lowest offsets. Includes NULL values.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="earliestN"></param>
    /// <returns></returns>
    float?[] EarliestByOffsetAllowNulls(Func<TSource, float?> selector, int earliestN);
    /// <summary>
    /// Return the earliest N values for the specified column as an ARRAY. The earliest values in the partition have the lowest offsets. Includes NULL values.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="earliestN"></param>
    /// <returns></returns>
    double?[] EarliestByOffsetAllowNulls(Func<TSource, double?> selector, int earliestN);
    /// <summary>
    /// Return the earliest N values for the specified column as an ARRAY. The earliest values in the partition have the lowest offsets. Includes NULL values.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="earliestN"></param>
    /// <returns></returns>
    decimal?[] EarliestByOffsetAllowNulls(Func<TSource, decimal?> selector, int earliestN);

    #endregion

    #region LatestByOffset

    /// <summary>
    /// Return the latest value for the specified column. The latest value in the partition. Null values are ignored.
    /// has the largest offset.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    string LatestByOffset(Func<TSource, string> selector);
    /// <summary>
    /// Return the latest value for the specified column. The latest value in the partition. Null values are ignored.
    /// has the largest offset.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    int LatestByOffset(Func<TSource, int> selector);
    /// <summary>
    /// Return the latest value for the specified column. The latest value in the partition. Null values are ignored.
    /// has the largest offset.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    long LatestByOffset(Func<TSource, long> selector);
    /// <summary>
    /// Return the latest value for the specified column. The latest value in the partition. Null values are ignored.
    /// has the largest offset.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    float LatestByOffset(Func<TSource, float> selector);
    /// <summary>
    /// Return the latest value for the specified column. The latest value in the partition. Null values are ignored.
    /// has the largest offset.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    double LatestByOffset(Func<TSource, double> selector);
    /// <summary>
    /// Return the latest value for the specified column. The latest value in the partition. Null values are ignored.
    /// has the largest offset.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    decimal LatestByOffset(Func<TSource, decimal> selector);

    #endregion

    #region LatestByOffsetAllowNulls

    /// <summary>
    /// Return the latest value for the specified column. The latest value in the partition. Includes NULL values.
    /// has the largest offset.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    string LatestByOffsetAllowNulls(Func<TSource, string> selector);
        
    /// <summary>
    /// Return the latest value for the specified column. The latest value in the partition. Includes NULL values.
    /// has the largest offset.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    int? LatestByOffsetAllowNulls(Func<TSource, int?> selector);    
    /// <summary>
    /// Return the latest value for the specified column. The latest value in the partition. Includes NULL values.
    /// has the largest offset.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    long? LatestByOffsetAllowNulls(Func<TSource, long?> selector);    
    /// <summary>
    /// Return the latest value for the specified column. The latest value in the partition. Includes NULL values.
    /// has the largest offset.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    float? LatestByOffsetAllowNulls(Func<TSource, float?> selector);    
    /// <summary>
    /// Return the latest value for the specified column. The latest value in the partition. Includes NULL values.
    /// has the largest offset.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    double? LatestByOffsetAllowNulls(Func<TSource, double?> selector);    
    /// <summary>
    /// Return the latest value for the specified column. The latest value in the partition. Includes NULL values.
    /// has the largest offset.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    decimal? LatestByOffsetAllowNulls(Func<TSource, decimal?> selector);

    #endregion

    #region LatestByOffset

    /// <summary>
    /// Returns the latest N values for the specified column as an ARRAY. The latest values have the largest offset. Null values are ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="latestN"></param>
    /// <returns></returns>
    string[] LatestByOffset(Func<TSource, string> selector, int latestN);
    /// <summary>
    /// Returns the latest N values for the specified column as an ARRAY. The latest values have the largest offset. Null values are ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="latestN"></param>
    /// <returns></returns>
    int[] LatestByOffset(Func<TSource, int> selector, int latestN);
    /// <summary>
    /// Returns the latest N values for the specified column as an ARRAY. The latest values have the largest offset. Null values are ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="latestN"></param>
    /// <returns></returns>
    long[] LatestByOffset(Func<TSource, long> selector, int latestN);
    /// <summary>
    /// Returns the latest N values for the specified column as an ARRAY. The latest values have the largest offset. Null values are ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="latestN"></param>
    /// <returns></returns>
    float[] LatestByOffset(Func<TSource, float> selector, int latestN);
    /// <summary>
    /// Returns the latest N values for the specified column as an ARRAY. The latest values have the largest offset. Null values are ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="latestN"></param>
    /// <returns></returns>
    double[] LatestByOffset(Func<TSource, double> selector, int latestN);
    /// <summary>
    /// Returns the latest N values for the specified column as an ARRAY. The latest values have the largest offset. Null values are ignored.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="latestN"></param>
    /// <returns></returns>
    decimal[] LatestByOffset(Func<TSource, decimal> selector, int latestN);

    #endregion

    #region LatestByOffsetAllowNulls
    
    /// <summary>
    /// Returns the latest N values for the specified column as an ARRAY. The latest values have the largest offset. Includes NULL values.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="latestN"></param>
    /// <returns></returns>
    string[] LatestByOffsetAllowNulls(Func<TSource, string> selector, int latestN);    
    /// <summary>
    /// Returns the latest N values for the specified column as an ARRAY. The latest values have the largest offset. Includes NULL values.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="latestN"></param>
    /// <returns></returns>
    int?[] LatestByOffsetAllowNulls(Func<TSource, int?> selector, int latestN);    
    /// <summary>
    /// Returns the latest N values for the specified column as an ARRAY. The latest values have the largest offset. Includes NULL values.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="latestN"></param>
    /// <returns></returns>
    long?[] LatestByOffsetAllowNulls(Func<TSource, long?> selector, int latestN);    
    /// <summary>
    /// Returns the latest N values for the specified column as an ARRAY. The latest values have the largest offset. Includes NULL values.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="latestN"></param>
    /// <returns></returns>
    float?[] LatestByOffsetAllowNulls(Func<TSource, float?> selector, int latestN);    
    /// <summary>
    /// Returns the latest N values for the specified column as an ARRAY. The latest values have the largest offset. Includes NULL values.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="latestN"></param>
    /// <returns></returns>
    double?[] LatestByOffsetAllowNulls(Func<TSource, double?> selector, int latestN);    
    /// <summary>
    /// Returns the latest N values for the specified column as an ARRAY. The latest values have the largest offset. Includes NULL values.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="latestN"></param>
    /// <returns></returns>
    decimal?[] LatestByOffsetAllowNulls(Func<TSource, decimal?> selector, int latestN);

    #endregion
  }
}