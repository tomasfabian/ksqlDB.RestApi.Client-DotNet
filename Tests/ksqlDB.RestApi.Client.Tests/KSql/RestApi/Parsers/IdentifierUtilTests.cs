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

    [TestCase(SystemColumns.ROWTIME, IdentifierFormat.Keywords, ExpectedResult = $"`{SystemColumns.ROWTIME}`")]
    [TestCase(SystemColumns.ROWTIME, IdentifierFormat.Always, ExpectedResult = $"`{SystemColumns.ROWTIME}`")]
    [TestCase(SystemColumns.ROWOFFSET, IdentifierFormat.Keywords, ExpectedResult = $"`{SystemColumns.ROWOFFSET}`")]
    [TestCase(SystemColumns.ROWOFFSET, IdentifierFormat.Always, ExpectedResult = $"`{SystemColumns.ROWOFFSET}`")]
    [TestCase(SystemColumns.ROWPARTITION, IdentifierFormat.Keywords,
      ExpectedResult = $"`{SystemColumns.ROWPARTITION}`")]
    [TestCase(SystemColumns.ROWPARTITION, IdentifierFormat.Always, ExpectedResult = $"`{SystemColumns.ROWPARTITION}`")]
    [TestCase(SystemColumns.WINDOWSTART, IdentifierFormat.Keywords, ExpectedResult = $"`{SystemColumns.WINDOWSTART}`")]
    [TestCase(SystemColumns.WINDOWSTART, IdentifierFormat.Always, ExpectedResult = $"`{SystemColumns.WINDOWSTART}`")]
    [TestCase(SystemColumns.WINDOWEND, IdentifierFormat.Keywords, ExpectedResult = $"`{SystemColumns.WINDOWEND}`")]
    [TestCase(SystemColumns.WINDOWEND, IdentifierFormat.Always, ExpectedResult = $"`{SystemColumns.WINDOWEND}`")]
    public string ShouldBeFormatted(string identifier, IdentifierFormat format) =>
      IdentifierUtil.Format(identifier, format);
  }
}
