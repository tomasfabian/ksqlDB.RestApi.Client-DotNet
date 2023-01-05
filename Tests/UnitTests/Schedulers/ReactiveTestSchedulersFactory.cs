using System.Reactive.Concurrency;
using Joker.Factories.Schedulers;
using Microsoft.Reactive.Testing;

namespace UnitTests.Schedulers;

public class ReactiveTestSchedulersFactory : ISchedulersFactory
{
  private TestScheduler currentThread = null!;

  public TestScheduler CurrentThread => currentThread ??= new TestScheduler();

  private TestScheduler immediate = null!;

  public TestScheduler Immediate => immediate ??= new TestScheduler();

  private TestScheduler newThread = null!;

  public TestScheduler NewThread => newThread ??= new TestScheduler();

  private TestScheduler taskPool = null!;

  public TestScheduler TaskPool => taskPool ??= new TestScheduler();

  private TestScheduler threadPool = null!;

  public TestScheduler ThreadPool => threadPool ??= new TestScheduler();

  public IScheduler EventLoopScheduler => threadPool ??= new TestScheduler();

  public IScheduler NewEventLoopScheduler => threadPool ??= new TestScheduler();

  private TestScheduler dispatcher = null!;

  public TestScheduler Dispatcher => dispatcher ??= new TestScheduler();

  #region ISchedulerProvider Members

  IScheduler ISchedulersFactory.CurrentThread => CurrentThread;

  IScheduler ISchedulersFactory.Immediate => Immediate;

  IScheduler ISchedulersFactory.NewThread => NewThread;

  IScheduler ISchedulersFactory.TaskPool => TaskPool;

  IScheduler ISchedulersFactory.ThreadPool => ThreadPool;

  #endregion
}
