using System;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations
{
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public sealed class IgnoreByInsertsAttribute : Attribute
  {	
  }
}