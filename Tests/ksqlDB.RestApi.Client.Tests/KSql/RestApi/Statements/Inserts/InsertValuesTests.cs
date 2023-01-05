using System.Linq.Expressions;
using FluentAssertions;
using ksqlDb.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Inserts;
using NUnit.Framework;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi.Statements.Inserts;

public class InsertValuesTests
{
  private record Movie
  {
    internal string Id { get; init; } = null!;
    internal string Release_Year { get; set; } = null!;
  }

  [KSqlFunction]
  public static string FormatTimestamp(long input, string format) => throw new NotSupportedException();

  [KSqlFunction]
  public static long FromUnixtime(long milliseconds) => throw new NotSupportedException();

  [KSqlFunction]
  public static long UNIX_TIMESTAMP() => throw new NotSupportedException();

  [Test]
  public void WithValue_RendersFromProvidedValue()
  {
    //Arrange
    Expression<Func<string>> valueExpression = () => FormatTimestamp(FromUnixtime(UNIX_TIMESTAMP()), "yyyy");

    var insertValues = new InsertValues<Movie>(new Movie { Id = "1" });

    //Act
    insertValues.WithValue(c => c.Release_Year, valueExpression)
      .WithValue(c => c.Id, () => "2");

    //Assert
    string expectedFunction = "FORMAT_TIMESTAMP(FROM_UNIXTIME(UNIX_TIMESTAMP()), 'yyyy')";

    insertValues.PropertyValues[nameof(Movie.Release_Year)].Should().Be(expectedFunction);
    insertValues.PropertyValues[nameof(Movie.Id)].Should().Be("'2'");
  }

  [Test]
  public void WithValue_IsCalledTwice_LastWins()
  {
    //Arrange
    Expression<Func<string>> valueExpression = () => "2022";

    var insertValues = new InsertValues<Movie>(new Movie { Id = "1" });

    //Act
    insertValues.WithValue(c => c.Release_Year, valueExpression)
      .WithValue(c => c.Release_Year, () => "2023");

    //Assert
    insertValues.PropertyValues[nameof(Movie.Release_Year)].Should().Be("'2023'");
  }

  [Test]
  public void WithValue_NotAPropertyGetter_ThrowsArgumentException()
  {
    //Arrange
    Expression<Func<string>> valueExpression = () => "2022";

    var insertValues = new InsertValues<Movie>(new Movie { Id = "1" });

    //Act
    Assert.Throws<ArgumentException>(() => insertValues.WithValue(c => c.ToString(), valueExpression));
  }
}
