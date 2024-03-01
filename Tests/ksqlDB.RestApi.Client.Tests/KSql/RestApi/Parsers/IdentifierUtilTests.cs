using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDb.RestApi.Client.KSql.RestApi.Parsers;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Parsers
{
  public class IdentifierUtilTests
  {
    [TestCase("FOO", ExpectedResult = true)]
    [TestCase("foo", ExpectedResult = true)]
    [TestCase("ARRAY", ExpectedResult = true)]
    [TestCase("MAP", ExpectedResult = true)]
    public bool ShouldBeValid(string identifier) => IdentifierUtil.IsValid(identifier);

    [TestCase("@FOO", ExpectedResult = false)]
    [TestCase("FOO.BAR", ExpectedResult = false)]
    [TestCase("SELECT", ExpectedResult = false)]
    [TestCase("VALUES", ExpectedResult = false)]
    public bool ShouldNotBeValid(string identifier) => IdentifierUtil.IsValid(identifier);

    [TestCase(SystemColumns.ROWTIME, IdentifierEscaping.Keywords, ExpectedResult = $"`{SystemColumns.ROWTIME}`")]
    [TestCase(SystemColumns.ROWTIME, IdentifierEscaping.Always, ExpectedResult = $"`{SystemColumns.ROWTIME}`")]
    [TestCase(SystemColumns.ROWOFFSET, IdentifierEscaping.Keywords, ExpectedResult = $"`{SystemColumns.ROWOFFSET}`")]
    [TestCase(SystemColumns.ROWOFFSET, IdentifierEscaping.Always, ExpectedResult = $"`{SystemColumns.ROWOFFSET}`")]
    [TestCase(SystemColumns.ROWPARTITION, IdentifierEscaping.Keywords,
      ExpectedResult = $"`{SystemColumns.ROWPARTITION}`")]
    [TestCase(SystemColumns.ROWPARTITION, IdentifierEscaping.Always, ExpectedResult = $"`{SystemColumns.ROWPARTITION}`")]
    [TestCase(SystemColumns.WINDOWSTART, IdentifierEscaping.Keywords, ExpectedResult = $"`{SystemColumns.WINDOWSTART}`")]
    [TestCase(SystemColumns.WINDOWSTART, IdentifierEscaping.Always, ExpectedResult = $"`{SystemColumns.WINDOWSTART}`")]
    [TestCase(SystemColumns.WINDOWEND, IdentifierEscaping.Keywords, ExpectedResult = $"`{SystemColumns.WINDOWEND}`")]
    [TestCase(SystemColumns.WINDOWEND, IdentifierEscaping.Always, ExpectedResult = $"`{SystemColumns.WINDOWEND}`")]
    public string ShouldBeFormatted(string identifier, IdentifierEscaping escaping) =>
      IdentifierUtil.Format(identifier, escaping);
  }
}
