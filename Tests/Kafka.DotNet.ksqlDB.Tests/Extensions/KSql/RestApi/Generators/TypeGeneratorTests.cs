using System;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Generators;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations;
using NUnit.Framework;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.RestApi.Generators
{
  public class TypeGeneratorTests
  {
    [Test]
    public void CreateType()
    {      
      //Arrange

      //Act
      string statement = new TypeGenerator().Print<Address>();

      //Assert
      statement.Should().Be($@"CREATE TYPE {nameof(Address).ToUpper()} AS STRUCT<Number INT, Street VARCHAR, City VARCHAR>;");
    }

    [Test]
    public void CreateType_NestedType()
    {      
      //Arrange

      //Act
      string statement = new TypeGenerator().Print<Person>();

      //Assert
      statement.Should().Be($@"CREATE TYPE {nameof(Person).ToUpper()} AS STRUCT<Name VARCHAR, Address ADDRESS>;");
    }

    [Test]
    public void CreateType_BytesType()
    {      
      //Arrange

      //Act
      string statement = new TypeGenerator().Print<Thumbnail>();

      //Assert
      statement.Should().Be(@$"CREATE TYPE {nameof(Thumbnail).ToUpper()} AS STRUCT<Image BYTES>;");
    }

    public record Address
    {
      public int Number { get; set; }
      public string Street { get; set; }
      public string City { get; set; }
    }

    public class Person
    {
      public string Name { get; set; }
      public Address Address { get; set; }
    }

    public struct Thumbnail
    {
      public byte[] Image { get; set; }
    }

    #region GenericType

    record DatabaseChangeObject<TEntity> : DatabaseChangeObject
    {
      public TEntity Before { get; set; }
      public TEntity After { get; set; }
    }

    record DatabaseChangeObject
    {
      public Source Source { get; set; }
      public string Op { get; set; }
      public long TsMs { get; set; }
      //public object Transaction { get; set; }

      public ChangeDataCaptureType OperationType => ChangeDataCaptureType.Created;
    }

    [Flags]
    enum ChangeDataCaptureType
    {
      Read,
      Created,
      Updated,
      Deleted
    }

    record IoTSensor
    {
      [Key]
      public string SensorId { get; set; }
      public int Value { get; set; }
    }

    public record Source
    {
      public string Version { get; set; }
      public string Connector { get; set; }
    }

    [Test]
    public void CreateType_GenericType()
    {      
      //Arrange

      //Act
      string statement = new TypeGenerator().Print<DatabaseChangeObject<IoTSensor>>();

      //Assert
      statement.Should().Be(@"CREATE TYPE DATABASECHANGEOBJECT AS STRUCT<Before IOTSENSOR, After IOTSENSOR, Source SOURCE, Op VARCHAR, TsMs BIGINT>;"); //, Transaction OBJECT
    }

    #endregion
  }
}