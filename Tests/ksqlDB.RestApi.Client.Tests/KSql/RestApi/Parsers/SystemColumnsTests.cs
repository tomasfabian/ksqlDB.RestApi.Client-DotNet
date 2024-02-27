using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Parsers
{
  public class SystemColumnsTests
  {
    [TestCase(SystemColumns.ROWTIME, ExpectedResult = false)]
    [TestCase(SystemColumns.ROWOFFSET, ExpectedResult = false)]
    [TestCase(SystemColumns.ROWPARTITION, ExpectedResult = false)]
    [TestCase(SystemColumns.WINDOWSTART, ExpectedResult = false)]
    [TestCase(SystemColumns.WINDOWEND, ExpectedResult = false)]
    public bool ShouldNotBeValid(string identifier) => SystemColumns.IsValid(identifier);

    [TestCase("@FOO", ExpectedResult = true)]
    [TestCase("FOO.BAR", ExpectedResult = true)]
    [TestCase("SELECT", ExpectedResult = true)]
    [TestCase("VALUES", ExpectedResult = true)]
    public bool ShouldBeValidWhenNotReservedSystemColumnName(string identifier) => SystemColumns.IsValid(identifier);
  }
}
