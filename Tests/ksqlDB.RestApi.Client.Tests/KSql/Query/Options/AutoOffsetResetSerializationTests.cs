using System.Text.Json;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Options;

public class AutoOffsetResetExtensionsTests
{
  [Test]
  public void ToAutoOffsetReset_UnknownValue()
  {
    //Arrange

    //Assert
    Assert.Throws<JsonException>(() =>
    {
      //Act
      JsonSerializer.Deserialize<AutoOffsetReset>("\"xyz\"");
    });
  }

  [Test]
  public void ToAutoOffsetReset_Earliest()
  {
    //Arrange

    //Act
    var value = JsonSerializer.Deserialize<AutoOffsetReset>("\"earliest\"");

    //Assert
    value.Should().Be(AutoOffsetReset.Earliest);
  }

  [Test]
  public void ToAutoOffsetReset_Latest()
  {
    //Arrange

    //Act
    var value = JsonSerializer.Deserialize<AutoOffsetReset>("\"latest\"");

    //Assert
    value.Should().Be(AutoOffsetReset.Latest);
  }

  [Test]
  public void ToKSqlValue_Earliest()
  {
    //Arrange

    //Act
    var value = JsonSerializer.Serialize(AutoOffsetReset.Earliest);

    //Assert
    value.Should().Be("\"earliest\"");
  }

  [Test]
  public void ToKSqlValue_Latest()
  {
    //Arrange

    //Act
    var value = JsonSerializer.Serialize(AutoOffsetReset.Latest);

    //Assert
    value.Should().Be("\"latest\"");
  }
}
