using FluentAssertions;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDb.RestApi.Client.Tests.Models.Movies;
using NUnit.Framework;
using UnitTests;
using static ksqlDB.RestApi.Client.KSql.RestApi.Enums.IdentifierEscaping;
using TestParameters = ksqlDb.RestApi.Client.Tests.Helpers.TestParameters;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Visitors;

#pragma warning disable IDE0037

public class JoinVisitorTests : TestBase
{
  private KSqlDBContext KSqlDbContext { get; set; } = null!;

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    var contextOptions = new KSqlDBContextOptions(TestParameters.KsqlDbUrl);
    KSqlDbContext = new KSqlDBContext(contextOptions, new ModelBuilder());
  }

  [TearDown]
  public override void TestCleanup()
  {
    KSqlDbContext.Dispose();
    base.TestCleanup();
  }

  private static string MovieAlias => "movie";
  private static string ActorAlias => "actor";

  #region Join

  public static IEnumerable<(IdentifierEscaping, string)> JoinTestCases()
  {
    yield return (Never,
      @$"SELECT {MovieAlias}.Id Id, {MovieAlias}.Title Title, {MovieAlias}.Release_Year Release_Year, TRIM({ActorAlias}.Actor_Name) ActorName, UCASE({ActorAlias}.Actor_Name) UpperActorName, {ActorAlias}.Title AS ActorTitle FROM Movies {MovieAlias}
INNER JOIN LeadActors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords,
      @$"SELECT {MovieAlias}.Id Id, {MovieAlias}.Title Title, {MovieAlias}.Release_Year Release_Year, TRIM({ActorAlias}.Actor_Name) ActorName, UCASE({ActorAlias}.Actor_Name) UpperActorName, {ActorAlias}.Title AS ActorTitle FROM Movies {MovieAlias}
INNER JOIN LeadActors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always,
      @$"SELECT `{MovieAlias}`.`Id` `Id`, `{MovieAlias}`.`Title` `Title`, `{MovieAlias}`.`Release_Year` `Release_Year`, TRIM(`{ActorAlias}`.`Actor_Name`) `ActorName`, UCASE(`{ActorAlias}`.`Actor_Name`) `UpperActorName`, `{ActorAlias}`.`Title` AS `ActorTitle` FROM `Movies` `{MovieAlias}`
INNER JOIN `LeadActors` `{ActorAlias}`
ON `{MovieAlias}`.`Title` = `{ActorAlias}`.`Title`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(JoinTestCases))]
  public void Join_BuildKSql_PrintsInnerJoin((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = kSqlDbContext.CreatePushQuery<Movie>()
      .Join(
        Source.Of<Lead_Actor>("LeadActor"),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          movie.Id,
          movie.Title,
          movie.Release_Year,
          ActorName = K.Functions.Trim(actor.Actor_Name),
          UpperActorName = actor.Actor_Name.ToUpper(),
          ActorTitle = actor.Title
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be(expectedQuery);
  }

  public static IEnumerable<(IdentifierEscaping, string)> JoinPluralizedJointItemTestCases()
  {
    yield return (Never,
      @$"SELECT {MovieAlias}.Id Id, {MovieAlias}.Title Title, {MovieAlias}.Release_Year Release_Year, TRIM({ActorAlias}.Actor_Name) ActorName, UCASE({ActorAlias}.Actor_Name) UpperActorName, {ActorAlias}.Title AS ActorTitle FROM Movies {MovieAlias}
INNER JOIN Lead_Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords,
      @$"SELECT {MovieAlias}.Id Id, {MovieAlias}.Title Title, {MovieAlias}.Release_Year Release_Year, TRIM({ActorAlias}.Actor_Name) ActorName, UCASE({ActorAlias}.Actor_Name) UpperActorName, {ActorAlias}.Title AS ActorTitle FROM Movies {MovieAlias}
INNER JOIN Lead_Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always,
      @$"SELECT `{MovieAlias}`.`Id` `Id`, `{MovieAlias}`.`Title` `Title`, `{MovieAlias}`.`Release_Year` `Release_Year`, TRIM(`{ActorAlias}`.`Actor_Name`) `ActorName`, UCASE(`{ActorAlias}`.`Actor_Name`) `UpperActorName`, `{ActorAlias}`.`Title` AS `ActorTitle` FROM `Movies` `{MovieAlias}`
INNER JOIN `Lead_Actors` `{ActorAlias}`
ON `{MovieAlias}`.`Title` = `{ActorAlias}`.`Title`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(JoinPluralizedJointItemTestCases))]
  public void Join_BuildKSql_PrintsInnerJoin_PluralizedJoinItem((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = kSqlDbContext.CreatePushQuery<Movie>()
      .Join(
        Source.Of<Lead_Actor>(),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          movie.Id,
          movie.Title,
          movie.Release_Year,
          ActorName = K.Functions.Trim(actor.Actor_Name),
          UpperActorName = actor.Actor_Name.ToUpper(),
          ActorTitle = actor.Title
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be(expectedQuery);
  }

  public static IEnumerable<(IdentifierEscaping, string)> JoinAndSelectWithAliasesPrintsInnerJoinTestCases()
  {
    yield return (Never,
      @$"SELECT myMovie.Title Title, LEN({ActorAlias}.Actor_Name) Length FROM Movies myMovie
INNER JOIN Lead_Actors {ActorAlias}
ON myMovie.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords,
      @$"SELECT myMovie.Title Title, LEN({ActorAlias}.Actor_Name) Length FROM Movies myMovie
INNER JOIN Lead_Actors {ActorAlias}
ON myMovie.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always,
      @$"SELECT `myMovie`.`Title` `Title`, LEN(`{ActorAlias}`.`Actor_Name`) `Length` FROM `Movies` `myMovie`
INNER JOIN `Lead_Actors` `{ActorAlias}`
ON `myMovie`.`Title` = `{ActorAlias}`.`Title`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(JoinAndSelectWithAliasesPrintsInnerJoinTestCases))]
  public void JoinAndSelectWithAliases_BuildKSql_PrintsInnerJoin((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = kSqlDbContext.CreatePushQuery<Movie>()
      .Join(
        Source.Of<Lead_Actor>(),
        movie => movie.Title,
        actor => actor.Title,
        (myMovie, actor) => new
        {
          Title = myMovie.Title,
          Length = actor.Actor_Name.Length
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be(expectedQuery);
  }

  private class MovieExt : Movie
  {
  }

  public static IEnumerable<(IdentifierEscaping, string)> SameStreamNameDifferentAliasesTestCases()
  {
    yield return (Never, @$"SELECT {MovieAlias}.Title Title FROM Movies {MovieAlias}
INNER JOIN MovieExts ext
ON {MovieAlias}.Title = ext.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords, @$"SELECT {MovieAlias}.Title Title FROM Movies {MovieAlias}
INNER JOIN MovieExts ext
ON {MovieAlias}.Title = ext.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always, @$"SELECT `{MovieAlias}`.`Title` `Title` FROM `Movies` `{MovieAlias}`
INNER JOIN `MovieExts` `ext`
ON `{MovieAlias}`.`Title` = `ext`.`Title`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(SameStreamNameDifferentAliasesTestCases))]
  public void SameStreamName_BuildKSql_PrintsDifferentAliases((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = kSqlDbContext.CreatePushQuery<Movie>()
      .Join(
        Source.Of<MovieExt>(),
        movie => movie.Title,
        ext => ext.Title,
        (movie, ext) => new
        {
          Title = movie.Title,
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be(expectedQuery);
  }

  public static IEnumerable<(IdentifierEscaping, string)> InnerJoinOverrideStatementNoProjectionFromJoinTableTestCases()
  {
    yield return (Never, @$"SELECT {MovieAlias}.Title Title FROM Movies {MovieAlias}
INNER JOIN Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords, @$"SELECT {MovieAlias}.Title Title FROM Movies {MovieAlias}
INNER JOIN Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always, @$"SELECT `{MovieAlias}`.`Title` `Title` FROM `Movies` `{MovieAlias}`
INNER JOIN `Actors` `{ActorAlias}`
ON `{MovieAlias}`.`Title` = `{ActorAlias}`.`Title`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(InnerJoinOverrideStatementNoProjectionFromJoinTableTestCases))]
  public void InnerJoinOverrideStreamName_NoProjectionFromJoinTable_BuildKSql_Prints((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = kSqlDbContext.CreatePushQuery<Movie>()
      .Join(
        Source.Of<Lead_Actor>("Actors"),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          Title = movie.Title
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be(expectedQuery);
  }

  public static IEnumerable<(IdentifierEscaping, string)> InnerJoinOverrideStatementTestCases()
  {
    yield return (Never,
      @$"SELECT {MovieAlias}.Title Title, {ActorAlias}.Actor_Name AS ActorName FROM Movies {MovieAlias}
INNER JOIN Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords,
      @$"SELECT {MovieAlias}.Title Title, {ActorAlias}.Actor_Name AS ActorName FROM Movies {MovieAlias}
INNER JOIN Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always,
      @$"SELECT `{MovieAlias}`.`Title` `Title`, `{ActorAlias}`.`Actor_Name` AS `ActorName` FROM `Movies` `{MovieAlias}`
INNER JOIN `Actors` `{ActorAlias}`
ON `{MovieAlias}`.`Title` = `{ActorAlias}`.`Title`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(InnerJoinOverrideStatementTestCases))]
  public void InnerJoinOverrideStreamName_BuildKSql_Prints((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = kSqlDbContext.CreatePushQuery<Movie>()
      .Join(
        Source.Of<Lead_Actor>("Actors"),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          Title = movie.Title,
          ActorName = actor.Actor_Name
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be(expectedQuery);
  }

  public static IEnumerable<(IdentifierEscaping, string)> InnerJoinQuerySyntaxTestCases()
  {
    yield return (Never, @$"SELECT {MovieAlias}.Title Title, {ActorAlias}.Actor_Name AS ActorName FROM Movies {MovieAlias}
INNER JOIN Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords, @$"SELECT {MovieAlias}.Title Title, {ActorAlias}.Actor_Name AS ActorName FROM Movies {MovieAlias}
INNER JOIN Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always, @$"SELECT `{MovieAlias}`.`Title` `Title`, `{ActorAlias}`.`Actor_Name` AS `ActorName` FROM `Movies` `{MovieAlias}`
INNER JOIN `Actors` `{ActorAlias}`
ON `{MovieAlias}`.`Title` = `{ActorAlias}`.`Title`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(InnerJoinQuerySyntaxTestCases))]
  public void InnerJoinQuerySyntax_BuildKSql_Prints((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = from movie in kSqlDbContext.CreatePushQuery<Movie>()
      join actor in Source.Of<Lead_Actor>("Actors") on movie.Title equals actor.Title
      select new
      {
        movie.Title,
        ActorName = actor.Actor_Name
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be(expectedQuery);
  }

  private class Payment
  {
    public int Id { get; set; }
  }

  private record Shipment
  {
    public int Id { get; set; }
  }

  struct Foo
  {
    public int Prop { get; set; }
  }

  public static IEnumerable<(IdentifierEscaping, string)> MultipleInnerJoinsQuerySyntaxTestCases()
  {
    yield return (Never,
      @"SELECT STRUCT(Prop := 42) value, O.OrderId AS orderId, S1.Id AS shipmentId, P1.Id AS paymentId FROM Orders O
INNER JOIN Shipments S1
ON O.OrderId = S1.Id
INNER JOIN Payments P1
ON O.OrderId = P1.Id
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords,
      @"SELECT STRUCT(Prop := 42) value, O.OrderId AS orderId, S1.Id AS shipmentId, P1.Id AS paymentId FROM Orders O
INNER JOIN Shipments S1
ON O.OrderId = S1.Id
INNER JOIN Payments P1
ON O.OrderId = P1.Id
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always,
      @"SELECT STRUCT(`Prop` := 42) `value`, `O`.`OrderId` AS `orderId`, `S1`.`Id` AS `shipmentId`, `P1`.`Id` AS `paymentId` FROM `Orders` `O`
INNER JOIN `Shipments` `S1`
ON `O`.`OrderId` = `S1`.`Id`
INNER JOIN `Payments` `P1`
ON `O`.`OrderId` = `P1`.`Id`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(MultipleInnerJoinsQuerySyntaxTestCases))]
  public void MultipleInnerJoinsQuerySyntax_BuildKSql_Prints((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var value = new Foo { Prop = 42 };
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = from o in kSqlDbContext.CreatePushQuery<Order>()
      join p1 in Source.Of<Payment>() on o.OrderId equals p1.Id
      join s1 in Source.Of<Shipment>() on o.OrderId equals s1.Id
      select new
      {
        value,
        orderId = o.OrderId,
        shipmentId = s1.Id,
        paymentId = p1.Id,
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(expectedQuery);
  }

  class LambdaMap
  {
    public int Id { get; set; }
    public IDictionary<string, City> Dictionary { get; set; } = null!;
    public Nested Nested { get; set; } = null!;
  }

  private class City
  {
    public int[] Values { get; set; } = null!;
  }

  private class Nested
  {
    public string Prop { get; set; } = null!;
  }

  public static IEnumerable<(IdentifierEscaping, string)> TestCasesJoinWithInvocation()
  {
    yield return (Never,
      @$"SELECT TRANSFORM(lm.{nameof(LambdaMap.Dictionary)}, (k, v) => CONCAT(k, '_new'), (k, v) => TRANSFORM(v->Values, (x) => x * x)) A, O.OrderId AS orderId, s1.Id AS shipmentId FROM Orders O
INNER JOIN Shipments s1
ON O.OrderId = s1.Id
INNER JOIN LambdaMaps lm
ON O.OrderId = lm.Id
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords,
      @$"SELECT TRANSFORM(lm.{nameof(LambdaMap.Dictionary)}, (k, v) => CONCAT(k, '_new'), (k, v) => TRANSFORM(v->`Values`, (x) => x * x)) A, O.OrderId AS orderId, s1.Id AS shipmentId FROM Orders O
INNER JOIN Shipments s1
ON O.OrderId = s1.Id
INNER JOIN LambdaMaps lm
ON O.OrderId = lm.Id
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always,
      @$"SELECT TRANSFORM(`lm`.`{nameof(LambdaMap.Dictionary)}`, (k, v) => CONCAT(k, '_new'), (k, v) => TRANSFORM(v->`Values`, (x) => x * x)) `A`, `O`.`OrderId` AS `orderId`, `s1`.`Id` AS `shipmentId` FROM `Orders` `O`
INNER JOIN `Shipments` `s1`
ON `O`.`OrderId` = `s1`.`Id`
INNER JOIN `LambdaMaps` `lm`
ON `O`.`OrderId` = `lm`.`Id`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(TestCasesJoinWithInvocation))]
  public void JoinWithInvocationFunction_BuildKSql_Prints((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = from o in kSqlDbContext.CreatePushQuery<Order>()
      join lm in Source.Of<LambdaMap>() on o.OrderId equals lm.Id
      join s1 in Source.Of<Shipment>() on o.OrderId equals s1.Id
      select new
      {
        A = K.Functions.Transform(lm.Dictionary, (k, v) => K.Functions.Concat(k, "_new"), (k, v) => K.Functions.Transform(v.Values, x => x * x)),
        orderId = o.OrderId,
        shipmentId = s1.Id
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(expectedQuery);
  }

  [Test]
  [Ignore("TODO:")]
  public void JoinWithSeveralOnConditions_BuildKSql_Prints()
  {
    //Arrange
    var query = from o in KSqlDbContext.CreatePushQuery<Order>()
      join lm in Source.Of<LambdaMap>() on new { OrderId = o.OrderId, NestedProp = "Nested" } equals new { OrderId = lm.Id, NestedProp = lm.Nested.Prop }
      select new
      {
        lm.Nested.Prop,
        orderId = o.OrderId,
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    //TODO:
  }

  public static IEnumerable<(IdentifierEscaping, string)> JoinWithNestedTypeTestCases()
  {
    yield return (Never, @$"SELECT lm.Nested->Prop Prop, O.OrderId AS orderId FROM Orders O
INNER JOIN LambdaMaps lm
ON O.OrderId = lm.Id
WHERE lm.Nested->Prop = 'Nested' EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords, @$"SELECT lm.Nested->Prop Prop, O.OrderId AS orderId FROM Orders O
INNER JOIN LambdaMaps lm
ON O.OrderId = lm.Id
WHERE lm.Nested->Prop = 'Nested' EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always, @$"SELECT `lm`.`Nested`->`Prop` `Prop`, `O`.`OrderId` AS `orderId` FROM `Orders` `O`
INNER JOIN `LambdaMaps` `lm`
ON `O`.`OrderId` = `lm`.`Id`
WHERE `lm`.`Nested`->`Prop` = 'Nested' EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(JoinWithNestedTypeTestCases))]
  public void JoinWithNestedType_BuildKSql_Prints((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = from o in kSqlDbContext.CreatePushQuery<Order>()
      join lm in Source.Of<LambdaMap>() on o.OrderId equals lm.Id
      where lm.Nested.Prop == "Nested"
      select new
      {
        lm.Nested.Prop,
        orderId = o.OrderId,
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(expectedQuery);
  }

  public static IEnumerable<(IdentifierEscaping, string)> JoinWithFunctionAndNestedTypeTestCases()
  {
    yield return (Never,
      @$"SELECT CONCAT(lm.Nested->Prop, '_new') Concat, o.OrderId AS orderId FROM Orders o
INNER JOIN LambdaMaps lm
ON o.OrderId = lm.Id
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords,
      @$"SELECT CONCAT(lm.Nested->Prop, '_new') Concat, o.OrderId AS orderId FROM Orders o
INNER JOIN LambdaMaps lm
ON o.OrderId = lm.Id
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always,
      @$"SELECT CONCAT(`lm`.`Nested`->`Prop`, '_new') `Concat`, `o`.`OrderId` AS `orderId` FROM `Orders` `o`
INNER JOIN `LambdaMaps` `lm`
ON `o`.`OrderId` = `lm`.`Id`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(JoinWithFunctionAndNestedTypeTestCases))]
  public void JoinWithFunctionAndNestedType_BuildKSql_Prints((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = from o in kSqlDbContext.CreatePushQuery<Order>()
      join lm in Source.Of<LambdaMap>() on o.OrderId equals lm.Id
      select new
      {
        Concat = K.Functions.Concat(lm.Nested.Prop, "_new"),
        orderId = o.OrderId,
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(expectedQuery);
  }

  public static IEnumerable<(IdentifierEscaping, string)> MultipleInnerJoinsQuerySyntaxWithTakeTestCase()
  {
    yield return (Never,
      @"SELECT O.OrderId AS orderId, S1.Id AS shipmentId, P1.Id AS paymentId FROM Orders O
INNER JOIN Shipments S1
ON O.OrderId = S1.Id
INNER JOIN Payments P1
ON O.OrderId = P1.Id
EMIT CHANGES LIMIT 2;".ReplaceLineEndings());
    yield return (Keywords,
      @"SELECT O.OrderId AS orderId, S1.Id AS shipmentId, P1.Id AS paymentId FROM Orders O
INNER JOIN Shipments S1
ON O.OrderId = S1.Id
INNER JOIN Payments P1
ON O.OrderId = P1.Id
EMIT CHANGES LIMIT 2;".ReplaceLineEndings());
    yield return (Always,
      @"SELECT `O`.`OrderId` AS `orderId`, `S1`.`Id` AS `shipmentId`, `P1`.`Id` AS `paymentId` FROM `Orders` `O`
INNER JOIN `Shipments` `S1`
ON `O`.`OrderId` = `S1`.`Id`
INNER JOIN `Payments` `P1`
ON `O`.`OrderId` = `P1`.`Id`
EMIT CHANGES LIMIT 2;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(MultipleInnerJoinsQuerySyntaxWithTakeTestCase))]
  public void MultipleInnerJoinsQuerySyntax_WithTake_BuildKSql_Prints((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = from o in kSqlDbContext.CreatePushQuery<Order>()
      join p1 in Source.Of<Payment>() on o.OrderId equals p1.Id
      join s1 in Source.Of<Shipment>() on o.OrderId equals s1.Id
      select new
      {
        orderId = o.OrderId,
        shipmentId = s1.Id,
        paymentId = p1.Id,
      };

    query = query.Take(2);

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(expectedQuery);
  }

  public static IEnumerable<(IdentifierEscaping, string)> JoinAndLeftJoinWithTakeTestCases()
  {
    yield return (Never, @$"SELECT o.OrderId AS orderId, sa.Id AS shipmentId, p1.Id AS paymentId FROM Orders o
LEFT JOIN Shipments sa
ON o.OrderId = sa.Id
INNER JOIN Payments p1
ON o.OrderId = p1.Id
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords, @$"SELECT o.OrderId AS orderId, sa.Id AS shipmentId, p1.Id AS paymentId FROM Orders o
LEFT JOIN Shipments sa
ON o.OrderId = sa.Id
INNER JOIN Payments p1
ON o.OrderId = p1.Id
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always, @$"SELECT `o`.`OrderId` AS `orderId`, `sa`.`Id` AS `shipmentId`, `p1`.`Id` AS `paymentId` FROM `Orders` `o`
LEFT JOIN `Shipments` `sa`
ON `o`.`OrderId` = `sa`.`Id`
INNER JOIN `Payments` `p1`
ON `o`.`OrderId` = `p1`.`Id`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(JoinAndLeftJoinWithTakeTestCases))]
  public void JoinAndLeftJoin_WithTake_BuildKSql_Prints((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = from o in kSqlDbContext.CreatePushQuery<Order>()
      join p1 in Source.Of<Payment>() on o.OrderId equals p1.Id
      join s1 in Source.Of<Shipment>() on o.OrderId equals s1.Id into gj
      from sa in gj.DefaultIfEmpty()
      select new
      {
        orderId = o.OrderId,
        shipmentId = sa.Id,
        paymentId = p1.Id
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(expectedQuery);
  }

  public static IEnumerable<(IdentifierEscaping, string)> JoinWithinTimeUnitTestCases()
  {
    yield return (Never,
      @$"SELECT o.OrderId AS orderId, s.Id AS shipmentId, p.Id AS paymentId FROM Orders o
INNER JOIN Shipments s
WITHIN 5 DAYS ON o.OrderId = s.Id
INNER JOIN Payments p
WITHIN 1 HOURS ON o.OrderId = p.Id
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords,
      @$"SELECT o.OrderId AS orderId, s.Id AS shipmentId, p.Id AS paymentId FROM Orders o
INNER JOIN Shipments s
WITHIN 5 DAYS ON o.OrderId = s.Id
INNER JOIN Payments p
WITHIN 1 HOURS ON o.OrderId = p.Id
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always,
      @$"SELECT `o`.`OrderId` AS `orderId`, `s`.`Id` AS `shipmentId`, `p`.`Id` AS `paymentId` FROM `Orders` `o`
INNER JOIN `Shipments` `s`
WITHIN 5 DAYS ON `o`.`OrderId` = `s`.`Id`
INNER JOIN `Payments` `p`
WITHIN 1 HOURS ON `o`.`OrderId` = `p`.`Id`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(JoinWithinTimeUnitTestCases))]
  public void JoinWithinTimeUnit_BuildKSql_Prints((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = from o in kSqlDbContext.CreatePushQuery<Order>()
      join p in Source.Of<Payment>().Within(Duration.OfHours(1)) on o.OrderId equals p.Id
      join s in Source.Of<Shipment>().Within(Duration.OfDays(5)) on o.OrderId equals s.Id
      select new
      {
        orderId = o.OrderId,
        shipmentId = s.Id,
        paymentId = p.Id
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(expectedQuery);
  }

  public static IEnumerable<(IdentifierEscaping, string)> JoinWithTimeUnitBeforeAfterTestCases()
  {
    yield return (Never, @$"SELECT o.OrderId AS orderId, p.Id AS paymentId FROM Orders o
INNER JOIN Payments p
WITHIN (1 HOURS, 5 DAYS) ON o.OrderId = p.Id
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords, @$"SELECT o.OrderId AS orderId, p.Id AS paymentId FROM Orders o
INNER JOIN Payments p
WITHIN (1 HOURS, 5 DAYS) ON o.OrderId = p.Id
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always, @$"SELECT `o`.`OrderId` AS `orderId`, `p`.`Id` AS `paymentId` FROM `Orders` `o`
INNER JOIN `Payments` `p`
WITHIN (1 HOURS, 5 DAYS) ON `o`.`OrderId` = `p`.`Id`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(JoinWithTimeUnitBeforeAfterTestCases))]
  public void JoinWithinTimeUnit_BeforeAfter_BuildKSql_Prints((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = from o in kSqlDbContext.CreatePushQuery<Order>()
      join p in Source.Of<Payment>().Within(Duration.OfHours(1), Duration.OfDays(5)) on o.OrderId equals p.Id
      select new
      {
        orderId = o.OrderId,
        paymentId = p.Id
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(expectedQuery);
  }

  public static IEnumerable<(IdentifierEscaping, string)> JoinKSqlFunctionKeySelectorTestCases()
  {
    yield return (Never, @"SELECT actor.Title AS EnduserId, actor.Actor_Name AS Name, movie.Title AS Raw FROM movies movie
INNER JOIN actors actor
WITHIN 1 DAYS ON EXTRACTJSONFIELD(movie.Title, '$.movie_title') = EXTRACTJSONFIELD(actor.Actor_Name, '$.actor_name')
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords, @"SELECT actor.Title AS EnduserId, actor.Actor_Name AS Name, movie.Title AS Raw FROM movies movie
INNER JOIN actors actor
WITHIN 1 DAYS ON EXTRACTJSONFIELD(movie.Title, '$.movie_title') = EXTRACTJSONFIELD(actor.Actor_Name, '$.actor_name')
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always, @"SELECT `actor`.`Title` AS `EnduserId`, `actor`.`Actor_Name` AS `Name`, `movie`.`Title` AS `Raw` FROM `movies` `movie`
INNER JOIN `actors` `actor`
WITHIN 1 DAYS ON EXTRACTJSONFIELD(`movie`.`Title`, '$.movie_title') = EXTRACTJSONFIELD(`actor`.`Actor_Name`, '$.actor_name')
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(JoinKSqlFunctionKeySelectorTestCases))]
  public void Join_KSqlFunctionKeySelector_ShouldBeWithoutAlias((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = kSqlDbContext
      .CreatePushQuery<Movie>("movies")
      .Join(Source.Of<Lead_Actor>("actors").Within(Duration.OfDays(1)),
        movie => K.Functions.ExtractJsonField(movie!.Title, "$.movie_title"),
        actor => K.Functions.ExtractJsonField(actor!.Actor_Name, "$.actor_name"),
        (movie, actor) => new { EnduserId = actor.Title, Name = actor.Actor_Name, Raw = movie.Title });

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(expectedQuery);
  }
  #endregion

  #region LeftJoin

  public static IEnumerable<(IdentifierEscaping, string)> LeftJoinTestCases()
  {
    yield return (Never,
      @$"SELECT movie.Id Id, {MovieAlias}.Title Title, {MovieAlias}.Release_Year Release_Year, TRIM({ActorAlias}.Actor_Name) ActorName, UCASE({ActorAlias}.Actor_Name) UpperActorName, {ActorAlias}.Title AS ActorTitle FROM Movies {MovieAlias}
LEFT JOIN Lead_Actors {ActorAlias}
ON movie.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords,
      @$"SELECT movie.Id Id, {MovieAlias}.Title Title, {MovieAlias}.Release_Year Release_Year, TRIM({ActorAlias}.Actor_Name) ActorName, UCASE({ActorAlias}.Actor_Name) UpperActorName, {ActorAlias}.Title AS ActorTitle FROM Movies {MovieAlias}
LEFT JOIN Lead_Actors {ActorAlias}
ON movie.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always,
      @$"SELECT `movie`.`Id` `Id`, `{MovieAlias}`.`Title` `Title`, `{MovieAlias}`.`Release_Year` `Release_Year`, TRIM(`{ActorAlias}`.`Actor_Name`) `ActorName`, UCASE(`{ActorAlias}`.`Actor_Name`) `UpperActorName`, `{ActorAlias}`.`Title` AS `ActorTitle` FROM `Movies` `{MovieAlias}`
LEFT JOIN `Lead_Actors` `{ActorAlias}`
ON `movie`.`Title` = `{ActorAlias}`.`Title`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(LeftJoinTestCases))]
  public void LeftJoin_BuildKSql_PrintsLeftJoin((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = kSqlDbContext.CreatePushQuery<Movie>()
      .LeftJoin(
        Source.Of<Lead_Actor>(),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          movie.Id,
          movie.Title,
          movie.Release_Year,
          ActorName = K.Functions.Trim(actor.Actor_Name),
          UpperActorName = actor.Actor_Name.ToUpper(),
          ActorTitle = actor.Title
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be(expectedQuery);
  }

  public static IEnumerable<(IdentifierEscaping, string)> LeftJoinQuerySyntaxTestCases()
  {
    yield return (Never,
      @$"SELECT {MovieAlias}.Id Id, UCASE(a.Actor_Name) UpperActorName, a.Title AS ActorTitle FROM Movies {MovieAlias}
LEFT JOIN Actors a
ON {MovieAlias}.Title = a.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords,
      @$"SELECT {MovieAlias}.Id Id, UCASE(a.Actor_Name) UpperActorName, a.Title AS ActorTitle FROM Movies {MovieAlias}
LEFT JOIN Actors a
ON {MovieAlias}.Title = a.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always,
      @$"SELECT `{MovieAlias}`.`Id` `Id`, UCASE(`a`.`Actor_Name`) `UpperActorName`, `a`.`Title` AS `ActorTitle` FROM `Movies` `{MovieAlias}`
LEFT JOIN `Actors` `a`
ON `{MovieAlias}`.`Title` = `a`.`Title`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(LeftJoinQuerySyntaxTestCases))]
  public void LeftJoinQuerySyntax_BuildKSql_Prints((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query =
      from movie in kSqlDbContext.CreatePushQuery<Movie>()
      join actor in Source.Of<Lead_Actor>("Actors")
        on movie.Title equals actor.Title into gj
      from a in gj.DefaultIfEmpty()
      select new
      {
        movie.Id,
        UpperActorName = a.Actor_Name.ToUpper(),
        ActorTitle = a.Title
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be(expectedQuery);
  }

  public static IEnumerable<(IdentifierEscaping, string)> TestCasesGroupJoinSelectMany()
  {
    yield return (Never,
      @$"SELECT {MovieAlias}.Id Id, UCASE(a.Actor_Name) UpperActorName, a.Title AS ActorTitle FROM Movies {MovieAlias}
LEFT JOIN Actors a
ON {MovieAlias}.Title = a.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords,
      @$"SELECT {MovieAlias}.Id Id, UCASE(a.Actor_Name) UpperActorName, a.Title AS ActorTitle FROM Movies {MovieAlias}
LEFT JOIN Actors a
ON {MovieAlias}.Title = a.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always,
      @$"SELECT `{MovieAlias}`.`Id` `Id`, UCASE(`a`.`Actor_Name`) `UpperActorName`, `a`.`Title` AS `ActorTitle` FROM `Movies` `{MovieAlias}`
LEFT JOIN `Actors` `a`
ON `{MovieAlias}`.`Title` = `a`.`Title`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(TestCasesGroupJoinSelectMany))]
  public void GroupJoinSelectMany_BuildKSql_Prints((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(
      new KSqlDBContextOptions(TestParameters.KsqlDbUrl) { IdentifierEscaping = escaping } );
    var query = kSqlDbContext.CreatePushQuery<Movie>()
      .GroupJoin(Source.Of<Lead_Actor>("Actors"), c => c.Title, d => d.Title, (movie, gj) => new
      {
        movie,
        grouping = gj
      }).SelectMany(c => c.grouping.DefaultIfEmpty(), (movie, a) => new
      {
        movie.movie.Id,
        UpperActorName = a.Actor_Name.ToUpper(),
        ActorTitle = a.Title
      });

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be(expectedQuery);
  }

  private class Order
  {
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public int ItemId { get; set; }
  }

  private sealed class Customer
  {
    public int CustomerId { get; set; }
  }

  private sealed class Item
  {
    public int ItemId { get; set; }
    public string ItemName { get; set; } = null!;
  }

  public static IEnumerable<(IdentifierEscaping, string)> MultipleLeftJoinsQuerySyntax()
  {
    yield return (Never,
      @"SELECT customers.CustomerId AS customerid, orders.OrderId OrderId, items.ItemId ItemId, items.ItemName ItemName FROM Orders orders
LEFT JOIN Items items
ON orders.ItemId = items.ItemId
LEFT JOIN Customers customers
ON orders.CustomerId = customers.CustomerId
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords,
      @"SELECT customers.CustomerId AS customerid, orders.OrderId OrderId, items.ItemId ItemId, items.ItemName ItemName FROM Orders orders
LEFT JOIN Items items
ON orders.ItemId = items.ItemId
LEFT JOIN Customers customers
ON orders.CustomerId = customers.CustomerId
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always,
      @"SELECT `customers`.`CustomerId` AS `customerid`, `orders`.`OrderId` `OrderId`, `items`.`ItemId` `ItemId`, `items`.`ItemName` `ItemName` FROM `Orders` `orders`
LEFT JOIN `Items` `items`
ON `orders`.`ItemId` = `items`.`ItemId`
LEFT JOIN `Customers` `customers`
ON `orders`.`CustomerId` = `customers`.`CustomerId`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(MultipleLeftJoinsQuerySyntax))]
  public void MultipleLeftJoinsQuerySyntax_BuildKSql_Prints((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query =
      from orders in kSqlDbContext.CreatePushQuery<Order>()
      join customer in Source.Of<Customer>()
        on orders.CustomerId equals customer.CustomerId into gj
      from customers in gj.DefaultIfEmpty()
      join item in Source.Of<Item>()
        on orders.ItemId equals item.ItemId into igj
      from items in igj.DefaultIfEmpty()
      select new
      {
        customerid = customers.CustomerId,
        orders.OrderId,
        items.ItemId,
        items.ItemName,
      };

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be(expectedQuery);
  }

  public static IEnumerable<(IdentifierEscaping, string)> LeftJoinOverrideStreamNameTestCases()
  {
    yield return (Never,
      @$"SELECT {ActorAlias}.RowTime RowTime, {MovieAlias}.Title Title FROM Movies {MovieAlias}
LEFT JOIN Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords,
      @$"SELECT {ActorAlias}.RowTime `RowTime`, {MovieAlias}.Title Title FROM Movies {MovieAlias}
LEFT JOIN Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always,
      @$"SELECT `{ActorAlias}`.RowTime `RowTime`, `{MovieAlias}`.`Title` `Title` FROM `Movies` `{MovieAlias}`
LEFT JOIN `Actors` `{ActorAlias}`
ON `{MovieAlias}`.`Title` = `{ActorAlias}`.`Title`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(LeftJoinOverrideStreamNameTestCases))]
  public void LeftJoinOverrideStreamName_BuildKSql_Prints((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = kSqlDbContext.CreatePushQuery<Movie>()
      .LeftJoin(
        Source.Of<Lead_Actor>("Actors"),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          actor.RowTime,
          Title = movie.Title,
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be(expectedQuery);
  }

  #endregion

  #region FullOuterJoin

  public static IEnumerable<(IdentifierEscaping, string)> FullOuterJoinOverrideStatementTestCases()
  {
    yield return (Never,
      @$"SELECT {MovieAlias}.Title Title, {ActorAlias}.Actor_Name AS ActorName FROM Movies {MovieAlias}
FULL OUTER JOIN Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords,
      @$"SELECT {MovieAlias}.Title Title, {ActorAlias}.Actor_Name AS ActorName FROM Movies {MovieAlias}
FULL OUTER JOIN Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always,
      @$"SELECT `{MovieAlias}`.`Title` `Title`, `{ActorAlias}`.`Actor_Name` AS `ActorName` FROM `Movies` `{MovieAlias}`
FULL OUTER JOIN `Actors` `{ActorAlias}`
ON `{MovieAlias}`.`Title` = `{ActorAlias}`.`Title`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(FullOuterJoinOverrideStatementTestCases))]
  public void FullOuterJoinOverrideStreamName_BuildKSql_Prints((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = kSqlDbContext.CreatePushQuery<Movie>()
      .FullOuterJoin(
        Source.Of<Lead_Actor>("Actors"),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          Title = movie.Title,
          ActorName = actor.Actor_Name
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be(expectedQuery);
  }

  #endregion

  #region RightJoin

  public static IEnumerable<(IdentifierEscaping, string)> RightJoinOverrideStreamNameTestCases()
  {
    yield return (Never,
      @$"SELECT {MovieAlias}.Title Title, {ActorAlias}.Actor_Name AS ActorName FROM Movies {MovieAlias}
RIGHT JOIN Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Keywords,
      @$"SELECT {MovieAlias}.Title Title, {ActorAlias}.Actor_Name AS ActorName FROM Movies {MovieAlias}
RIGHT JOIN Actors {ActorAlias}
ON {MovieAlias}.Title = {ActorAlias}.Title
EMIT CHANGES;".ReplaceLineEndings());
    yield return (Always,
      @$"SELECT `{MovieAlias}`.`Title` `Title`, `{ActorAlias}`.`Actor_Name` AS `ActorName` FROM `Movies` `{MovieAlias}`
RIGHT JOIN `Actors` `{ActorAlias}`
ON `{MovieAlias}`.`Title` = `{ActorAlias}`.`Title`
EMIT CHANGES;".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(RightJoinOverrideStreamNameTestCases))]
  public void RightJoinOverrideStreamName_BuildKSql_Prints((IdentifierEscaping escaping, string expectedQuery) testCase)
  {
    //Arrange
    var (escaping, expectedQuery) = testCase;
    var kSqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });
    var query = kSqlDbContext.CreatePushQuery<Movie>()
      .RightJoin(
        Source.Of<Lead_Actor>("Actors"),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          Title = movie.Title,
          ActorName = actor.Actor_Name
        }
      );

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be(expectedQuery);
  }

  #endregion
}

#pragma warning restore IDE0037
