using System;
using Kafka.DotNet.ksqlDB.KSql.Query.Functions;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Operators
{
  public static class KSqlOperatorExtensions
  {
    /// <summary>
    /// The BETWEEN operator is used to indicate that a certain value must be within a specified range, including boundaries.
    /// </summary>
    public static bool Between(this short expression, short startExpression, short endExpression)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }

    /// <summary>
    /// The BETWEEN operator is used to indicate that a certain value must be within a specified range, including boundaries.
    /// </summary>
    public static bool Between(this int expression, int startExpression, int endExpression)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }
    
    /// <summary>
    /// The BETWEEN operator is used to indicate that a certain value must be within a specified range, including boundaries.
    /// </summary>
    public static bool Between(this float expression, float startExpression, float endExpression)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }
    
    /// <summary>
    /// The BETWEEN operator is used to indicate that a certain value must be within a specified range, including boundaries.
    /// </summary>
    public static bool Between(this double expression, double startExpression, double endExpression)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }

    /// <summary>
    /// The BETWEEN operator is used to indicate that a certain value must be within a specified range, including boundaries.
    /// </summary>
    public static bool Between(this decimal expression, decimal startExpression, decimal endExpression)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }

    /// <summary>
    /// The BETWEEN operator is used to indicate that a certain value must be within a specified range, including boundaries.
    /// </summary>
    public static bool Between(this string expression, string startExpression, string endExpression)
    {
      throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
    }
  }
}