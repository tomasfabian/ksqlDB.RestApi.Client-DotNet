using System;
using System.Linq.Expressions;
using ksqlDb.RestApi.Client.KSql.Entities;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors
{
  internal sealed record KSqlQueryMetadata
  {
    public Type FromItemType { get; set; }
    public FromItem[] Joins { get; set; }

    internal LambdaExpression Select { get; set; }
  }
}