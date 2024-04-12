using System.Linq.Expressions;
using FluentAssertions;
using ksqlDb.RestApi.Client.Metadata;
using ksqlDb.RestApi.Client.Tests.FluentAPI.Builders;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.Metadata
{
  public class MemberInfoExtractorTests
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
    public void GetPropertyMemberInfo()
    {
      //Arrange
      Expression<Func<Payment, decimal>> propertyExpression = c => c.Amount;

      //Act
      var memberInfo = propertyExpression.GetMemberInfo();

      //Assert
      memberInfo.Name.Should().Be(nameof(Payment.Amount));
    }

    internal record PaymentWithFields
    {
      public string Id = null!;
      public decimal Amount;
    }

    [Test]
    public void GetFieldMembers()
    {
      //Arrange
      Expression<Func<PaymentWithFields, decimal>> propertyExpression = c => c.Amount;

      //Act
      var memberInfos = propertyExpression.GetMembers().ToList();

      //Assert
      memberInfos.Count().Should().Be(1);
      memberInfos.First().Item1.Should().Be(nameof(PaymentWithFields.Amount));
    }

    [Test]
    public void GetFieldMemberInfo()
    {
      //Arrange
      Expression<Func<PaymentWithFields, decimal>> propertyExpression = c => c.Amount;

      //Act
      var memberInfo = propertyExpression.GetMemberInfo();

      //Assert
      memberInfo.Name.Should().Be(nameof(PaymentWithFields.Amount));
    }
  }
}
