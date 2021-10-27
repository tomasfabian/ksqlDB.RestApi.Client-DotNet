using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using Pluralize.NET;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors
{
  internal class KSqlJoinsVisitor : KSqlVisitor
  {
    private readonly KSqlDBContextOptions contextOptions;
    private readonly QueryContext queryContext;
    private readonly Type type;

    public KSqlJoinsVisitor(StringBuilder stringBuilder, KSqlDBContextOptions contextOptions, QueryContext queryContext, Type type)
      : base(stringBuilder, useTableAlias: false)
    {
      this.contextOptions = contextOptions ?? throw new ArgumentNullException(nameof(contextOptions));
      this.queryContext = queryContext ?? throw new ArgumentNullException(nameof(queryContext));
      this.type = type ?? throw new ArgumentNullException(nameof(type));
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

    private PropertyInfo GetPropertyType(Type type)
    {
      var propertyInfo = type.GetProperties()[0];

      bool isAnonymous = propertyInfo.PropertyType.IsAnonymousType();

      if (isAnonymous)
        return GetPropertyType(propertyInfo.PropertyType);

      return propertyInfo;
    }

    private Type[] tableTypes;

    internal void VisitJoinTable(IEnumerable<(MethodInfo, IEnumerable<Expression>, LambdaExpression)> joins)
    {
      bool isFirst = true;

      tableTypes = joins.Select(c => c.Item2.ToArray()[0].Type.GenericTypeArguments[0]).Append(type).ToArray();

      foreach (var join in joins)
      {
        var (methodInfo, e, groupJoin) = join;

        var expressions = e.ToArray();
      
        expressions = expressions.Select(StripQuotes).ToArray();

        Visit(expressions[0]);
        var outerStreamAlias = GenerateAlias(queryContext.FromItemName);

        var streamAlias = GenerateAlias(fromItemName);

        if (groupJoin != null)
        {
          var prop = GetPropertyType(groupJoin.Parameters[0].Type);

          outerStreamAlias = prop.Name;

          streamAlias = groupJoin.Parameters[1].Name;
        }

        var lambdaExpression = expressions[3] as LambdaExpression;

        if (isFirst)
        {
          isFirst = false;

          Append("SELECT ");

          if (groupJoin != null)
          {
            new KSqlTransparentIdentifierJoinSelectFieldsVisitor(StringBuilder, tableTypes).Visit(groupJoin.Body);
          }
          else
          {
            var rewrittenAliases = PredicateReWriter.Rewrite(lambdaExpression, outerStreamAlias, streamAlias);

            new KSqlJoinSelectFieldsVisitor(StringBuilder).Visit(rewrittenAliases);
          }

          AppendLine($" FROM {queryContext.FromItemName} {outerStreamAlias}");
        }

        var joinType = methodInfo.Name switch
        {
          nameof(QbservableExtensions.Join) => "INNER",
          nameof(QbservableExtensions.LeftJoin) => "LEFT",
          nameof(QbservableExtensions.GroupJoin) => "LEFT",
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
    }

    private string fromItemName;

    private static readonly IPluralize EnglishPluralizationService = new Pluralizer();

    protected virtual string InterceptFromItemName(string value)
    {
      if (contextOptions.ShouldPluralizeFromItemName)
        return EnglishPluralizationService.Pluralize(value);

      return value;
    }

    protected override Expression VisitMember(MemberExpression memberExpression)
    {
      if (memberExpression == null) throw new ArgumentNullException(nameof(memberExpression));

      if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
      {
        Append(memberExpression.Member.Name);

        return memberExpression;
      }

      bool isTableType = tableTypes.Contains(memberExpression.Member.DeclaringType);

      if (isTableType && memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
      {
        Append(memberExpression.Member.Name);
      }
      else
        base.VisitMember(memberExpression);

      return memberExpression;
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