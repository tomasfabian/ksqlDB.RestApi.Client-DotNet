using System.Reactive.Concurrency;
using Microsoft.Reactive.Testing;
using Ninject.MockingKernel.Moq;
using NUnit.Framework;
using UnitTests.Schedulers;

namespace UnitTests;

public abstract class TestBase<TClassUnderTest> : TestBase
{
  protected TClassUnderTest ClassUnderTest { get; set; } = default!;
}

public abstract class TestBase
{
  protected MoqMockingKernel MockingKernel = null!;
  protected TestScheduler TestScheduler = new();

  protected ReactiveTestSchedulersFactory SchedulersFactory = null!;

  [SetUp]
  public virtual void TestInitialize()
  {
    SchedulersFactory = new ReactiveTestSchedulersFactory();

    MockingKernel = new MoqMockingKernel();

    MockingKernel.Bind<IScheduler>().ToConstant(TestScheduler);
    MockingKernel.Bind<ReactiveTestSchedulersFactory>().ToConstant(SchedulersFactory);
  }

  [TearDown]
  public virtual void TestCleanup()
  {
  }

  #region RunSchedulers

  public void RunSchedulers()
  {
    TestScheduler.Start();
      
    SchedulersFactory.ThreadPool.Start();
    SchedulersFactory.TaskPool.Start();
    SchedulersFactory.Dispatcher.Start();
  }

  #endregion

  #region AdvanceSchedulers

  public void AdvanceSchedulers(long time)
  {
    TestScheduler.AdvanceBy(time);
      
    SchedulersFactory.ThreadPool.AdvanceBy(time);
    SchedulersFactory.TaskPool.AdvanceBy(time);
    SchedulersFactory.Dispatcher.AdvanceBy(time);
  }

  #endregion    
    
  protected void Schedule(TestScheduler testScheduler, TimeSpan timeSpan, Action action)
  {
    testScheduler.Schedule(timeSpan, action);
  }
    
  protected void ScheduleOnTaskPool(TimeSpan timeSpan, Action action)
  {
    Schedule(SchedulersFactory.TaskPool, timeSpan, action);
  }
    
  protected void ScheduleOnThreadPool(TimeSpan timeSpan, Action action)
  {
    Schedule(SchedulersFactory.ThreadPool, timeSpan, action);
  }
}
