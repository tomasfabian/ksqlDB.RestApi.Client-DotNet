using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using NUnit.Framework;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Visitors
{
  public class JoinAliasGeneratorTests
  {
    private JoinAliasGenerator joinAliasGenerator = null!;

    [SetUp]
    public void SetUp()
    {
      joinAliasGenerator = new JoinAliasGenerator();
    }

    [Test]
    public void GenerateAlias_WithDifferentNames_ShouldReturnUniqueAliases()
    {
      var alias1 = joinAliasGenerator.GenerateAlias("Name");
      var alias2 = joinAliasGenerator.GenerateAlias("Different");

      alias1.Should().NotBe(alias2);
    }

    [Test]
    public void GenerateAlias_WithSameName_ShouldReturnSameAlias()
    {
      var alias1 = joinAliasGenerator.GenerateAlias("Name");
      var alias2 = joinAliasGenerator.GenerateAlias("Name");

      alias1.Should().Be(alias2);
    }

    [Test]
    public void GenerateAlias_CalledWithNamesThatStartsSame_ShouldReturnDistinctAliases()
    {
      var alias1 = joinAliasGenerator.GenerateAlias("Name1");
      var alias2 = joinAliasGenerator.GenerateAlias("Name2");

      alias1.Should().NotBe(alias2);
    }
  }
}
