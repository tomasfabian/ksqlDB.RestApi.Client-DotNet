using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Options;

public class ProcessingGuaranteeExtensionsTests
{
  [Test]
  public void ToProcessingGuarantee_UnknownValue()
  {
    //Arrange


    //Assert
    Assert.Throws<ArgumentOutOfRangeException>(() =>
    {
      //Act
      "xyz".ToProcessingGuarantee();
    });
  }

  [Test]
  public void ToProcessingGuarantee_ExactlyOnce()
  {
    //Arrange

    //Act
    var value = ProcessingGuaranteeExtensions.ExactlyOnce.ToProcessingGuarantee();

    //Assert
    value.Should().Be(ProcessingGuarantee.ExactlyOnce);
  }

  [Test]
  public void ToProcessingGuarantee_ExactlyOnceV2()
  {
    //Arrange

    //Act
    var value = ProcessingGuaranteeExtensions.ExactlyOnceV2.ToProcessingGuarantee();

    //Assert
    value.Should().Be(ProcessingGuarantee.ExactlyOnceV2);
  }
    
  [Test]
  public void ToProcessingGuarantee_AtLeastOnce()
  {
    //Arrange

    //Act
    var value = ProcessingGuaranteeExtensions.AtLeastOnce.ToProcessingGuarantee();

    //Assert
    value.Should().Be(ProcessingGuarantee.AtLeastOnce);
  }
    
  [Test]
  public void ToKSqlValue_AtLeastOnce()
  {
    //Arrange

    //Act
    var value = ProcessingGuarantee.AtLeastOnce.ToKSqlValue();

    //Assert
    value.Should().BeEquivalentTo("at_least_once");
  }
    
  [Test]
  public void ToKSqlValue_ExactlyOnce()
  {
    //Arrange

    //Act
    var value = ProcessingGuarantee.ExactlyOnce.ToKSqlValue();

    //Assert
    value.Should().BeEquivalentTo("exactly_once");
  }
}
