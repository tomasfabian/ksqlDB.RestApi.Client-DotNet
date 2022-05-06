using System.Net.Http;

namespace ksqlDB.Api.Client.Tests.Helpers.Http
{
  internal class IsDisposedHttpClient : HttpClient
  {
    public IsDisposedHttpClient(HttpMessageHandler handler)
      : base(handler)
    {
      
    }

    public bool IsDisposed { get; private set; }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);

      IsDisposed = true;
    }
  }
}