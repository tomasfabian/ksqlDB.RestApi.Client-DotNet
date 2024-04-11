using System.Linq.Expressions;
using FluentAssertions;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using ksqlDb.RestApi.Client.Tests.Models;
using NUnit.Framework;
using UnitTests;
using Location = ksqlDb.RestApi.Client.Tests.Models.Location;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query;

public class KSqlVisitorTests : TestBase
{
  private ModelBuilder modelBuilder = null!;
  private KSqlVisitor ClassUnderTest { get; set; } = null!;

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    modelBuilder = new();
    ClassUnderTest = new KSqlVisitor(new KSqlQueryMetadata { ModelBuilder = modelBuilder });
  }

  #region Constants

  [Test]
  public void NullConstant_BuildKSql_PrintsStringifiedNull()
  {
    //Arrange
    var constantExpression = Expression.Constant(null);

    //Act
    var query = ClassUnderTest.BuildKSql(constantExpression);

    //Assert
    query.Should().BeEquivalentTo("NULL");
  }

  [Test]
  public void TextConstant_BuildKSql_PrintsTextSurroundedWithTicks()
  {
    //Arrange
    var constant = "TeSt Me";
    var constantExpression = Expression.Constant(constant);

    //Act
    var query = ClassUnderTest.BuildKSql(constantExpression);

    //Assert
    query.Should().BeEquivalentTo($"'{constant}'");
  }

  [Test]
  public void ValueTypeConstant_BuildKSql_PrintsPlainText()
  {
    //Arrange
    var constant = 42;
    var constantExpression = Expression.Constant(constant);

    //Act
    var query = ClassUnderTest.BuildKSql(constantExpression);

    //Assert
    query.Should().BeEquivalentTo(constant.ToString());
  }

  [Test]
  public void EnumerableStringConstant_BuildKSql_PrintsArray()
  {
    //Arrange
    var constant = new[] { "Field1", "Field2" };
    var constantExpression = Expression.Constant(constant);

    //Act
    var query = ClassUnderTest.BuildKSql(constantExpression);

    //Assert
    query.Should().BeEquivalentTo("ARRAY['Field1', 'Field2']");
  }

  [Test]
  public void EnumerableIntConstant_BuildKSql_PrintsArray()
  {
    //Arrange
    var constant = new[] { 1, 2 };
    var constantExpression = Expression.Constant(constant);

    //Act
    var query = ClassUnderTest.BuildKSql(constantExpression);

    //Assert
    query.Should().BeEquivalentTo("ARRAY[1, 2]");
  }

  [Test]
  public void ReferenceTypeConstant_BuildKSql_PrintsCommaSeparatedTextFields()
  {
    //Arrange
    var constant = new object();
    var constantExpression = Expression.Constant(constant);

    //Act
    var query = ClassUnderTest.BuildKSql(constantExpression);

    //Assert
    query.Should().BeEquivalentTo("Struct()");
  }

  #endregion

  #region Binary

  [Test]
  public void BinaryAnd_BuildKSql_PrintsOperatorAnd()
  {
    //Arrange
    var andAlso = Expression.AndAlso(Expression.Constant(true), Expression.Constant(true));

    //Act
    var query = ClassUnderTest.BuildKSql(andAlso);

    //Assert
    query.Should().BeEquivalentTo("True AND True");
  }

  [Test]
  public void BinaryOr_BuildKSql_PrintsOperatorOr()
  {
    //Arrange
    var orElse = Expression.OrElse(Expression.Constant(true), Expression.Constant(false));

    //Act
    var query = ClassUnderTest.BuildKSql(orElse);

    //Assert
    query.Should().BeEquivalentTo("True OR False");
  }

  [Test]
  public void BinaryEqual_BuildKSql_PrintsEqual()
  {
    //Arrange
    ConstantExpression constant = Expression.Constant(1);
    var expression = Expression.Equal(constant, constant);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("1 = 1");
  }

  [Test]
  public void BinaryNotEqual_BuildKSql_PrintsNotEqual()
  {
    //Arrange
    ConstantExpression constant1 = Expression.Constant(1);
    ConstantExpression constant2 = Expression.Constant(2);
    var expression = Expression.NotEqual(constant1, constant2);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("1 != 2");
  }

  [Test]
  public void BinaryLessThan_BuildKSql_PrintsLessThan()
  {
    //Arrange
    ConstantExpression constant1 = Expression.Constant(1);
    ConstantExpression constant2 = Expression.Constant(2);
    var expression = Expression.LessThan(constant1, constant2);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("1 < 2");
  }

  [Test]
  public void BinaryLessThanOrEqual_BuildKSql_PrintsLessThanOrEqual()
  {
    //Arrange
    ConstantExpression constant1 = Expression.Constant(1);
    ConstantExpression constant2 = Expression.Constant(2);
    var expression = Expression.LessThanOrEqual(constant1, constant2);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("1 <= 2");
  }

  [Test]
  public void BinaryGreaterThan_BuildKSql_PrintsGreaterThan()
  {
    //Arrange
    ConstantExpression constant1 = Expression.Constant(2);
    ConstantExpression constant2 = Expression.Constant(1);
    var expression = Expression.GreaterThan(constant1, constant2);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("2 > 1");
  }

  [Test]
  public void BinaryGreaterThanOrEqual_BuildKSql_PrintsGreaterThanOrEqual()
  {
    //Arrange
    ConstantExpression constant1 = Expression.Constant(2);
    ConstantExpression constant2 = Expression.Constant(1);
    var expression = Expression.GreaterThanOrEqual(constant1, constant2);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("2 >= 1");
  }

  #endregion

  #region New

  [Test]
  public void NewAnonymousType_BuildKSql_PrintsMemberName()
  {
    //Arrange
    Expression<Func<Location, object>> expression = l => new { l.Longitude };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo(nameof(Location.Longitude));
  }

  [Test]
  public void NewAnonymousTypeMultipleMembers_BuildKSql_PrintsAllCommaSeparatedMemberNames()
  {
    //Arrange
    Expression<Func<Location, object>> expression = l => new { l.Longitude, l.Latitude };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(Location.Longitude)}, {nameof(Location.Latitude)}");
  }

  [Test]
  public void NewAnonymousTypeMultipleMembersOneHasAlias_BuildKSql_PrintsAllCommaSeparatedMemberNames()
  {
    //Arrange
    Expression<Func<Location, object>> expression = l => new { l.Longitude, La = l.Latitude };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(Location.Longitude)}, {nameof(Location.Latitude)} AS La");
  }

  [Test]
  public void NewReferenceType_BuildKSql_PrintsStruct()
  {
    //Arrange
    Expression<Func<Location, object>> expression = l => new Location();

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEmpty();
  }

  [Test]
  public void NewMemberInit_BuildKSql_PrintsStruct()
  {
    //Arrange
    Expression<Func<Location, object>> expression = l => new Location { Latitude = "t" };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().Be("STRUCT(Latitude := 't')");
  }

  [Test]
  public void NewMemberInitMemberAccess_BuildKSql_PrintsStructMemberAccess()
  {
    //Arrange
    Expression<Func<Location, object>> expression = l => new Location { Latitude = "t" }.Latitude;

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().Be("STRUCT(Latitude := 't')->Latitude");
  }

  #endregion

  #region MemberAccess

  [Test]
  public void MemberAccess_BuildKSql_PrintsNameOfTheProperty()
  {
    //Arrange
    Expression<Func<Location, double>> predicate = l => l.Longitude;

    //Act
    var query = ClassUnderTest.BuildKSql(predicate);

    //Assert
    query.Should().BeEquivalentTo(nameof(Location.Longitude));
  }

  [Test]
  public void Predicate_BuildKSql_PrintsOperatorAndOperands()
  {
    //Arrange
    Expression<Func<Location, bool>> predicate = l => l.Latitude != "ahoj svet";

    //Act
    var query = ClassUnderTest.BuildKSql(predicate);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(Location.Latitude)} != 'ahoj svet'");
  }

  private record Update
  {
    public string ExtraField = null!;
  }

  [Test]
  public void Field_BuildKSql_PrintsFieldName()
  {
    //Arrange
    Expression<Func<Update, string>> predicate = l => l.ExtraField;

    //Act
    var query = ClassUnderTest.BuildKSql(predicate);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(Update.ExtraField)}");
  }

  [Test]
  public void PredicateCompareWithVariable_BuildKSql_PrintsOperatorAndOperands()
  {
    //Arrange
    string value = "ahoj svet";

    Expression<Func<Location, bool>> predicate = l => l.Latitude != value;

    //Act
    var query = ClassUnderTest.BuildKSql(predicate);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(Location.Latitude)} != '{value}'");
  }

  [Test]
  public void PredicateCompareWithDouble_BuildKSql_PrintsOperatorAndOperands()
  {
    //Arrange
    Expression<Func<Location, bool>> predicate = l => l.Longitude == 1.2;

    KSqlDBContextOptions.NumberFormatInfo = new System.Globalization.NumberFormatInfo
    {
      NumberDecimalSeparator = "."
    };

    //Act
    var query = ClassUnderTest.BuildKSql(predicate);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(Location.Longitude)} = 1.2");
  }

  record DatabaseChangeObject<TEntity>
  {
    public TEntity Before { get; set; } = default!;
    public TEntity After { get; set; } = default!;
  }

  record IoTSensor
  {
    public string SensorId { get; set; } = null!;
    public Model Model { get; set; } = null!;
  }

  record Model
  {
    public string Version { get; set; } = null!;
    public string[] Capabilities { get; set; } = null!;
  }

  [Test]
  public void PredicateNestedProperty_BuildKSql_PrintsDestructuredField()
  {
    //Arrange
    Expression<Func<DatabaseChangeObject<IoTSensor>, bool>> predicate = l => l.After.SensorId == "sensor-42";

    //Act
    var query = ClassUnderTest.BuildKSql(predicate);

    //Assert
    query.Should().BeEquivalentTo("After->SensorId = 'sensor-42'");
  }

  [Test]
  public void PredicateDeeplyNestedProperty_BuildKSql_PrintsDestructuredField()
  {
    //Arrange
    Expression<Func<DatabaseChangeObject<IoTSensor>, bool>> predicate = l => l.After.Model.Version == "v-42";

    //Act
    var query = ClassUnderTest.BuildKSql(predicate);

    //Assert
    query.Should().BeEquivalentTo("After->Model->Version = 'v-42'");
  }

  [Test]
  public void PredicateDeeplyNestedArrayProperty_BuildKSql_PrintsAllFields()
  {
    //Arrange
    Expression<Func<DatabaseChangeObject<IoTSensor>, bool>> predicate = l => l.After.Model.Capabilities.Length > 0;

    //Act
    var query = ClassUnderTest.BuildKSql(predicate);

    //Assert
    query.Should().BeEquivalentTo("ARRAY_LENGTH(After->Model->Capabilities) > 0");
  }

  #endregion

  #region Generics

  public interface IIdentifiable
  {
    public Guid Id { get; }
  }

  [Test]
  public void GenericType_Convert_AccessProperty()
  {
    //Arrange
    Guid uniqueId = Guid.NewGuid();
    Expression<Func<IIdentifiable, bool>> predicate = GetUniqueIdExpression<IIdentifiable>(uniqueId);

    //Act
    var query = ClassUnderTest.BuildKSql(predicate);
    
    //Assert
    query.Should().BeEquivalentTo($"Id = '{uniqueId}'");
  }

  private static Expression<Func<T, bool>> GetUniqueIdExpression<T>(Guid uniqueId)
    where T : IIdentifiable
  {
    return c => c.Id == uniqueId;
  }

  [Test]
  public void GenericType_Convert_AccessNestedProperty()
  {
    //Arrange
    Expression<Func<IWrapper, bool>> predicate = CompareNestedProperty<IWrapper>();

    //Act
    var query = ClassUnderTest.BuildKSql(predicate);

    //Assert
    query.Should().BeEquivalentTo("Nested->SensorId = 'v-42'");
  }

  interface IWrapper
  {
    public IoTSensor Nested { get; set; }
  }

  private static Expression<Func<T, bool>> CompareNestedProperty<T>()
    where T : IWrapper
  {
    return l => l.Nested.SensorId == "v-42";
  }

  #endregion

  #region Parameter

  [Test]
  public void Parameter_BuildKSql_PrintsParameterName()
  {
    //Arrange
    var expression = Expression.Parameter(typeof(int), "param");

    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().BeEmpty();
  }

  #endregion

  #region Convert

  [Test]
  public void Convert_BuildKSql_PrintsParameterName()
  {
    //Arrange
    Expression<Func<Tweet, object>> expression = t => t.RowTime >= 1;

    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be("RowTime >= 1");
  }

  #endregion

  #region Lambda

  [Test]
  public void LambdaWithNewCount_BuildKSql_PrintsKeyAndCountAsterix()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Location>, object>> expression = l => new { Key = l.Key, Agg = l.Count() };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("Key, Count(*) Agg");
  }

  [Test]
  public void LambdaWithNewLongCount_BuildKSql_PrintsKeyAndCountAsterix()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Location>, object>> expression = l => new { Key = l.Key, Agg = l.LongCount() };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("Key, Count(*) Agg");
  }

  [Test]
  public void LambdaWithNewCount_BuildKSql_PrintsKeyAndCountColumnName()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Location>, object>> expression = l => new { l.Key, Agg = l.Count(x => x.Longitude) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, Count({nameof(Location.Longitude)}) Agg");
  }

  [Test]
  public void LambdaWithNewSum_BuildKSql_PrintsKeyAndSumColumnName()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Location>, object>> expression = l => new { l.Key, Agg = l.Sum(x => x.Longitude) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("Key, Sum(Longitude) Agg");
  }

  #endregion

  #region String functions

  #region Case

  [Test]
  public void ToUpper_BuildKSql_PrintsUCase()
  {
    //Arrange
    Expression<Func<Tweet, string>> expression = l => l.Message.ToUpper();

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("UCASE(Message)");
  }

  [Test]
  public void ToUpperInCondition_BuildKSql_PrintsUCase()
  {
    //Arrange
    Expression<Func<Tweet, bool>> expression = l => l.Message.ToUpper() != "hi";

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("UCASE(Message) != 'hi'");
  }

  [Test]
  public void ToLower_BuildKSql_PrintsLCase()
  {
    //Arrange
    Expression<Func<Tweet, string>> expression = l => l.Message.ToLower();

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("LCASE(Message)");
  }

  [Test]
  public void ToLowerInCondition_BuildKSql_PrintsLCase()
  {
    //Arrange
    Expression<Func<Tweet, bool>> expression = l => l.Message.ToLower() != "hi";

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("LCASE(Message) != 'hi'");
  }

  [Test]
  public void ConstantToLowerInCondition_BuildKSql_PrintsLCase()
  {
    //Arrange
    Expression<Func<Tweet, bool>> expression = l => l.Message.ToLower() != "HI".ToLower();

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("LCASE(Message) != LCASE('HI')");
  }

  #endregion

  #region Length

  [Test]
  public void Length_BuildKSql_PrintsLenFunction()
  {
    //Arrange
    Expression<Func<Tweet, int>> lengthExpression = c => c.Message.Length;

    //Act
    var query = ClassUnderTest.BuildKSql(lengthExpression);

    //Assert
    query.Should().BeEquivalentTo($"LEN({nameof(Tweet.Message)})");
  }

  [Test]
  public void LengthWithPMinusOperator_BuildKSql_PrintsQuery()
  {
    //Arrange
    Expression<Func<Person, int>> lengthExpression = c => c.FirstName.Length - c.LastName.Length;

    //Act
    var query = ClassUnderTest.BuildKSql(lengthExpression);

    //Assert
    query.Should().BeEquivalentTo($"LEN({nameof(Person.FirstName)}) - LEN({nameof(Person.LastName)})");
  }

  [Test]
  public void LengthWithPlusOperator_BuildKSql_PrintsQuery()
  {
    //Arrange
    Expression<Func<Person, int>> lengthExpression = c => c.FirstName.Length + c.LastName.Length;

    //Act
    var query = ClassUnderTest.BuildKSql(lengthExpression);

    //Assert
    query.Should().BeEquivalentTo($"LEN({nameof(Person.FirstName)}) + LEN({nameof(Person.LastName)})");
  }

  [Test]
  public void LengthWithPlusNew_BuildKSql_PrintsQuery()
  {
    //Arrange
    Expression<Func<Person, object>> lengthExpression = c => new { NAME_LENGTH = c.FirstName.Length + c.LastName.Length };

    //Act
    var query = ClassUnderTest.BuildKSql(lengthExpression);

    //Assert
    query.Should().BeEquivalentTo($"LEN({nameof(Person.FirstName)}) + LEN({nameof(Person.LastName)}) AS NAME_LENGTH");
  }

  #endregion

  #region Like

  [Test]
  public void StartsWith_BuildKSql_PrintsLike()
  {
    //Arrange
    string text = "ET";
    Expression<Func<Tweet, bool>> expression = c => c.Message.StartsWith("ET");

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(Tweet.Message)} LIKE '{text}%'");
  }

  [Test]
  public void ConstantStartsWith_BuildKSql_PrintsLike()
  {
    //Arrange
    string text = "ET";
    Expression<Func<Tweet, bool>> expression = c => "message".StartsWith(text);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"'message' LIKE '{text}%'");
  }

  [Test]
  public void EndsWith_BuildKSql_PrintsLike()
  {
    //Arrange
    string text = "ET";
    Expression<Func<Tweet, bool>> expression = c => c.Message.EndsWith(text);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(Tweet.Message)} LIKE '%{text}'");
  }

  [Test]
  public void Contains_BuildKSql_PrintsLike()
  {
    //Arrange
    string text = "ET";
    Expression<Func<Tweet, bool>> expression = c => c.Message.Contains(text);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(Tweet.Message)} LIKE '%{text}%'");
  }

  [Test]
  public void ContainsGuid_BuildKSql_PrintsLike()
  {
    //Arrange
    var guid1 = Guid.NewGuid();
    var guid2 = Guid.NewGuid();
    var guids = new List<Guid> { guid1, guid2 };

    Expression<Func<Tweet, bool>> expression = c => guids.Contains(guid1);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"'{guid1}' IN ('{guid1}', '{guid2}')");
  }

  [Test]
  public void ContainsString_BuildKSql_PrintsLike()
  {
    //Arrange
    var guids = new List<string> { "one", "two"};

    Expression<Func<Tweet, bool>> expression = c => guids.Contains("one");

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("'one' IN ('one', 'two')");
  }

  #endregion

  #endregion

  #region Arithmetic

  [Test]
  public void Divide_BuildKSql_PrintsQuery()
  {
    //Arrange
    Expression<Func<Person, object>> expression = c => c.FirstName.Length / c.LastName.Length;

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"LEN({nameof(Person.FirstName)}) / LEN({nameof(Person.LastName)})");
  }

  [Test]
  public void Multiply_BuildKSql_PrintsQuery()
  {
    //Arrange
    Expression<Func<Person, object>> expression = c => c.FirstName.Length * c.LastName.Length;

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"LEN({nameof(Person.FirstName)}) * LEN({nameof(Person.LastName)})");
  }

  [Test]
  public void Modulo_BuildKSql_PrintsQuery()
  {
    //Arrange
    Expression<Func<Person, object>> expression = c => c.FirstName.Length % c.LastName.Length;

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"LEN({nameof(Person.FirstName)}) % LEN({nameof(Person.LastName)})");
  }

  #endregion

  #region Constants

  [Test]
  public void BooleanConstant_BuildKSql_PrintsTrue()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Location>, object>> expression = l => new { Const = true };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("true Const");
  }

  [Test]
  public void BooleanConstant_BuildKSql_PrintsFalse()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Location>, object>> expression = l => false;

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("false");
  }

  [Test]
  [Ignore("TODO")]
  public void NegateBooleanConstant_BuildKSql_PrintsFalse()
  {
    //Arrange
    Expression<Func<Tweet, bool>> expression = l => !true;

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("NOT true");
  }

  [Test]
  public void NegateBooleanColumn_BuildKSql_PrintsFalse()
  {
    //Arrange
    Expression<Func<Tweet, bool>> expression = l => !l.IsRobot;

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("NOT IsRobot");
  }

  #endregion

  #region ListContains

  class OrderData
  {
    public int OrderType { get; set; }
    public string Category { get; set; } = null!;
  }

  [Test]
  public void ListContains()
  {
    //Arrange
    Expression<Func<OrderData, bool>> expression = o => new List<int> { 1, 3 }.Contains(o.OrderType);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(OrderData.OrderType)} IN (1, 3)");
  }

  [Test]
  public void ListMemberContains()
  {
    //Arrange
    var orderTypes = new List<int> { 1, 3 };

    Expression<Func<OrderData, bool>> expression = o => orderTypes.Contains(o.OrderType);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(OrderData.OrderType)} IN (1, 3)");
  }

  [Test]
  public void VisitNewArrayContains()
  {
    //Arrange
    Expression<Func<OrderData, bool>> expression = o => new []{ 1, 3 }.Contains(o.OrderType);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(OrderData.OrderType)} IN (1, 3)");
  }

  [Test]
  public void EnumerableContains()
  {
    //Arrange
    IEnumerable<int> orderTypes = Enumerable.Range(1, 3);

    Expression<Func<OrderData, bool>> expression = o => orderTypes.Contains(o.OrderType);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(OrderData.OrderType)} IN (1, 2, 3)");
  }

  [Test]
  public void EnumerableOfStringContains()
  {
    //Arrange
    IEnumerable<string> orderTypes = new [] { "1", "2" };

    Expression<Func<OrderData, bool>> expression = o => orderTypes.Contains(o.Category);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(OrderData.Category)} IN ('1', '2')");
  }

  [Test]
  public void IListMemberContains()
  {
    //Arrange
    IList<int> orderTypes = new List<int> { 1, 3 };

    Expression<Func<OrderData, bool>> expression = o => orderTypes.Contains(o.OrderType);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(OrderData.OrderType)} IN (1, 3)");
  }

  [Test]
  public void IEnumerableMemberContains()
  {
    //Arrange
    IEnumerable<int> orderTypes = new List<int> { 1, 3 };

    Expression<Func<OrderData, bool>> expression = o => orderTypes.Contains(o.OrderType);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(OrderData.OrderType)} IN (1, 3)");
  }

  #endregion

  [Test]
  public void ArrayContains_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<OrderData, bool>> expression = c => K.Functions.ArrayContains(new[]{ 1, 3 }, c.OrderType);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ARRAY_CONTAINS(ARRAY[1, 3], {nameof(OrderData.OrderType)})");
  }

  [Test]
  public void ArrayMemberContains_BuildKSql_PrintsFunction()
  {
    //Arrange
    var orderTypes = new[] { 1, 3 };
    Expression<Func<OrderData, bool>> expression = c => K.Functions.ArrayContains(orderTypes, c.OrderType);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ARRAY_CONTAINS(ARRAY[1, 3], {nameof(OrderData.OrderType)})");
  }

  [Test]
  public void Conditional()
  {
    //Arrange
    Expression<Func<Location, string>> expression = c => c.Longitude < 4.1 ? "left" : "right";

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($" WHEN {nameof(Location.Longitude)} < 4.1 THEN 'left' ELSE 'right'");
  }
}
