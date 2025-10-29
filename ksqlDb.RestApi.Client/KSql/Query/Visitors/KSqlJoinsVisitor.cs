using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDb.RestApi.Client.KSql.RestApi.Parsers;
using Pluralize.NET;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors;

internal class KSqlJoinsVisitor : KSqlVisitor
{
  private readonly KSqlDBContextOptions contextOptions;
  private readonly QueryContext queryContext;

  public KSqlJoinsVisitor(StringBuilder stringBuilder, KSqlDBContextOptions contextOptions, QueryContext queryContext, KSqlQueryMetadata queryMetadata)
    : base(stringBuilder, queryMetadata)
  {
    this.contextOptions = contextOptions ?? throw new ArgumentNullException(nameof(contextOptions));
    this.queryContext = queryContext ?? throw new ArgumentNullException(nameof(queryContext));
  }

  private readonly JoinAliasGenerator joinAliasGenerator = new();

  private PropertyInfo GetPropertyType(Type type)
  {
    var propertyInfo = type.GetProperties()[0];

    bool isAnonymous = propertyInfo.PropertyType.IsAnonymousType();

    if (isAnonymous)
      return GetPropertyType(propertyInfo.PropertyType);

    return propertyInfo;
  }

  internal void VisitJoinTable(IEnumerable<(MethodInfo, IEnumerable<Expression>, LambdaExpression?)> joins)
  {
    int i = 0;

    foreach (var join in joins)
    {
      var (methodInfo, e, groupJoin) = join;

      var expressions = e.Select(StripQuotes).ToArray();

      Visit(expressions[0]);

      var outerItemAlias = IdentifierUtil.Format(joinAliasGenerator.GenerateAlias(queryContext.FromItemName!), QueryMetadata.IdentifierEscaping);

      var itemAlias = IdentifierUtil.Format(joinAliasGenerator.GenerateAlias(fromItemName), QueryMetadata.IdentifierEscaping);

      bool isFirst = i == 0;
      if (isFirst)
      {
        Append("SELECT ");

        var lambdaExpression = expressions[3] as LambdaExpression;
        var body = QueryMetadata.Select?.Body ?? lambdaExpression?.Body;

        body = groupJoin != null ? groupJoin.Body : body;

        new KSqlVisitor(StringBuilder, QueryMetadata).Visit(body);

        var fromItemAlias = QueryMetadata.Joins?.Skip(i).Where(c => c.Type == QueryMetadata.FromItemType && !string.IsNullOrEmpty(c.Alias)).Select(c => c.Alias).LastOrDefault();

        outerItemAlias = fromItemAlias ?? outerItemAlias;

        AppendLine($" FROM {queryContext.FromItemName} {outerItemAlias}");
      }

      if (groupJoin != null)
      {
        var prop = GetPropertyType(groupJoin.Parameters[0].Type);

        outerItemAlias = IdentifierUtil.Format(prop.Name, QueryMetadata.IdentifierEscaping);

        itemAlias = IdentifierUtil.Format(groupJoin.Parameters[1].Name!, QueryMetadata.IdentifierEscaping);
      }

      var joinType = methodInfo.Name switch
      {
        nameof(QbservableExtensions.Join) => "INNER",
        nameof(QbservableExtensions.LeftJoin) => "LEFT",
        nameof(QbservableExtensions.RightJoin) => "RIGHT",
        nameof(QbservableExtensions.GroupJoin) => "LEFT",
        nameof(QbservableExtensions.FullOuterJoin) => "FULL OUTER",
        _ => throw new ArgumentOutOfRangeException()
      };

      var itemType = join.Item2.First().Type.GetGenericArguments()[0];

      var joinItemAlias = QueryMetadata.Joins?.Where(c => c.Type == itemType && !string.IsNullOrEmpty(c.Alias)).Select(c => c.Alias).FirstOrDefault();

      itemAlias = joinItemAlias ?? itemAlias;

      AppendLine($"{joinType} JOIN {fromItemName} {itemAlias}");

      TryAppendWithin(join);

      Append(GetOn(outerItemAlias, expressions));
      Visit(expressions[1]);

      Append(GetEqualsTo(itemAlias, expressions));
      Visit(expressions[2]);

      Append(Environment.NewLine);

      i++;
    }
  }

  private string GetOn(string outerItemAlias, Expression[] expressions)
  {
    bool useAlias = !IsKSqlFunctionsExtension(expressions[1]);

    string on = useAlias ? $"{outerItemAlias}." : string.Empty;

    return $"ON {on}";
  }

  private string GetEqualsTo(string itemAlias, Expression[] expressions)
  {
    bool useAlias = !IsKSqlFunctionsExtension(expressions[2]);

    string equalsTo = useAlias ? $"{itemAlias}." : string.Empty;

    return $" = {equalsTo}";
  }

  private bool IsKSqlFunctionsExtension(Expression expression)
  {
    var lambdaExpression = expression as LambdaExpression;

    var methodCallExpression = lambdaExpression?.Body as MethodCallExpression;

    var methodInfo = methodCallExpression?.Method;

    if (methodCallExpression?.Object == null && methodInfo?.DeclaringType is { Name: nameof(KSqlFunctionsExtensions) })
      return true;

    return false;
  }

  private void TryAppendWithin((MethodInfo, IEnumerable<Expression>, LambdaExpression?) join)
  {
    var sourceExpression = join.Item2.First() as ConstantExpression;

    if (sourceExpression?.Value is SourceBase source && (source.DurationBefore != null || source.DurationAfter != null))
    {
      Append("WITHIN ");

      if (source.DurationBefore != null && source.DurationAfter != null)
      {
        Append($"({source.DurationBefore.Value} {source.DurationBefore.TimeUnit}, ");
        Append($"{source.DurationAfter.Value} {source.DurationAfter.TimeUnit}) ");
      }
      else
      {
        var duration = source.DurationBefore ?? source.DurationAfter;
        if (duration != null)
        {
          Append($"{duration.Value} {duration.TimeUnit} ");
        }
      }
    }
  }

  private string fromItemName = null!;

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

    if (memberExpression.Expression?.NodeType == ExpressionType.Parameter)
    {
      var memberName = IdentifierUtil.Format(memberExpression, QueryMetadata.IdentifierEscaping, QueryMetadata.ModelBuilder);

      Append(memberName);

      return memberExpression;
    }

    if (QueryMetadata.Joins != null && memberExpression.Expression?.NodeType == ExpressionType.MemberAccess)
    {
      Append(memberExpression.Member.Format(QueryMetadata.IdentifierEscaping, QueryMetadata.ModelBuilder));
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
      fromItemName = GetFromItemName(source, constantExpression);
    }

    return constantExpression;
  }

  private string GetFromItemName(ISource source, ConstantExpression constantExpression)
  {
    var name = constantExpression.Type.GenericTypeArguments[0].Name;

    name = source.QueryContext.FromItemName ?? name;

    name = InterceptFromItemName(name);

    return IdentifierUtil.Format(name, QueryMetadata.IdentifierEscaping);
  }
}
