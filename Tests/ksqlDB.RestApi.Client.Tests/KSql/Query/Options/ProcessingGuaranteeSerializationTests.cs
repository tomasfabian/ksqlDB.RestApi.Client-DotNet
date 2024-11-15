using System.Text.Json;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Options;

public class ProcessingGuaranteeSerializationTests
{
  [Test]
  public void ToProcessingGuarantee_UnknownValue()
  {
    //Arrange

    //Assert
    Assert.Throws<JsonException>(() =>
    {
      //Act
      JsonSerializer.Deserialize<ProcessingGuarantee>("\"xyz\"");
    });
  }

  [Test]
  public void ToProcessingGuarantee_ExactlyOnce()
  {
    //Arrange

    //Act
    var value = JsonSerializer.Deserialize<ProcessingGuarantee>("\"exactlyOnce\"");

    //Assert
    value.Should().Be(ProcessingGuarantee.ExactlyOnce);
  }

  [Test]
  public void ToProcessingGuarantee_ExactlyOnceV2()
  {
    //Arrange

    //Act
    var value = JsonSerializer.Deserialize<ProcessingGuarantee>("\"exactlyOnceV2\"");

    //Assert
    value.Should().Be(ProcessingGuarantee.ExactlyOnceV2);
  }

  [Test]
  public void ToProcessingGuarantee_AtLeastOnce()
  {
    //Arrange

    //Act
    var value = JsonSerializer.Deserialize<ProcessingGuarantee>("\"atLeastOnce\"");

    //Assert
    value.Should().Be(ProcessingGuarantee.AtLeastOnce);
  }

  [Test]
  public void ToKSqlValue_ExactlyOnce()
  {
    //Arrange

    //Act
    var value = JsonSerializer.Serialize(ProcessingGuarantee.ExactlyOnce);

    //Assert
    value.Should().Be("\"exactly_once\"");
  }

  [Test]
  public void ToKSqlValue_ExactlyOnceV2()
  {
    //Arrange

    //Act
    var value = JsonSerializer.Serialize(ProcessingGuarantee.ExactlyOnceV2);

    //Assert
    value.Should().Be("\"exactly_once_v2\"");
  }
}
