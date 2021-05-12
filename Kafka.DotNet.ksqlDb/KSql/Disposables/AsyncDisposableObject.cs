using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.DotNet.ksqlDB.KSql.Disposables
{
  public abstract class AsyncDisposableObject : IAsyncDisposable
  {
    private readonly AsyncLock gate = new();

    public bool IsDisposed => isDisposed;

    public bool HasBeenInitialized { get; private set; }

    private readonly CancellationTokenSource cancellationTokenSource = new();

    public async Task InitializeAsync()
    {
      using (await gate.LockAsync().ConfigureAwait(false))
      {
        if (IsDisposed)
          throw new ObjectDisposedException("Object already disposed.");

        if (!HasBeenInitialized && !cancellationTokenSource.IsCancellationRequested)
        {
          await OnInitializeAsync(cancellationTokenSource.Token);

          HasBeenInitialized = true;
        }
      }
    }

    protected virtual Task OnInitializeAsync(CancellationToken cancellationToken)
    {
      return Task.FromResult(true);
    }

    private bool isDisposed;

    public async ValueTask DisposeAsync()
    {
      cancellationTokenSource.Cancel();

      using (await gate.LockAsync().ConfigureAwait(false))
      {
        if (!isDisposed)
        {
          isDisposed = true;
          await DisposeAsync(true);
          GC.SuppressFinalize(this);
        }
      }
    }

    protected async ValueTask DisposeAsync(bool disposing)
    {
      if (disposing)
      {
        await OnDisposeAsync();
      }
    }

    protected virtual ValueTask OnDisposeAsync()
    {
      return new();
    }
  }
}