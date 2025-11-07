using System.Linq.Expressions;
using FluentAssertions;
using ksqlDb.RestApi.Client.KSql.Query.Metadata;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Metadata
{
  public class AnonymousPropertyMapperTests
  {
    private AnonymousPropertyMapper Mapper { get; set; } = null!;

    [SetUp]
    public void TestInitialize()
    {
      Mapper = new AnonymousPropertyMapper(new KSqlQueryMetadata());
    }

    [Test]
    public void AddLambda_NonAnonymousTypesAreNotMapped()
    {
      //Arrange
      Expression<Func<Order, object>> lambda = c => new Order { Id = c.Id, Description = c.Description};

      //Act
      Mapper.AddLambda(lambda);

      //Assert
      Mapper.QueryMetadata.NewAnonymousTypeMappings.Count.Should().Be(0);
    }

    [Test]
    public void AddLambda_AllAnonymousPropertiesAreMapped()
    {
      //Arrange
      Expression<Func<Order, object>> lambda = c => new {c.Id, Description = c.Description };

      //Act
      Mapper.AddLambda(lambda);

      //Assert
      Mapper.QueryMetadata.NewAnonymousTypeMappings.Count.Should().Be(2);
      var mappings = Mapper.QueryMetadata.NewAnonymousTypeMappings[nameof(Order.Id)];
      mappings.Count.Should().Be(1);
      mappings[0].Should().Be(new AnonymousTypeMapping
      {
        PropertyName = nameof(Order.Id),
        DeclaringType = typeof(Order),
        ParameterName = "c",
        ProjectedName = nameof(Order.Id)
      });
      mappings = Mapper.QueryMetadata.NewAnonymousTypeMappings[nameof(Order.Description)];
      mappings[0].Should().Be(new AnonymousTypeMapping
      {
        PropertyName = nameof(Order.Description),
        DeclaringType = typeof(Order),
        ParameterName = "c",
        ProjectedName = nameof(Order.Description)
      });
    }

    [Test]
    public void AddLambda_AllAnonymousPropertiesAreMappedFromMultipleTypes()
    {
      //Arrange
      Expression<Func<Order, OrderItem, object>> lambda = (o, oi) => new {o.Id, OrderItemId = oi.Id, Desc = oi.Description };

      //Act
      Mapper.AddLambda(lambda);

      //Assert
      Mapper.QueryMetadata.NewAnonymousTypeMappings.Count.Should().Be(2);
      var mappings = Mapper.QueryMetadata.NewAnonymousTypeMappings[nameof(Order.Id)];
      mappings.Count.Should().Be(2);
      mappings[0].Should().Be(new AnonymousTypeMapping
      {
        PropertyName = nameof(Order.Id),
        DeclaringType = typeof(Order),
        ParameterName = "o",
        ProjectedName = nameof(Order.Id)
      });
      mappings[1].Should().Be(new AnonymousTypeMapping
      {
        PropertyName = nameof(OrderItem.Id),
        DeclaringType = typeof(OrderItem),
        ParameterName = "oi",
        ProjectedName = "OrderItemId"
      });

      mappings = Mapper.QueryMetadata.NewAnonymousTypeMappings[nameof(OrderItem.Description)];
      mappings.Count.Should().Be(1);
      mappings[0].Should().Be(new AnonymousTypeMapping
      {
        PropertyName = nameof(OrderItem.Description),
        DeclaringType = typeof(OrderItem),
        ParameterName = "oi",
        ProjectedName = "Desc"
      });
    }

    [Test]
    public void AddLambda_ProjectedPropertyWasMapped()
    {
      //Arrange
      Expression<Func<Order, object>> lambda = c => new { Desc = c.Description };

      //Act
      Mapper.AddLambda(lambda);

      //Assert
      Mapper.QueryMetadata.NewAnonymousTypeMappings.Count.Should().Be(1);
      var mappings = Mapper.QueryMetadata.NewAnonymousTypeMappings[nameof(Order.Description)];
      mappings.Count.Should().Be(1);
      mappings[0].Should().Be(new AnonymousTypeMapping
      {
        PropertyName = nameof(Order.Description),
        DeclaringType = typeof(Order),
        ParameterName = "c",
        ProjectedName = "Desc"
      });
    }
  }

  internal class Order
  {
    public int Id { get; set; }
    public string Description { get; set; } = null!;
  }

  internal class OrderItem
  {
    public int Id { get; set; }
    public string Description { get; set; } = null!;
  }
}
