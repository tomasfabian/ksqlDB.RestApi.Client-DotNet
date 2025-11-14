using FluentAssertions;
using ksqlDb.RestApi.Client.KSql.Linq;
using NUnit.Framework;
using UnitTests;

namespace ksqlDb.RestApi.Client.Tests.KSql.Linq;

public class AsyncEnumerableTests : TestBase
{
  [Test]
  public void ToObservable_WhenSourceIsNull_ShouldThrow()
  {
    // Arrange
    IAsyncEnumerable<int>? source = null;

    // Act
    Action act = () => source!.ToObservable();

    // Assert
    act.Should().Throw<ArgumentNullException>()
      .WithParameterName("source");
  }

  [Test]
  public async Task ToObservable_ShouldEmitValuesAndComplete()
  {
    // Arrange
    var source = GetAsyncEnumerable([1, 2, 3]);

    var values = new List<int>();
    Exception? error = null;
    bool completed = false;

    var observable = source.ToObservable();

    // Act
    var subscription = observable.Subscribe(
      onNext: v => values.Add(v),
      onError: ex => error = ex,
      onCompleted: () => completed = true
    );

    // Wait for async push
    await Task.Delay(50);

    // Assert
    error.Should().BeNull();
    completed.Should().BeTrue();
    values.Should().BeEquivalentTo([1, 2, 3], options => options.WithStrictOrdering());

    subscription.Dispose();
  }

  [Test]
  public async Task ToObservable_WhenMoveNextThrows_ShouldProduceError()
  {
    // Arrange
    var source = ThrowingAsyncEnumerable();

    Exception? error = null;
    var observable = source.ToObservable();

    // Act
    var subscription = observable.Subscribe(
      _ => { },
      ex => error = ex,
      () => { }
    );

    await Task.Delay(50);

    // Assert
    error.Should().NotBeNull();
    error.Should().BeOfType<InvalidOperationException>();

    subscription.Dispose();
  }

  private static async IAsyncEnumerable<int> GetAsyncEnumerable(IEnumerable<int> items)
  {
    foreach (var item in items)
    {
      await Task.Yield();
      yield return item;
    }
  }

  private static async IAsyncEnumerable<int> ThrowingAsyncEnumerable()
  {
    await Task.Yield();
    throw new InvalidOperationException("boom");
#pragma warning disable CS0162
    yield return 0;
#pragma warning restore CS0162
  }
}
