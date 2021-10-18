using System;
using ksqlDB.Api.Client.Samples.Models;

namespace ksqlDB.Api.Client.Samples.Observers
{
  public class TweetsObserver : IObserver<Tweet>
  {
    public void OnNext(Tweet tweetMessage)
    {
      Console.WriteLine($"{nameof(Tweet)}: {tweetMessage.Id} - {tweetMessage.Message}");
    }

    public void OnError(Exception error)
    {
      Console.WriteLine($"{nameof(Tweet)}: {error.Message}");
    }

    public void OnCompleted()
    {
      Console.WriteLine($"{nameof(Tweet)}: completed successfully");
    }
  }
}