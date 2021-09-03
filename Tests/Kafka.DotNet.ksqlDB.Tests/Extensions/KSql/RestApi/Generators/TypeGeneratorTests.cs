using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Generators;
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
      statement.Should().Be(@"CREATE TYPE ADDRESS AS STRUCT<Number INT, Street VARCHAR, City VARCHAR>;");
    }

    [Test]
    public void CreateType_NestedType()
    {      
      //Arrange

      //Act
      string statement = new TypeGenerator().Print<Person>();

      //Assert
      statement.Should().Be(@"CREATE TYPE PERSON AS STRUCT<Name VARCHAR, Address ADDRESS>;");
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
  }
}