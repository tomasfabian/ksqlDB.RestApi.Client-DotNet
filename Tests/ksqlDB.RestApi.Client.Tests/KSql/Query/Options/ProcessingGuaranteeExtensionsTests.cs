using System;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Options;

[TestClass]
public class ProcessingGuaranteeExtensionsTests
{
  [TestMethod]
  [ExpectedException(typeof(ArgumentOutOfRangeException))]
  public void ToProcessingGuarantee_UnknownValue()
  {
    //Arrange

    //Act
    "xyz".ToProcessingGuarantee();

    //Assert
  }

  [TestMethod]
  public void ToProcessingGuarantee_ExactlyOnce()
  {
    //Arrange

    //Act
    var value = ProcessingGuaranteeExtensions.ExactlyOnce.ToProcessingGuarantee();

    //Assert
    value.Should().Be(ProcessingGuarantee.ExactlyOnce);
  }
    
  [TestMethod]
  public void ToProcessingGuarantee_AtLeastOnce()
  {
    //Arrange

    //Act
    var value = ProcessingGuaranteeExtensions.AtLeastOnce.ToProcessingGuarantee();

    //Assert
    value.Should().Be(ProcessingGuarantee.AtLeastOnce);
  }
    
  [TestMethod]
  public void ToKSqlValue_AtLeastOnce()
  {
    //Arrange

    //Act
    var value = ProcessingGuarantee.AtLeastOnce.ToKSqlValue();

    //Assert
    value.Should().BeEquivalentTo("at_least_once");
  }
    
  [TestMethod]
  public void ToKSqlValue_ExactlyOnce()
  {
    //Arrange

    //Act
    var value = ProcessingGuarantee.ExactlyOnce.ToKSqlValue();

    //Assert
    value.Should().BeEquivalentTo("exactly_once");
  }
}