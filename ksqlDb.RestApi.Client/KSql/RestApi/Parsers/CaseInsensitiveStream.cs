using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace ksqlDb.RestApi.Client.KSql.RestApi.Parsers
{
  internal class CaseInsensitiveStream : ICharStream
  {
    private readonly ICharStream stream;

    public CaseInsensitiveStream(ICharStream stream)
    {
      this.stream = stream;
    }

    public void Consume()
    {
      stream.Consume();
    }

    public int LA(int i)
    {
      var result = stream.LA(i);

      switch (result)
      {
        case 0:
        case SqlBaseParser.Eof:
          return result;
        default:
          return char.ToUpper((char)result);
      }
    }

    public int Mark()
    {
      return stream.Mark();
    }

    public void Release(int marker)
    {
      stream.Release(marker);
    }

    public void Seek(int index)
    {
      stream.Seek(index);
    }

    public int Index
    {
      get => stream.Index;
    }

    public int Size
    {
      get => stream.Size;
    }

    public string SourceName
    {
      get => stream.SourceName;
    }

    public string GetText(Interval interval)
    {
      return stream.GetText(interval);
    }
  }
}
