using System;
using ksqlDB.RestApi.Client.KSql.Query.Functions;

namespace ksqlDB.RestApi.Client.KSql.Query.Operators;

public static class KSqlOperatorExtensions
{
  #region Between

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
    
  /// <summary>
  /// The BETWEEN operator is used to indicate that a certain value must be within a specified range, including boundaries.
  /// </summary>
  public static bool Between(this TimeSpan expression, TimeSpan startExpression, TimeSpan endExpression)
  {
    throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
  }
    
  /// <summary>
  /// The BETWEEN operator is used to indicate that a certain value must be within a specified range, including boundaries.
  /// </summary>
  public static bool Between(this DateTime expression, DateTime startExpression, DateTime endExpression)
  {
    throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
  }
    
  /// <summary>
  /// The BETWEEN operator is used to indicate that a certain value must be within a specified range, including boundaries.
  /// </summary>
  public static bool Between(this DateTimeOffset expression, DateTimeOffset startExpression, DateTimeOffset endExpression)
  {
    throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
  }

  #endregion

  #region NotBetween

  /// <summary>
  /// The NOT BETWEEN operator is used to indicate that a certain value must not be within a specified range, including boundaries.
  /// </summary>
  public static bool NotBetween(this short expression, short startExpression, short endExpression)
  {
    throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
  }

  /// <summary>
  /// The NOT BETWEEN operator is used to indicate that a certain value must not be within a specified range, including boundaries.
  /// </summary>
  public static bool NotBetween(this int expression, int startExpression, int endExpression)
  {
    throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
  }
    
  /// <summary>
  /// The NOT BETWEEN operator is used to indicate that a certain value must not be within a specified range, including boundaries.
  /// </summary>
  public static bool NotBetween(this float expression, float startExpression, float endExpression)
  {
    throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
  }
    
  /// <summary>
  /// The NOT BETWEEN operator is used to indicate that a certain value must not be within a specified range, including boundaries.
  /// </summary>
  public static bool NotBetween(this double expression, double startExpression, double endExpression)
  {
    throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
  }

  /// <summary>
  /// The NOT BETWEEN operator is used to indicate that a certain value must not be within a specified range, including boundaries.
  /// </summary>
  public static bool NotBetween(this decimal expression, decimal startExpression, decimal endExpression)
  {
    throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
  }

  /// <summary>
  /// The NOT BETWEEN operator is used to indicate that a certain value must not be within a specified range, including boundaries.
  /// </summary>
  public static bool NotBetween(this string expression, string startExpression, string endExpression)
  {
    throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
  }

  /// <summary>
  /// The NOT BETWEEN operator is used to indicate that a certain value must not be within a specified range, including boundaries.
  /// </summary>
  public static bool NotBetween(this TimeSpan expression, TimeSpan startExpression, TimeSpan endExpression)
  {
    throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
  }

  /// <summary>
  /// The NOT BETWEEN operator is used to indicate that a certain value must not be within a specified range, including boundaries.
  /// </summary>
  public static bool NotBetween(this DateTime expression, DateTime startExpression, DateTime endExpression)
  {
    throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
  }

  /// <summary>
  /// The NOT BETWEEN operator is used to indicate that a certain value must not be within a specified range, including boundaries.
  /// </summary>
  public static bool NotBetween(this DateTimeOffset expression, DateTimeOffset startExpression, DateTimeOffset endExpression)
  {
    throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
  }

  #endregion
}