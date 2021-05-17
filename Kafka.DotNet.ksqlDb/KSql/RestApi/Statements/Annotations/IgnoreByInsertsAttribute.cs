using System;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations
{
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public sealed class IgnoreByInsertsAttribute : Attribute
  {	
  }
}