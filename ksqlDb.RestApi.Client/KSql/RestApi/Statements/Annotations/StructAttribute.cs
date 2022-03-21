using System;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
  public sealed class StructAttribute : Attribute
  {
  }
}