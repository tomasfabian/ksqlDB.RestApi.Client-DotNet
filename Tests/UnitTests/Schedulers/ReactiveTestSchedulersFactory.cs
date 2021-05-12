using System.Reactive.Concurrency;
using Joker.Factories.Schedulers;
using Microsoft.Reactive.Testing;

namespace UnitTests.Schedulers
{
  public class ReactiveTestSchedulersFactory : ISchedulersFactory
  {
    private TestScheduler currentThread;

    public TestScheduler CurrentThread => currentThread ?? (currentThread = new TestScheduler());

    private TestScheduler immediate;

    public TestScheduler Immediate => immediate ?? (immediate = new TestScheduler());

    private TestScheduler newThread;

    public TestScheduler NewThread => newThread ?? (newThread = new TestScheduler());

    private TestScheduler taskPool;

    public TestScheduler TaskPool => taskPool ?? (taskPool = new TestScheduler());

    private TestScheduler threadPool;

    public TestScheduler ThreadPool => threadPool ?? (threadPool = new TestScheduler());

    public IScheduler EventLoopScheduler => threadPool ?? (threadPool = new TestScheduler());

    public IScheduler NewEventLoopScheduler => threadPool ?? (threadPool = new TestScheduler());

    private TestScheduler dispatcher;

    public TestScheduler Dispatcher => dispatcher ?? (dispatcher = new TestScheduler());

    #region ISchedulerProvider Members

    IScheduler ISchedulersFactory.CurrentThread => CurrentThread;

    IScheduler ISchedulersFactory.Immediate => Immediate;

    IScheduler ISchedulersFactory.NewThread => NewThread;

    IScheduler ISchedulersFactory.TaskPool => TaskPool;

    IScheduler ISchedulersFactory.ThreadPool => ThreadPool;

    #endregion
  }
}