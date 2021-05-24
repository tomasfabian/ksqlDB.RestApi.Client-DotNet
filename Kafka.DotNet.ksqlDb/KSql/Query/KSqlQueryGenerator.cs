using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Kafka.DotNet.ksqlDB.Infrastructure.Extensions;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Linq.PullQueries;
using Kafka.DotNet.ksqlDB.KSql.Linq.Statements;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.Query.Options;
using Kafka.DotNet.ksqlDB.KSql.Query.Visitors;
using Pluralize.NET;

namespace Kafka.DotNet.ksqlDB.KSql.Query
{
  internal class KSqlQueryGenerator : ExpressionVisitor, IKSqlQueryGenerator
  {
    private readonly KSqlDBContextOptions options;
    private static readonly IPluralize EnglishPluralizationService = new Pluralizer();

    private KSqlVisitor kSqlVisitor = new();

    public bool ShouldEmitChanges { get; set; } = true;

    public KSqlQueryGenerator(KSqlDBContextOptions options)
    {
      this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public string BuildKSql(Expression expression, QueryContext queryContext)
    {
      kSqlVisitor = new KSqlVisitor();
      whereClauses = new Queue<Expression>();
      joinTables = new List<(MethodInfo, IEnumerable<Expression>)>();

      Visit(expression);

      string finalFromItemName = InterceptFromItemName(queryContext.FromItemName ?? fromItemName);

      queryContext.AutoOffsetReset = autoOffsetReset;
    
      if (joinTables.Any())
      {
        var joinsVisitor = new KSqlJoinsVisitor(kSqlVisitor.StringBuilder, options, new QueryContext { FromItemName = finalFromItemName });

        foreach (var joinTable in joinTables)
        {
          joinsVisitor.VisitJoinTable(joinTable);
        }
      }
      else
      {
        kSqlVisitor.Append("SELECT ");

        if (@select != null)
          kSqlVisitor.Visit(@select.Body);
        else
          kSqlVisitor.Append("*");

        kSqlVisitor.Append($" FROM {finalFromItemName}");
      }

      bool isFirst = true;

      foreach (var methodCallExpression in whereClauses)
      {
        if (isFirst)
        {
          kSqlVisitor.AppendLine("");
          kSqlVisitor.Append("WHERE ");

          isFirst = false;
        }
        else
          kSqlVisitor.Append(" AND ");

        kSqlVisitor.Visit(methodCallExpression);
      }

      TryGenerateWindowAggregation();

      if (groupBy != null)
      {
        kSqlVisitor.Append(" GROUP BY ");
        kSqlVisitor.Visit(groupBy.Body);
      }

      if (having != null)
      {
        kSqlVisitor.Append(" HAVING ");
        kSqlVisitor.Visit(having.Body);
      }

      if (partitionBy != null)
      {
        kSqlVisitor.Append(" PARTITION BY ");
        kSqlVisitor.Visit(partitionBy.Body);
      }

      if (ShouldEmitChanges)
        kSqlVisitor.Append(" EMIT CHANGES");

      if (Limit.HasValue)
        kSqlVisitor.Append($" LIMIT {Limit}");

      kSqlVisitor.Append(";");

      return kSqlVisitor.BuildKSql();
    }

    private void TryGenerateWindowAggregation()
    {
      if (windowedBy == null)
        return;

      new KSqlWindowsVisitor(kSqlVisitor.StringBuilder).Visit(windowedBy);
    }

    protected virtual string InterceptFromItemName(string value)
    {
      if (options.ShouldPluralizeFromItemName)
        return EnglishPluralizationService.Pluralize(value);

      return value;
    }

    public override Expression? Visit(Expression? expression)
    {
      if (expression == null)
        return null;

      switch (expression.NodeType)
      {
        case ExpressionType.Constant:
          VisitConstant((ConstantExpression)expression);
          break;
        case ExpressionType.Call:
          VisitMethodCall((MethodCallExpression)expression);
          break;
      }

      return expression;
    }

    private string fromItemName;

    protected override Expression VisitConstant(ConstantExpression constantExpression)
    {
      if (constantExpression == null) throw new ArgumentNullException(nameof(constantExpression));

      var type = constantExpression.Type;

      var kStreamSetType = type.TryFindProviderAncestor();

      if (kStreamSetType != null)
        fromItemName = ((KSet) constantExpression.Value).ElementType.Name;

      return constantExpression;
    }

    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
      var methodInfo = methodCallExpression.Method;

      if(methodInfo.DeclaringType.IsNotOneOfFollowing(typeof(QbservableExtensions), typeof(CreateStatementExtensions), typeof(PullQueryExtensions)))
        return methodCallExpression;

      if (methodInfo.Name.IsOneOfFollowing(nameof(QbservableExtensions.Select), nameof(CreateStatementExtensions.Select)))
      {
        LambdaExpression lambda = (LambdaExpression)StripQuotes(methodCallExpression.Arguments[1]);

        if (select == null)
          select = lambda;

        VisitChained(methodCallExpression);
      }

      if (methodInfo.Name == nameof(CreateStatementExtensions.PartitionBy))
      {
        LambdaExpression lambda = (LambdaExpression)StripQuotes(methodCallExpression.Arguments[1]);

        if (partitionBy == null)
          partitionBy = lambda;

        VisitChained(methodCallExpression);
      }

      if (methodInfo.Name.IsOneOfFollowing(nameof(QbservableExtensions.Where), nameof(CreateStatementExtensions.Where), nameof(PullQueryExtensions.Where)))
      {
        VisitChained(methodCallExpression);

        LambdaExpression lambda = (LambdaExpression)StripQuotes(methodCallExpression.Arguments[1]);
        whereClauses.Enqueue(lambda.Body);
      }

      if (methodInfo.Name.IsOneOfFollowing(nameof(QbservableExtensions.Take), nameof(CreateStatementExtensions.Take)))
      {
        var arg = (ConstantExpression)methodCallExpression.Arguments[1];
        Limit = (int)arg.Value;

        VisitChained(methodCallExpression);
      }

      if (methodInfo.Name.IsOneOfFollowing(nameof(QbservableExtensions.WithOffsetResetPolicy)))
      {
        var arg = (ConstantExpression)methodCallExpression.Arguments[1];
        autoOffsetReset = (AutoOffsetReset)arg.Value;

        VisitChained(methodCallExpression);
      }

      if (methodInfo.Name.IsOneOfFollowing(nameof(QbservableExtensions.WindowedBy), nameof(CreateStatementExtensions.WindowedBy)))
      {
        windowedBy = (ConstantExpression)StripQuotes(methodCallExpression.Arguments[1]);

        VisitChained(methodCallExpression);
      }

      if (methodInfo.Name.IsOneOfFollowing(nameof(QbservableExtensions.GroupBy), nameof(CreateStatementExtensions.GroupBy)))
      {
        groupBy = (LambdaExpression)StripQuotes(methodCallExpression.Arguments[1]);

        VisitChained(methodCallExpression);
      }

      if (methodInfo.Name.IsOneOfFollowing(nameof(QbservableExtensions.Having), nameof(CreateStatementExtensions.Having)))
      {
        having = (LambdaExpression)StripQuotes(methodCallExpression.Arguments[1]);

        VisitChained(methodCallExpression);
      }

      switch (methodInfo.Name)
      {
        case nameof(QbservableExtensions.Join):
        case nameof(QbservableExtensions.LeftJoin):
        case nameof(QbservableExtensions.FullOuterJoin):
          var joinTable = methodCallExpression.Arguments.Skip(1);
        
          joinTables.Add((methodInfo, joinTable));

          VisitChained(methodCallExpression);
          break;
      }

      return methodCallExpression;
    }

    protected void VisitChained(MethodCallExpression methodCallExpression)
    {
      var firstPart = methodCallExpression.Arguments[0];

      if (firstPart.NodeType == ExpressionType.Call || firstPart.NodeType == ExpressionType.Constant)
        Visit(firstPart);
    }

    private Queue<Expression> whereClauses;
    private LambdaExpression select;
    private LambdaExpression partitionBy;
    private AutoOffsetReset? autoOffsetReset;
    protected int? Limit;
    private ConstantExpression windowedBy;
    private LambdaExpression groupBy;
    private LambdaExpression having;
    private List<(MethodInfo, IEnumerable<Expression>)> joinTables;

    protected static Expression StripQuotes(Expression expression)
    {
      while (expression.NodeType == ExpressionType.Quote)
      {
        expression = ((UnaryExpression)expression).Operand;
      }

      return expression;
    }
  }
}