using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using ksqlDb.RestApi.Client.Tests.Models;
using NUnit.Framework;
using UnitTests;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Visitors;

#pragma warning disable IDE0037

public class AggregationFunctionExpressionTests : TestBase
{
  private AggregationFunctionVisitor ClassUnderTest { get; set; } = null!;
  private StringBuilder StringBuilder { get; set; } = null!;

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    StringBuilder = new StringBuilder();
    ClassUnderTest = new AggregationFunctionVisitor(StringBuilder, new KSqlQueryMetadata());
  }

  [Test]
  public void LongCount_BuildKSql_PrintsCountWithAsterix()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, Count = l.LongCount() };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("Key, COUNT(*) Count");
  }

  [Test]
  public void LongCount_BuildKSql_PrintsCountWithColumn()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, Count = l.LongCount(c => c.Amount) };
      
    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, COUNT({nameof(Transaction.Amount)}) Count");
  }

  [Test]
  public void Count_BuildKSql_PrintsCountWithColumn()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, Count = l.Count(c => c.Amount) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, COUNT({nameof(Transaction.Amount)}) Count");
  }

  [Test]
  public void LongCountDistinct_BuildKSql_PrintsCountDistinctWithColumn()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, CountDistinct = l.LongCountDistinct(c => c.Amount) };
      
    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, COUNT_DISTINCT({nameof(Transaction.Amount)}) CountDistinct");
  }

  [Test]
  public void CountDistinct_BuildKSql_PrintsCountDistinctWithColumn()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, CountDistinct = l.CountDistinct(c => c.Amount) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, COUNT_DISTINCT({nameof(Transaction.Amount)}) CountDistinct");
  }
    
  [Test]
  public void CollectSet_BuildKSql_PrintsCollectSetWithColumn()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, CollectSet = l.CollectSet(c => c.Amount) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, COLLECT_SET({nameof(Transaction.Amount)}) CollectSet");
  }
    
  [Test]
  public void CollectSet_BuildKSql_Map()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, CollectSet = l.CollectSet(c => c.Dictionary) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, COLLECT_SET({nameof(Transaction.Dictionary)}) CollectSet");
  }
    
  [Test]
  public void CollectList_BuildKSql_PrintsCollectListWithColumn()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, CollectList = l.CollectList(c => c.Amount) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, COLLECT_LIST({nameof(Transaction.Amount)}) CollectList");
  }
    
  [Test]
  public void CollectList_BuildKSql_Array()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, CollectList = l.CollectList(c => c.Array) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, COLLECT_LIST({nameof(Transaction.Array)}) CollectList");
  }
    
  [Test]
  public void CollectList_BuildKSql_Map()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, CollectList = l.CollectList(c => c.Dictionary) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, COLLECT_LIST({nameof(Transaction.Dictionary)}) CollectList");
  }

  [Test]
  public void Max_BuildKSql_PrintsMaxWithColumn()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, System.Drawing.Rectangle>, object>> expression = l => new { Key = l.Key, Max = l.Max(c => c.Height) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, MAX({nameof(System.Drawing.Rectangle.Height)}) Max");
  }

  [Test]
  public void Min_BuildKSql_PrintsMinWithColumn()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, Min = l.Min(c => c.Amount) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, MIN({nameof(Transaction.Amount)}) Min");
  }

  [Test]
  public void EarliestByOffset_BuildKSql_PrintsEarliestByOffsetWithColumn()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, EarliestByOffset = l.EarliestByOffset(c => c.Amount) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, EARLIEST_BY_OFFSET({nameof(Transaction.Amount)}, true) EarliestByOffset");
  }

  [Test]
  public void EarliestByOffset_BuildKSql_Map()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, EarliestByOffset = l.EarliestByOffset(c => c.Dictionary) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, EARLIEST_BY_OFFSET({nameof(Transaction.Dictionary)}, true) EarliestByOffset");
  }

  [Test]
  public void EarliestByOffsetN_BuildKSql_PrintsEarliestByOffsetWithColumn()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, EarliestByOffset = l.EarliestByOffset(c => c.Amount, 2) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, EARLIEST_BY_OFFSET({nameof(Transaction.Amount)}, 2, true) EarliestByOffset");
  }

  [Test]
  public void EarliestByOffsetAllowNulls_BuildKSql_PrintsEarliestByOffsetAllowNullsWithColumn()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, EarliestByOffsetAllowNulls = l.EarliestByOffsetAllowNulls(c => c.Amount) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, EARLIEST_BY_OFFSET({nameof(Transaction.Amount)}, false) EarliestByOffsetAllowNulls");
  }

  [Test]
  public void EarliestByOffsetAllowNullsN_BuildKSql_PrintsEarliestByOffsetAllowNullsWithColumn()
  {
    //Arrange
    int earliestN = 3;
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, EarliestByOffsetAllowNulls = l.EarliestByOffsetAllowNulls(c => c.Amount, earliestN) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, EARLIEST_BY_OFFSET({nameof(Transaction.Amount)}, {earliestN}, false) EarliestByOffsetAllowNulls");
  }

  [Test]
  public void LatestByOffset_BuildKSql_PrintsLatestByOffsetWithColumn()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, LatestByOffset = l.LatestByOffset(c => c.Amount) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, LATEST_BY_OFFSET({nameof(Transaction.Amount)}, true) LatestByOffset");
  }

  [Test]
  public void LatestByOffset_BuildKSql_Map()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, LatestByOffset = l.LatestByOffset(c => c.Dictionary) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, LATEST_BY_OFFSET({nameof(Transaction.Dictionary)}, true) LatestByOffset");
  }

  [Test]
  public void LatestByOffsetN_BuildKSql_PrintsLatestByOffsetWithColumn()
  {
    //Arrange
    int latestN = 3;
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, LatestByOffset = l.LatestByOffset(c => c.Amount, latestN) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, LATEST_BY_OFFSET({nameof(Transaction.Amount)}, {latestN}, true) LatestByOffset");
  }

  [Test]
  public void LatestByOffsetAllowNulls_BuildKSql_PrintsLatestByOffsetAllowNullsWithColumn()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, LatestByOffsetAllowNulls = l.LatestByOffsetAllowNulls(c => c.Amount) };
      
    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, LATEST_BY_OFFSET({nameof(Transaction.Amount)}, false) LatestByOffsetAllowNulls");
  }

  [Test]
  public void LatestByOffsetAllowNullsN_BuildKSql_PrintsLatestByOffsetAllowNullsWithColumn()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, LatestByOffsetAllowNulls = l.LatestByOffsetAllowNulls(c => c.Amount, 2) };
      
    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, LATEST_BY_OFFSET({nameof(Transaction.Amount)}, 2, false) LatestByOffsetAllowNulls");
  }

  [Test]
  public void TopK_BuildKSql_PrintsTopK()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, TopK = l.TopK(c => c.Amount, 2) };
      
    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, TOPK({nameof(Transaction.Amount)}, 2) TopK");
  }

  [Test]
  public void TopKDistinct_BuildKSql_PrintsTopKDistinct()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, TopKDistinct = l.TopKDistinct(c => c.Amount, 2) };
      
    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, TOPKDISTINCT({nameof(Transaction.Amount)}, 2) TopKDistinct");
  }

  [Test]
  public void TopKDistinctWithVariableInput_BuildKSql_PrintsTopKDistinct()
  {
    //Arrange
    int k = 2;
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, TopKDistinct = l.TopKDistinct(c => c.Amount, k) };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, TOPKDISTINCT({nameof(Transaction.Amount)}, {k}) TopKDistinct");
  }

  [Test]
  public void Histogram_BuildKSql_PrintsAggregation()
  {
    //Arrange
    Expression<Func<IKSqlGrouping<int, Transaction>, object>> expression = l => new { Key = l.Key, Histogram = l.Histogram(c => c.CardNumber) };
      
    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Key, HISTOGRAM({nameof(Transaction.CardNumber)}) Histogram");
  }
}

#pragma warning restore IDE0037
