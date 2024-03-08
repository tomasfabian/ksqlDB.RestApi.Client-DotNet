using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using ksqlDb.RestApi.Client.KSql.RestApi.Statements.Providers;
using NUnit.Framework;
using static ksqlDB.RestApi.Client.KSql.RestApi.Enums.IdentifierEscaping;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Statements.Providers
{
  public class EntityProviderTests
  {
    private readonly EntityProvider entityProvider = new();

    private record TestType;

    internal static IEnumerable<(IdentifierEscaping, string)> GetEntityNameTestCases()
    {
      yield return (Never, nameof(TestType));
      yield return (Keywords, nameof(TestType));
      yield return (Always, $"`{nameof(TestType)}`");
    }

    [TestCaseSource(nameof(GetEntityNameTestCases))]
    public void GetName_ShouldNotPluralizeEntityName((IdentifierEscaping escaping, string expected) testCase)
    {
      //Arrange
      var (escaping, expected) = testCase;
      var properties = new EntityProperties
      {
        ShouldPluralizeEntityName = false,
        IdentifierEscaping = escaping
      };

      //Act
      var entityName = entityProvider.GetName<TestType>(properties);

      //Assert
      entityName.Should().Be(expected);
    }

    internal static IEnumerable<(IdentifierEscaping, string)> GetPluralizedEntityNameTestCases()
    {
      yield return (Never, $"{nameof(TestType)}s");
      yield return (Keywords, $"{nameof(TestType)}s");
      yield return (Always, $"`{nameof(TestType)}s`");
    }

    [TestCaseSource(nameof(GetPluralizedEntityNameTestCases))]
    public void GetName_ShouldPluralizeEntityName((IdentifierEscaping escaping, string expected) testCase)
    {
      //Arrange
      var (escaping, expected) = testCase;
      var properties = new EntityProperties
      {
        ShouldPluralizeEntityName = true,
        IdentifierEscaping = escaping
      };

      //Act
      var entityName = entityProvider.GetName<TestType>(properties);

      //Assert
      entityName.Should().Be(expected);
    }

    internal static IEnumerable<(IdentifierEscaping, string)> GetOverridenEntityNameTestCases()
    {
      yield return (Never, "Values");
      yield return (Keywords, "`Values`");
      yield return (Always, "`Values`");
    }

    [TestCaseSource(nameof(GetOverridenEntityNameTestCases))]
    public void GetName_GetOverridenEntityName((IdentifierEscaping escaping, string expected) testCase)
    {
      //Arrange
      var (escaping, expected) = testCase;
      var properties = new EntityProperties
      {
        EntityName = "Values",
        IdentifierEscaping = escaping
      };

      //Act
      var entityName = entityProvider.GetName<TestType>(properties);

      //Assert
      entityName.Should().Be(expected);
    }
  }
}
