using System;

namespace ksqlDB.RestApi.Client.KSql.Query.Functions;

public static class KSQLConvert
{
  public static int ToInt32(string value)
  {
    throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
  }

  public static long ToInt64(string value)
  {
    throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
  }

  public static decimal ToDecimal(string value, short precision, short scale)
  {
    throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
  }

  public static double ToDouble(string value)
  {
    throw new InvalidOperationException(KSqlFunctionsExtensions.ServerSideOperationErrorMessage);
  }
}