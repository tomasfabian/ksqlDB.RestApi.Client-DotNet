using System.Linq.Expressions;
using FluentAssertions;
using ksqlDb.RestApi.Client.Metadata;
using ksqlDb.RestApi.Client.Tests.FluentAPI.Builders;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.Metadata
{
  public class PropertyInfoExtractorTests
  {
    [Test]
    public void GetMembers()
    {
      //Arrange
      Expression<Func<Payment, decimal>> propertyExpression = c => c.Amount;

      //Act
      var memberInfos = propertyExpression.GetMembers().ToList();

      //Assert
      memberInfos.Count().Should().Be(1);
      memberInfos.First().Item1.Should().Be(nameof(Payment.Amount));
    }

    [Test]
    public void GetMemberInfo()
    {
      //Arrange
      Expression<Func<Payment, decimal>> propertyExpression = c => c.Amount;

      //Act
      var memberInfo = propertyExpression.GetMemberInfo();

      //Assert
      memberInfo.Name.Should().Be(nameof(Payment.Amount));
    }
  }
}
