using System;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations
{
  [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
  public sealed class DecimalAttribute : Attribute
  {
    public DecimalAttribute(byte precision, byte scale)
    {
      Precision = precision;
      Scale = scale;
    }

    public byte Precision { get; set; }
    public byte Scale { get; set; }
  }
}