using ksqlDb.RestApi.Client.KSql.RestApi.Parsers;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Validation
{
  internal class PseudoColumnValidator
  {
    private readonly string[] allowedPseudoColumnNames =
    [
      "Headers".ToUpper(),
      nameof(SystemColumns.ROWOFFSET).ToUpper(),
      nameof(SystemColumns.ROWPARTITION).ToUpper(),
      nameof(SystemColumns.ROWTIME).ToUpper()
    ];

    internal bool IsValid(string columnName)
    {
      return columnName.ToUpper().IsOneOfFollowing(allowedPseudoColumnNames);
    }
  }
}
