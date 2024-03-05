using System.Text;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Parsers;

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
        stringBuilder.Append(',');
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

    bool isInsideString = false;

    char? previousChar = null;

    foreach(var currentChar in row)
    {
      if(structuredTypeStarted.Contains(currentChar) && !isInsideString)
        isStructuredType++;

      if(structuredTypeEnded.Contains(currentChar) && !isInsideString)
        isStructuredType--;

      if (currentChar == '"' && (previousChar == null || previousChar != '\\'))
        isInsideString = !isInsideString;

      if (ShouldAppend(currentChar, isInsideString, isStructuredType))
        stringBuilder.Append(currentChar);

      if (IsRowEnd(currentChar, isInsideString, isStructuredType))
      {
        yield return stringBuilder.ToString();
        stringBuilder.Clear();
      }

      previousChar = currentChar;
    }

    yield return stringBuilder.ToString();
  }

  private static bool ShouldAppend(char currentChar, bool isInsideString, int isStructuredType)
  {
    return currentChar != ',' || isInsideString || isStructuredType > 0;
  }

  private static bool IsRowEnd(char currentChar, bool isInsideString, int isStructuredType)
  {
    return currentChar == ',' && !isInsideString && !(isStructuredType > 0);
  }
}
