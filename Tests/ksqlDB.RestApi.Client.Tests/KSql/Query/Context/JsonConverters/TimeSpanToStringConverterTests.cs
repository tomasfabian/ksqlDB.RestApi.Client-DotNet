using System.Text.Json;
using FluentAssertions;
using ksqlDB.Api.Client.Tests.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Context.JsonConverters;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using NUnit.Framework;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Context.JsonConverters;

public class TimeSpanToStringConverterTests
{
  [Test]
  public void Read()
  {
    //Arrange
    var value = new CreateEntityTests.TimeTypes
    {
      Dt = new DateTime(2021, 4, 1),
      Ts = new TimeSpan(1, 2, 3)
    };

    JsonSerializerOptions jsonSerializerOptions = new()
    {
      PropertyNameCaseInsensitive = true
    };

    jsonSerializerOptions.Converters.Add(new TimeSpanToStringConverter());

    var json = JsonSerializer.Serialize(value, jsonSerializerOptions);

    //Act
    var timeSpan = JsonSerializer.Deserialize<CreateEntityTests.TimeTypes>(json, jsonSerializerOptions);

    //Assert
    timeSpan!.Ts.Should().Be(value.Ts);
    timeSpan.Dt.Should().Be(value.Dt);
  }

  [Test]
  public void Write()
  {
    //Arrange
    var converter = new TimeSpanToStringConverter();

    using var stream = new MemoryStream();
    using var writer = new Utf8JsonWriter(stream);

    var options = KSqlDbJsonSerializerOptions.CreateInstance();

    var timeSpan = new TimeSpan(1, 1, 11);

    //Act
    converter.Write(writer, timeSpan, options);
    writer.Flush();

    //Assert
    stream.ToArray().Length.Should().NotBe(0);
  }
}
