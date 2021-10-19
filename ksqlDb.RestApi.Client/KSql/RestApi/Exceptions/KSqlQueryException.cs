using System;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Exceptions
{
  public class KSqlQueryException : Exception
  {        
    public KSqlQueryException(string message)
      : base(message)
    {
    }

    public string Statement { get; set; }

    public int ErrorCode { get; set; }
  }
}