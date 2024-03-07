namespace ksqlDb.RestApi.Client.Tests.Helpers.Http;

internal class IsDisposedHttpClient(HttpMessageHandler handler) : HttpClient(handler)
{
  public bool IsDisposed { get; private set; }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);

    IsDisposed = true;
  }
}
