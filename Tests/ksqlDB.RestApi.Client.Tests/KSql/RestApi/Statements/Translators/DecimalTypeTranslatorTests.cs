using System.Linq.Expressions;
using System.Reflection;
using FluentAssertions;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDb.RestApi.Client.FluentAPI.Builders.Configuration;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDb.RestApi.Client.KSql.RestApi.Statements.Translators;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Statements.Translators
{
  public class DecimalTypeTranslatorTests
  {
    private ModelBuilder modelBuilder = null!;
    private DecimalTypeTranslator decimalTypeTranslator = null!;

    [SetUp]
    public void Init()
    {
      modelBuilder = new();
      decimalTypeTranslator = new(modelBuilder);
    }

    private record Poco
    {
      public decimal Amount { get; set; }
    }

    [Test]
    public void TryGetDecimal()
    {
      //Arrange
      var parentType = typeof(Poco);
      modelBuilder.Entity<Poco>()
        .Property(c => c.Amount)
        .Decimal(10, 2);

      var propertyInfo = GetAmountPropertyInfo();

      //Act
      bool result = decimalTypeTranslator.TryGetDecimal(parentType, propertyInfo, out var @decimal);

      //Assert
      result.Should().BeTrue();
      @decimal.Should().BeEquivalentTo("(10,2)");
    }

    private static PropertyInfo GetAmountPropertyInfo()
    {
      Expression<Func<Poco, decimal>> memberExpression = c => c.Amount;
      MemberExpression memberExpr = (MemberExpression)memberExpression.Body;
      PropertyInfo propertyInfo = (PropertyInfo)memberExpr.Member;
      return propertyInfo;
    }

    [Test]
    public void TryGetDecimal_UseConvention()
    {
      //Arrange
      var parentType = typeof(Poco);
      modelBuilder.AddConvention(new DecimalTypeConvention(10, 3));
      var propertyInfo = GetAmountPropertyInfo();

      //Act
      bool result = decimalTypeTranslator.TryGetDecimal(parentType, propertyInfo, out var @decimal);

      //Assert
      result.Should().BeTrue();
      @decimal.Should().BeEquivalentTo("(10,3)");
    }

    private record Payment
    {
      [Decimal(10, 4)]
      public decimal Amount { get; set; }
    }

    [Test]
    public void TryGetDecimal_UseAttribute()
    {
      //Arrange
      var parentType = typeof(Payment);
      Expression<Func<Payment, decimal>> memberExpression = c => c.Amount;
      MemberExpression memberExpr = (MemberExpression)memberExpression.Body;
      PropertyInfo propertyInfo = (PropertyInfo)memberExpr.Member;

      //Act
      bool result = decimalTypeTranslator.TryGetDecimal(parentType, propertyInfo, out var @decimal);

      //Assert
      result.Should().BeTrue();
      @decimal.Should().BeEquivalentTo("(10,4)");
    }
  }
}
