using System;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Query;
using Kafka.DotNet.ksqlDB.Tests.Pocos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;
using Location = Kafka.DotNet.ksqlDB.Tests.Models.Location;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.Query
{
  [TestClass]
  public class KSqlVisitorTests : TestBase
  {
    private KSqlVisitor ClassUnderTest { get; set; }

    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      ClassUnderTest = new KSqlVisitor();
    }

    #region Constants

    [TestMethod]
    public void NullConstant_BuildKSql_PrintsStringifiedNull()
    {
      //Arrange
      var constantExpression = Expression.Constant(null);

      //Act
      var query = ClassUnderTest.BuildKSql(constantExpression);

      //Assert
      query.Should().BeEquivalentTo("NULL");
    }

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
    public void EnumerableConstant_BuildKSql_PrintsCommaSeparatedTextFields()
    {
      //Arrange
      var constant = new[] { "Field1", "Field2" };
      var constantExpression = Expression.Constant(constant);

      //Act
      var query = ClassUnderTest.BuildKSql(constantExpression);

      //Assert
      query.Should().BeEquivalentTo("Field1, Field2");
    }

    [TestMethod]
    public void ReferenceTypeConstant_BuildKSql_PrintsCommaSeparatedTextFields()
    {
      //Arrange
      var constant = new object();
      var constantExpression = Expression.Constant(constant);

      //Act
      var query = ClassUnderTest.BuildKSql(constantExpression);

      //Assert
      query.Should().BeEquivalentTo(constant.ToString());
    }

    #endregion

    #region Binary

    [TestMethod]
    public void BinaryAnd_BuildKSql_PrintsOperatorAnd()
    {
      //Arrange
      var andAlso = Expression.AndAlso(Expression.Constant(true), Expression.Constant(true));

      //Act
      var query = ClassUnderTest.BuildKSql(andAlso);

      //Assert
      query.Should().BeEquivalentTo("True AND True");
    }

    [TestMethod]
    public void BinaryOr_BuildKSql_PrintsOperatorOr()
    {
      //Arrange
      var orElse = Expression.OrElse(Expression.Constant(true), Expression.Constant(false));

      //Act
      var query = ClassUnderTest.BuildKSql(orElse);

      //Assert
      query.Should().BeEquivalentTo("True OR False");
    }

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
    public void NewAnonymousType_BuildKSql_PrintsMemberName()
    {
      //Arrange
      Expression<Func<Location, object>> expression = l => new { l.Longitude };

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo(nameof(Location.Longitude));
    }

    [TestMethod]
    public void NewAnonymousTypeMultipleMembers_BuildKSql_PrintsAllCommaSeparatedMemberNames()
    {
      //Arrange
      Expression<Func<Location, object>> expression = l => new { l.Longitude, l.Latitude };

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"{nameof(Location.Longitude)}, {nameof(Location.Latitude)}");
    }

    [TestMethod]
    public void NewAnonymousTypeMultipleMembersOneHasAlias_BuildKSql_PrintsAllCommaSeparatedMemberNames()
    {
      //Arrange
      Expression<Func<Location, object>> expression = l => new { l.Longitude, La = l.Latitude };

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"{nameof(Location.Longitude)}, {nameof(Location.Latitude)} AS La");
    }

    [TestMethod]
    public void NewReferenceType_BuildKSql_PrintsStruct()
    {
      //Arrange
      Expression<Func<Location, object>> expression = l => new Location();

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEmpty();
    }

    [TestMethod]
    public void NewMemberInit_BuildKSql_PrintsStruct()
    {
      //Arrange
      Expression<Func<Location, object>> expression = l => new Location { Latitude = "t" };

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().Be("STRUCT(Latitude := 't')");
    }

    [TestMethod]
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

    [TestMethod]
    public void MemberAccess_BuildKSql_PrintsNameOfTheProperty()
    {
      //Arrange
      Expression<Func<Location, double>> predicate = l => l.Longitude;

      //Act
      var query = ClassUnderTest.BuildKSql(predicate);

      //Assert
      query.Should().BeEquivalentTo(nameof(Location.Longitude));
    }

    [TestMethod]
    public void Predicate_BuildKSql_PrintsOperatorAndOperands()
    {
      //Arrange
      Expression<Func<Location, bool>> predicate = l => l.Latitude != "ahoj svet";

      //Act
      var query = ClassUnderTest.BuildKSql(predicate);

      //Assert
      query.Should().BeEquivalentTo($"{nameof(Location.Latitude)} != 'ahoj svet'");
    }

    [Ignore("Figure out what to do")]
    [TestMethod]
    public void PredicateCompareWithVariable_BuildKSql_PrintsOperatorAndOperands()
    {
      //Arrange
      string value = "ahoj svet";

      Expression<Func<Location, bool>> predicate = l => l.Latitude != value;

      //Act
      var query = ClassUnderTest.BuildKSql(predicate);

      //Assert
      query.Should().BeEquivalentTo($"{nameof(Location.Latitude)} != '${value}'");
    }

    [TestMethod]
    public void PredicateCompareWithDouble_BuildKSql_PrintsOperatorAndOperands()
    {
      //Arrange
      Expression<Func<Location, bool>> predicate = l => l.Longitude == 1.2;

      //Act
      var query = ClassUnderTest.BuildKSql(predicate);

      //Assert
      query.Should().BeEquivalentTo($"{nameof(Location.Longitude)} = 1.2");
    }

    #endregion

    #region Parameter

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
    public void LambdaWithNewCount_BuildKSql_PrintsKeyAndCountAsterix()
    {
      //Arrange
      Expression<Func<IKSqlGrouping<int, Location>, object>> expression = l => new { Key = l.Key, Agg = l.Count() };

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo("Key, Count(*) Agg");
    }

    [TestMethod]
    public void LambdaWithNewLongCount_BuildKSql_PrintsKeyAndCountAsterix()
    {
      //Arrange
      Expression<Func<IKSqlGrouping<int, Location>, object>> expression = l => new { Key = l.Key, Agg = l.LongCount() };

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo("Key, Count(*) Agg");
    }

    [TestMethod]
    public void LambdaWithNewCount_BuildKSql_PrintsKeyAndCountColumnName()
    {
      //Arrange
      Expression<Func<IKSqlGrouping<int, Location>, object>> expression = l => new { l.Key, Agg = l.Count(x => x.Longitude) };

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"Key, Count({nameof(Location.Longitude)}) Agg");
    }

    [TestMethod]
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

    [TestMethod]
    public void ToUpper_BuildKSql_PrintsUCase()
    {
      //Arrange
      Expression<Func<Tweet, string>> expression = l => l.Message.ToUpper();

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo("UCASE(Message)");
    }

    [TestMethod]
    public void ToUpperInCondition_BuildKSql_PrintsUCase()
    {
      //Arrange
      Expression<Func<Tweet, bool>> expression = l => l.Message.ToUpper() != "hi";

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo("UCASE(Message) != 'hi'");
    }

    [TestMethod]
    public void ToLower_BuildKSql_PrintsLCase()
    {
      //Arrange
      Expression<Func<Tweet, string>> expression = l => l.Message.ToLower();

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo("LCASE(Message)");
    }

    [TestMethod]
    public void ToLowerInCondition_BuildKSql_PrintsLCase()
    {
      //Arrange
      Expression<Func<Tweet, bool>> expression = l => l.Message.ToLower() != "hi";

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo("LCASE(Message) != 'hi'");
    }

    [TestMethod]
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

    #region Lenght

    [TestMethod]
    public void Lenght_BuildKSql_PrintsLenFunction()
    {
      //Arrange
      Expression<Func<Tweet, int>> lengthExpression = c => c.Message.Length;

      //Act
      var query = ClassUnderTest.BuildKSql(lengthExpression);

      //Assert
      query.Should().BeEquivalentTo($"LEN({nameof(Tweet.Message)})");
    }

    [TestMethod]
    public void LengthWithPMinusOperator_BuildKSql_PrintsQuery()
    {
      //Arrange
      Expression<Func<Person, int>> lengthExpression = c => c.FirstName.Length - c.LastName.Length;

      //Act
      var query = ClassUnderTest.BuildKSql(lengthExpression);

      //Assert
      query.Should().BeEquivalentTo($"LEN({nameof(Person.FirstName)}) - LEN({nameof(Person.LastName)})");
    }

    [TestMethod]
    public void LengthWithPlusOperator_BuildKSql_PrintsQuery()
    {
      //Arrange
      Expression<Func<Person, int>> lengthExpression = c => c.FirstName.Length + c.LastName.Length;

      //Act
      var query = ClassUnderTest.BuildKSql(lengthExpression);

      //Assert
      query.Should().BeEquivalentTo($"LEN({nameof(Person.FirstName)}) + LEN({nameof(Person.LastName)})");
    }

    [TestMethod]
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

    #endregion

    #region Arithmetic
    
    [TestMethod]
    public void Divide_BuildKSql_PrintsQuery()
    {
      //Arrange
      Expression<Func<Person, object>> expression = c => c.FirstName.Length / c.LastName.Length;

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"LEN({nameof(Person.FirstName)}) / LEN({nameof(Person.LastName)})");
    }
    
    [TestMethod]
    public void Multiply_BuildKSql_PrintsQuery()
    {
      //Arrange
      Expression<Func<Person, object>> expression = c => c.FirstName.Length * c.LastName.Length;

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"LEN({nameof(Person.FirstName)}) * LEN({nameof(Person.LastName)})");
    }
    
    [TestMethod]
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

    [TestMethod]
    public void BooleanConstant_BuildKSql_PrintsTrue()
    {
      //Arrange
      Expression<Func<IKSqlGrouping<int, Location>, object>> expression = l => new { Const = true };

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo("true Const");
    }

    [TestMethod]
    public void BooleanConstant_BuildKSql_PrintsFalse()
    {
      //Arrange
      Expression<Func<IKSqlGrouping<int, Location>, object>> expression = l => false;

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo("false");
    }

    [TestMethod]
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

    [TestMethod]
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
  }
}