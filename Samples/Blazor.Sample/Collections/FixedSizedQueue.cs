using System.Collections;
using System.Collections.Concurrent;

namespace Blazor.Sample.Collections;

public class FixedSizedQueue<T> : IEnumerable<T>
{
  readonly ConcurrentQueue<T> queue = new();

  public int Limit { get; set; } = 5;

  public void Enqueue(T obj)
  {
    queue.Enqueue(obj);

    while (queue.Count > Limit && queue.TryDequeue(out _))
    {
    }
  }

  public IEnumerator<T> GetEnumerator()
  {
    return queue.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return ((IEnumerable)queue).GetEnumerator();
  }
}
