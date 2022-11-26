using System.Text;
using System.Text.Json;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Query;

internal class HeaderColumnExtractor
{
  internal IEnumerable<string> GetColumnsFromSchema(string schema) {
    var cols = Split(schema).ToArray();
    int i = 0;
	
    while(i < cols.Length)
    {
      var column = cols[i++];
		
      var sb = new StringBuilder();

      foreach(var ch in column.TrimStart(' ').SkipWhile(c => c == '`'))
      {
        if(ch == '`') {			
          yield return sb.ToString();
          break;
        }
        sb.Append(ch);
      }
    }
  }

  readonly char[] structuredTypeStarted = { '<' };
  readonly char[] structuredTypeEnded = { '>' };

  internal IEnumerable<string> Split(string row)
  {
    var stringBuilder = new StringBuilder();
    var isStructuredType = 0;

    foreach(var ch in row)
    {
      if(structuredTypeStarted.Contains(ch))
        isStructuredType++;
		 
      if(structuredTypeEnded.Contains(ch))
        isStructuredType--;
					
      if(ch != ',' || isStructuredType > 0)
        stringBuilder.Append(ch);

      if(ch == ',' && !(isStructuredType > 0))
      {
        yield return stringBuilder.ToString();
        stringBuilder.Clear();
      }
    }
	
    yield return stringBuilder.ToString();
  }

  internal static string ExtractColumnValues(string rawJson)
  {
    var options = new JsonDocumentOptions
    {
      AllowTrailingCommas = true
    };

    using JsonDocument document = JsonDocument.Parse(rawJson, options);

    var row = document.RootElement.GetProperty("row");
    var columnValues = row.GetProperty("columns").GetRawText();

    columnValues = columnValues.Substring(1, columnValues.Length - 2);

    return columnValues;
  }
}