namespace ksqlDb.RestApi.Client.KSql.Linq;

public static class AsyncEnumerable
{
  /// <summary>
  /// Converts an async-enumerable sequence to an observable sequence.
  /// </summary>
  /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
  /// <param name="source">Enumerable sequence to convert to an observable sequence.</param>
  /// <returns>The observable sequence whose elements are pulled from the given enumerable sequence.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
  public static IObservable<TSource> ToObservable<TSource>(this IAsyncEnumerable<TSource> source)
  {
    if (source == null)
      throw new ArgumentNullException(nameof(source));

    return new ToObservableInt<TSource>(source);
  }

  private sealed class ToObservableInt<T>(IAsyncEnumerable<T> source) : IObservable<T>
  {
    public IDisposable Subscribe(IObserver<T> observer)
    {
      var cts = new CancellationTokenSource();

      Core(observer, cts.Token);

      return cts;
    }

    private async void Core(IObserver<T> observer, CancellationToken cancellationToken)
    {
      await using var e = source.GetAsyncEnumerator(cancellationToken);
      do
      {
        bool hasNext;
        var value = default(T)!;

        try
        {
          hasNext = await e.MoveNextAsync().ConfigureAwait(false);
          if (hasNext)
          {
            value = e.Current;
          }
        }
        catch (Exception ex)
        {
          if (!cancellationToken.IsCancellationRequested)
          {
            observer.OnError(ex);
          }

          return;
        }

        if (!hasNext)
        {
          observer.OnCompleted();
          return;
        }

        observer.OnNext(value);
      }
      while (!cancellationToken.IsCancellationRequested);
    }
  }
}
