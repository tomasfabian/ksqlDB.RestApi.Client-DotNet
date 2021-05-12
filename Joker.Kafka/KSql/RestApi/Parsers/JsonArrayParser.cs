using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Parsers
{
  internal class JsonArrayParser
  {
    internal string CreateJson(string[] headerColumns, string row)
    {
      var stringBuilder = new StringBuilder();

      stringBuilder.AppendLine("{");

      bool isFirst = true;

      var rowValues = Split(row);

      foreach (var column in headerColumns.Zip(rowValues.Select(c => c.Trim(' ')), (s, s1) => new { ColumnName = s, Value = s1 }))
      {
        if (!isFirst)
        {
          stringBuilder.Append(",");
        }

        stringBuilder.AppendLine($"\"{column.ColumnName}\": {column.Value}");

        isFirst = false;
      }

      stringBuilder.AppendLine("}");

      return stringBuilder.ToString();
    }

    readonly char[] structuredTypeStarted = { '[', '{' };
    readonly char[] structuredTypeEnded = { ']', '}' };

    private IEnumerable<string> Split(string row)
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
  }
}