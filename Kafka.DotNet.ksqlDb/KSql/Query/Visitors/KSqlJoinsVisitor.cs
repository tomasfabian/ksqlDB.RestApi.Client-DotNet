using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Pluralize.NET;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Visitors
{
  internal class KSqlJoinsVisitor : KSqlVisitor
  {
    private readonly KSqlDBContextOptions contextOptions;
    private readonly QueryContext queryContext;

    public KSqlJoinsVisitor(StringBuilder stringBuilder, KSqlDBContextOptions contextOptions, QueryContext queryContext)
      : base(stringBuilder, useTableAlias: false)
    {
      this.contextOptions = contextOptions ?? throw new ArgumentNullException(nameof(contextOptions));
      this.queryContext = queryContext ?? throw new ArgumentNullException(nameof(queryContext));
    }

    private readonly HashSet<string> aliasHashSet = new();

    private string GenerateAlias(string name)
    {
      var streamAlias = name[0].ToString();

      int i = 0;

      var streamAliasAttempt = streamAlias;

      while (aliasHashSet.Contains(streamAliasAttempt))
      {
        streamAliasAttempt = $"{streamAlias}{++i}";
      }
      
      aliasHashSet.Add(streamAliasAttempt);

      return streamAliasAttempt;
    }

    internal void VisitJoinTable((MethodInfo, IEnumerable<Expression>) join)
    {
      var (methodInfo, e) = join;
      
      var expressions = e.ToArray();

      expressions = expressions.Select(StripQuotes).ToArray();
      
      Visit(expressions[0]);
      var outerStreamAlias = GenerateAlias(queryContext.FromItemName);

      var streamAlias = GenerateAlias(fromItemName);

      var lambdaExpression = expressions[3] as LambdaExpression;
      var rewrittenAliases = PredicateReWriter.Rewrite(lambdaExpression, outerStreamAlias, streamAlias);

      Append("SELECT ");

      new KSqlJoinSelectFieldsVisitor(StringBuilder).Visit(rewrittenAliases);

      AppendLine($" FROM {queryContext.FromItemName} {outerStreamAlias}");

      var joinType = methodInfo.Name switch
      {
        nameof(QbservableExtensions.Join) => "INNER",
        nameof(QbservableExtensions.LeftJoin) => "LEFT",
        nameof(QbservableExtensions.FullOuterJoin) => "FULL OUTER",
        _ => throw new ArgumentOutOfRangeException()
      };

      AppendLine($"{joinType} JOIN {fromItemName} {streamAlias}");
      Append($"ON {outerStreamAlias}.");
      Visit(expressions[1]);
      Append($" = {streamAlias}.");
      Visit(expressions[2]);
      AppendLine(string.Empty);
    }
    
    private string fromItemName;
    
    private static readonly IPluralize EnglishPluralizationService = new Pluralizer();

    protected virtual string InterceptFromItemName(string value)
    {
      if (contextOptions.ShouldPluralizeFromItemName)
        return EnglishPluralizationService.Pluralize(value);

      return value;
    }

    protected override Expression VisitConstant(ConstantExpression constantExpression)
    {
      if (constantExpression == null) throw new ArgumentNullException(nameof(constantExpression));

      if (constantExpression.Value is ISource source)
      {
        fromItemName = constantExpression.Type.GenericTypeArguments[0].Name;

        fromItemName = source?.QueryContext?.FromItemName ?? fromItemName;

        fromItemName = InterceptFromItemName(fromItemName);
      }

      return constantExpression;
    }
  }
}