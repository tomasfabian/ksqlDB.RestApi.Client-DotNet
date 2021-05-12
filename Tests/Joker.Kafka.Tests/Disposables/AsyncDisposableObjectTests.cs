using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.Disposables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace Kafka.DotNet.ksqlDB.Tests.Disposables
{
  [TestClass]
  public class AsyncDisposableObjectTests : TestBase<AsyncDisposableObjectTests.TestableAsyncDisposableObject>
  {
    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      ClassUnderTest = new TestableAsyncDisposableObject();
    }

    [TestMethod]
    public async Task InitializeAsync_HasBeenInitialized()
    {
      //Arrange

      //Act
      await ClassUnderTest.InitializeAsync();

      //Assert
      ClassUnderTest.HasBeenInitialized.Should().BeTrue();
    }

    [TestMethod]
    [ExpectedException(typeof(ObjectDisposedException))]
    public async Task InitializeAsyncDisposed_Throws()
    {
      //Arrange
      await ClassUnderTest.DisposeAsync().ConfigureAwait(false);

      //Act
      await ClassUnderTest.InitializeAsync();

      //Assert
    }
    
    [TestMethod]
    public async Task InitializeAsyncDisposed_IsCancellationRequested()
    {
      //Arrange
      ClassUnderTest.ShouldDispose = true;

      //Act
      await ClassUnderTest.InitializeAsync();

      //Assert
      ClassUnderTest.CancellationToken.IsCancellationRequested.Should().BeTrue();
    }

    [TestMethod]
    public async Task MultipleInitializeAsync_InitializedOnce()
    {
      //Arrange
      await ClassUnderTest.InitializeAsync();

      //Act
      await ClassUnderTest.InitializeAsync();

      //Assert
      ClassUnderTest.InitializationCounter.Should().Be(1);
    }

    [TestMethod]
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
}