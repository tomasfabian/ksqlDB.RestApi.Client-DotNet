using NUnit.Framework;
using FluentAssertions;
using ksqlDb.RestApi.Client.KSql.RestApi.Parsers;
using ksqlDB.RestApi.Client.KSql.RestApi.Validation;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Validation
{
  public class PseudoColumnNameValidatorTests
  {
    private PseudoColumnValidator validator = null!;

    [SetUp]
    public void TestInitialize()
    {
      validator = new();
    }

    [TestCase("Headers")]
    [TestCase(nameof(SystemColumns.ROWTIME))]
    [TestCase(nameof(SystemColumns.ROWOFFSET))]
    [TestCase(nameof(SystemColumns.ROWPARTITION))]
    public void IsValid_ForPseudoColumns_ReturnsTrue(string columnName)
    {
      //Arrange

      //Act
      var isValid = validator.IsValid(columnName);

      //Assert
      isValid.Should().BeTrue();
    }

    [Test]
    public void IsValid_ForNonPseudoColumns_ReturnsFalse()
    {
      //Arrange

      //Act
      var isValid = validator.IsValid("foo");

      //Assert
      isValid.Should().BeFalse();
    }
  }
}
