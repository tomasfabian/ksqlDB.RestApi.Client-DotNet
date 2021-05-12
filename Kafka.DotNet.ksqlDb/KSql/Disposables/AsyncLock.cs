using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.DotNet.ksqlDB.KSql.Disposables
{
  sealed class AsyncLock
  {
    private readonly object gate = new();
    private readonly SemaphoreSlim semaphore = new(1, 1);
    private readonly AsyncLocal<int> recursionCount = new();

    public ValueTask<Releaser> LockAsync()
    {
      var shouldAcquire = false;

      lock (gate)
      {
        if (recursionCount.Value == 0)
        {
          shouldAcquire = true;
          recursionCount.Value = 1;
        }
        else
        {
          recursionCount.Value++;
        }
      }

      return shouldAcquire ? new ValueTask<Releaser>(semaphore.WaitAsync().ContinueWith(_ => new Releaser(this))) : new ValueTask<Releaser>(new Releaser(this));
    }

    private void Release()
    {
      lock (gate)
      {
        if (--recursionCount.Value == 0)
        {
          semaphore.Release();
        }
      }
    }

    public struct Releaser : IDisposable
    {
      private readonly AsyncLock parent;

      public Releaser(AsyncLock parent) => this.parent = parent;

      public void Dispose() => parent.Release();
    }
  }
}