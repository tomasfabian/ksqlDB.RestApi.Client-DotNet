using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Disposables;
using NUnit.Framework;
using UnitTests;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ksqlDb.RestApi.Client.Tests.Disposables;

public class AsyncDisposableObjectTests : TestBase<AsyncDisposableObjectTests.TestableAsyncDisposableObject>
{
  public override void TestInitialize()
  {
    base.TestInitialize();

    ClassUnderTest = new TestableAsyncDisposableObject();
  }

  [Test]
  public async Task InitializeAsync_HasBeenInitialized()
  {
    //Arrange

    //Act
    await ClassUnderTest.InitializeAsync();

    //Assert
    ClassUnderTest.HasBeenInitialized.Should().BeTrue();
  }

  [Test]
  public async Task InitializeAsyncDisposed_Throws()
  {
    //Arrange
    await ClassUnderTest.DisposeAsync().ConfigureAwait(false);


    //Assert
    await Assert.ThrowsExceptionAsync<ObjectDisposedException>(() => ClassUnderTest.InitializeAsync());
  }
    
  [Test]
  public async Task InitializeAsyncDisposed_IsCancellationRequested()
  {
    //Arrange
    ClassUnderTest.ShouldDispose = true;

    //Act
    await ClassUnderTest.InitializeAsync();

    //Assert
    ClassUnderTest.CancellationToken.IsCancellationRequested.Should().BeTrue();
  }

  [Test]
  public async Task MultipleInitializeAsync_InitializedOnce()
  {
    //Arrange
    await ClassUnderTest.InitializeAsync();

    //Act
    await ClassUnderTest.InitializeAsync();

    //Assert
    ClassUnderTest.InitializationCounter.Should().Be(1);
  }

  [Test]
  public async Task DisposeAsync_IsDisposed()
  {
    //Arrange

    //Act
    await ClassUnderTest.DisposeAsync().ConfigureAwait(false);

    //Assert
    ClassUnderTest.IsDisposed.Should().BeTrue();
  }

  public class TestableAsyncDisposableObject : AsyncDisposableObject
  {
    public int InitializationCounter { get; set; }

    public bool ShouldDispose { get; set; }

    public CancellationToken CancellationToken { get; set; }

    protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
    {
      InitializationCounter++;

      CancellationToken = cancellationToken;
        
      if(ShouldDispose)
        await DisposeAsync().ConfigureAwait(false);
        
      await base.OnInitializeAsync(cancellationToken);
    }
  }
}
