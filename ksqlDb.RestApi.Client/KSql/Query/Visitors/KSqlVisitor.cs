using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDb.RestApi.Client.KSql.Entities;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDb.RestApi.Client.KSql.RestApi.Parsers;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using DateTime = System.DateTime;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors;

internal class KSqlVisitor : ExpressionVisitor
{
  private readonly StringBuilder stringBuilder;
  internal KSqlQueryMetadata QueryMetadata { get; set; }
  internal StringBuilder StringBuilder => stringBuilder;

  public KSqlVisitor(KSqlQueryMetadata queryMetadata)
  {
    QueryMetadata = queryMetadata ?? throw new ArgumentNullException(nameof(queryMetadata));

    stringBuilder = new StringBuilder();
  }

  internal KSqlVisitor(StringBuilder stringBuilder, KSqlQueryMetadata queryMetadata)
    : this(queryMetadata)
  {
    this.stringBuilder = stringBuilder ?? throw new ArgumentNullException(nameof(stringBuilder));
  }

  public string BuildKSql()
  {
    var ksql = stringBuilder.ToString();

    stringBuilder.Clear();

    return ksql;
  }

  public string BuildKSql(Expression expression)
  {
    stringBuilder.Clear();

    Visit(expression);

    return stringBuilder.ToString();
  }

  public override Expression Visit(Expression expression)
  {
    if (expression == null)
      return null;

    //https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/quick-reference/
    switch (expression.NodeType)
    {
      case ExpressionType.Constant:
        VisitConstant((ConstantExpression)expression);
        break;

      //arithmetic
      case ExpressionType.Add:
      case ExpressionType.Subtract:
      case ExpressionType.Divide:
      case ExpressionType.Multiply:
      case ExpressionType.Modulo:
      //conditionals
      case ExpressionType.AndAlso:
      case ExpressionType.OrElse:
      case ExpressionType.NotEqual:
      case ExpressionType.Equal:
      case ExpressionType.GreaterThan:
      case ExpressionType.GreaterThanOrEqual:
      case ExpressionType.LessThan:
      case ExpressionType.LessThanOrEqual:
      //arrays
      case ExpressionType.ArrayIndex:
        VisitBinary((BinaryExpression)expression);
        break;

      case ExpressionType.Lambda:
      case ExpressionType.TypeAs:
        base.Visit(expression);
        break;

      case ExpressionType.Parameter:
        base.Visit(expression);
        break;

      case ExpressionType.New:
        VisitNew((NewExpression)expression);
        break;

      case ExpressionType.MemberAccess:
        VisitMember((MemberExpression)expression);
        break;

      case ExpressionType.Call:
        VisitMethodCall((MethodCallExpression)expression);
        break;

      case ExpressionType.Convert:
      case ExpressionType.ConvertChecked:
      case ExpressionType.ArrayLength:
      case ExpressionType.Not:
        VisitUnary((UnaryExpression)expression);
        break;

      case ExpressionType.NewArrayInit:
        VisitNewArray((NewArrayExpression)expression);
        break;

      case ExpressionType.ListInit:
        VisitListInit((ListInitExpression)expression);
        break;

      case ExpressionType.MemberInit:
        VisitMemberInit((MemberInitExpression)expression);
        break;

      case ExpressionType.Conditional:
        VisitConditional((ConditionalExpression)expression);
        break;
    }

    return expression;
  }

  protected override Expression VisitConditional(ConditionalExpression node)
  {
    Append(" WHEN ");
    Visit(node.Test);

    Append(" THEN ");
    Visit(node.IfTrue);

    if (node.IfFalse.NodeType != ExpressionType.Conditional)
      Append(" ELSE ");

    Visit(node.IfFalse);

    return node;
  }

  protected override Expression VisitMemberInit(MemberInitExpression node)
  {
    Append("STRUCT(");

    var memberAssignments = node.Bindings
      .Where(c => c.BindingType == MemberBindingType.Assignment)
      .OfType<MemberAssignment>();

    bool isFirst = true;
    foreach (var memberBinding in memberAssignments)
    {
      if (isFirst)
        isFirst = false;
      else
        Append(ColumnsSeparator);

      var memberName = memberBinding.Member.Format(QueryMetadata.IdentifierEscaping);

      Append($"{memberName} := ");

      Visit(memberBinding.Expression);
    }

    Append(")");

    return node;
  }

  protected override Expression VisitListInit(ListInitExpression listInitExpression)
  {
    if (listInitExpression == null) throw new ArgumentNullException(nameof(listInitExpression));

    var listInitExpressionVisitor = new ListInitVisitor(stringBuilder, QueryMetadata);

    return listInitExpressionVisitor.Visit(listInitExpression) ?? listInitExpression;
  }

  protected override Expression VisitNewArray(NewArrayExpression node)
  {
    if (QueryMetadata.IsInContainsScope)
      JoinAppend(node.Expressions);
    else
      PrintArray(node.Expressions);

    return node;
  }

  private protected void PrintArray(IEnumerable<Expression> expressions)
  {
    Append("ARRAY[");
    PrintCommaSeparated(expressions);
    Append("]");
  }

  protected virtual KSqlFunctionVisitor CreateKSqlFunctionVisitor()
  {
    return new KSqlFunctionVisitor(stringBuilder, QueryMetadata);
  }

  protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
  {
    new MethodCallVisitor(StringBuilder, QueryMetadata).Visit(methodCallExpression);

    return methodCallExpression;
  }

  private protected void TryPrintContains(MethodCallExpression methodCallExpression, MethodInfo methodInfo)
  {
    if (methodCallExpression.Object != null && methodCallExpression.Object.Type.IsList())
    {
      if (methodInfo.Name == nameof(List<int>.Contains))
      {
        PrintArrayContains(methodCallExpression);
      }
    }

    if (methodCallExpression.Method.DeclaringType == typeof(Enumerable) &&
        methodCallExpression.Method.Name == nameof(Enumerable.Contains))
    {
      PrintArrayContainsForEnumerable(methodCallExpression.Arguments);
    }
  }

  private void PrintArrayContainsForEnumerable(IReadOnlyCollection<Expression> arguments)
  {
    QueryMetadata.IsInContainsScope = true;

    Visit(arguments.Last());
    Append(" IN (");
    Visit(arguments.First());
    Append(")");

    QueryMetadata.IsInContainsScope = false;
  }

  private void PrintArrayContains(MethodCallExpression methodCallExpression)
  {
    QueryMetadata.IsInContainsScope = true;

    Visit(methodCallExpression.Arguments);
    Append(" IN (");
    Visit(methodCallExpression.Object);
    Append(")");

    QueryMetadata.IsInContainsScope = false;
  }

  protected void PrintFunctionArguments(IEnumerable<Expression> expressions)
  {
    Append("(");

    PrintCommaSeparated(expressions);

    Append(")");
  }

  protected void PrintCommaSeparated(IEnumerable<Expression> expressions)
  {
    bool isFirst = true;

    foreach (var expression in expressions)
    {
      if (isFirst)
        isFirst = false;
      else
        Append(ColumnsSeparator);

      Visit(expression);
    }
  }

  protected override Expression VisitConstant(ConstantExpression constantExpression)
  {
    if (constantExpression == null) throw new ArgumentNullException(nameof(constantExpression));

    var constantExpressionVisitor = new ConstantVisitor(stringBuilder, QueryMetadata);

    return constantExpressionVisitor.Visit(constantExpression) ?? constantExpression;
  }

  protected override Expression VisitBinary(BinaryExpression binaryExpression)
  {
    if (binaryExpression == null) throw new ArgumentNullException(nameof(binaryExpression));

    return new BinaryVisitor(StringBuilder, QueryMetadata).Visit(binaryExpression) ?? binaryExpression;
  }

  protected override Expression VisitNew(NewExpression newExpression)
  {
    if (newExpression == null) throw new ArgumentNullException(nameof(newExpression));

    var newExpressionVisitor = new NewVisitor(stringBuilder, QueryMetadata);

    return newExpressionVisitor.Visit(newExpression) ?? newExpression;
  }

  private protected bool TryVisitTimeTypes(Expression expression)
  {
    if (expression.Type == typeof(DateTime))
    {
      Visit(GetValue<DateTime>(expression));
    }
    else if (expression.Type == typeof(TimeSpan))
    {
      Visit(GetValue<TimeSpan>(expression));
    }
    else if (expression.Type == typeof(DateTimeOffset))
    {
      Visit(GetValue<DateTimeOffset>(expression));
    }
    else
      return false;

    return true;
  }

  private static ConstantExpression GetValue<TType>(Expression newExpression)
  {
    var factory = Expression
      .Lambda<Func<TType>>(newExpression)
      .Compile();

    var constantExpression = Expression.Constant(factory());

    return constantExpression;
  }

  private protected bool ShouldAppendAlias(MemberInfo memberInfo, Expression expression)
  {
    if (expression is MemberExpression me2 && me2.Member.DeclaringType.IsKsqlGrouping())
      return false;

    return expression.NodeType == ExpressionType.MemberAccess && expression is MemberExpression me &&
           me.Member.Name != memberInfo.Name;
  }

  private protected void PrintColumnWithAlias(MemberInfo memberInfo, Expression expression)
  {
    Visit(expression);
    Append(" AS ");
    Append(memberInfo.Format(QueryMetadata.IdentifierEscaping));
  }

  protected virtual void ProcessVisitNewMember(MemberInfo memberInfo, Expression expression)
  {
    if (QueryMetadata.Joins != null && QueryMetadata.Joins.Any() && expression.NodeType == ExpressionType.MemberAccess)
    {
      Visit(expression);

      Append(" " + memberInfo.Format(QueryMetadata.IdentifierEscaping));
      return;
    }

    if (expression is MemberExpression { Expression: MemberExpression { Expression: not null } me1 } &&
        me1.Expression.Type.IsKsqlGrouping())
    {
      Append(memberInfo.Format(QueryMetadata.IdentifierEscaping));
      return;
    }

    if (expression.NodeType == ExpressionType.New)
    {
      Visit(expression);
      Append(" ");
    }

    switch (expression)
    {
      case MemberExpression { Expression.NodeType: ExpressionType.MemberAccess } me:
        Destructure(me);
        break;
      case MemberExpression me2 when me2.Member.GetCustomAttribute<JsonPropertyNameAttribute>() != null ||
                                     me2.Member.GetCustomAttribute<PseudoColumnAttribute>() != null:
        Append(me2.Member.Format(QueryMetadata.IdentifierEscaping));
        break;
      case MemberExpression { Expression.NodeType: ExpressionType.Constant }:
        Visit(expression);
        break;
      default:
        Append(memberInfo.Format(QueryMetadata.IdentifierEscaping));
        break;
    }
  }

  protected override Expression VisitMember(MemberExpression memberExpression)
  {
    if (memberExpression == null) throw new ArgumentNullException(nameof(memberExpression));

    if (QueryMetadata.Joins != null && QueryMetadata.Joins.Any())
    {
      if (memberExpression.Expression?.NodeType == ExpressionType.Parameter)
      {
        var foundFromItem = QueryMetadata.TrySetAlias(memberExpression, (_, alias) => string.IsNullOrEmpty(alias));

        var memberName = memberExpression.Member.Format(QueryMetadata.IdentifierEscaping);

        var alias = IdentifierUtil.Format(((ParameterExpression)memberExpression.Expression).Name,
          QueryMetadata.IdentifierEscaping);

        Append(foundFromItem?.Alias ?? alias);
        Append(".");
        Append(memberName);

        return memberExpression;
      }

      var fromItem = QueryMetadata.Joins.FirstOrDefault(c => c.Type == memberExpression.Member.DeclaringType);

      if (fromItem != null && memberExpression.Expression?.NodeType == ExpressionType.MemberAccess)
      {
        string alias = ((MemberExpression)memberExpression.Expression).Member.Format(QueryMetadata.IdentifierEscaping);

        fromItem.Alias = alias;

        Append(alias);

        Append(".");

        var memberName = memberExpression.Member.Format(QueryMetadata.IdentifierEscaping);
        Append(memberName);
        return memberExpression;
      }
    }

    if (memberExpression.Expression == null)
    {
      TryVisitTimeTypes(memberExpression);

      new KSqlWindowBoundsVisitor(StringBuilder, QueryMetadata).Visit(memberExpression);

      return memberExpression;
    }

    var memberName2 = memberExpression.Member.GetMemberName();

    switch (memberExpression.Expression.NodeType)
    {
      case ExpressionType.Parameter:
        AppendVisitMemberParameter(memberExpression);
        break;
      case ExpressionType.MemberInit:
        Destructure(memberExpression);
        break;
      case ExpressionType.MemberAccess:
        HandleMemberAccess(memberExpression, memberName2);
        break;
      case ExpressionType.Convert:
      case ExpressionType.ConvertChecked:
        Append(memberName2);
        break;
      default:
      {
        var outerObj = ExtractMemberValue(memberExpression);

        Visit(Expression.Constant(outerObj));
        break;
      }
    }

    return memberExpression;
  }

  private void HandleMemberAccess(MemberExpression memberExpression, string memberName)
  {
    if (memberName == nameof(string.Length))
    {
      Append("LEN(");
      Visit(memberExpression.Expression);
      Append(")");
    }
    else if (memberExpression.NodeType == ExpressionType.MemberAccess && memberExpression.Expression is MemberExpression
             {
               Member.ReflectedType: not null
             } me && !me.Member.ReflectedType.IsKsqlGrouping())
    {
      Destructure(memberExpression);
    }
    else
      Append($"{memberExpression.Member.Name.ToUpper()}");
  }

  private void AppendVisitMemberParameter(MemberExpression memberExpression)
  {
    FromItem fromItem = null;

    Type type = default;

    if (memberExpression.Member is PropertyInfo propertyInfo)
      type = propertyInfo.PropertyType;
    else if (memberExpression.Member is FieldInfo fieldInfo)
      type = fieldInfo.FieldType;

    if (QueryMetadata.Joins?.Any() ?? false)
    {
      fromItem = TrySetFromItemAlias(memberExpression, type);

      string alias = fromItem?.Alias ?? ((ParameterExpression)memberExpression.Expression)?.Name;
      Append(alias);
      Append(".");
    }

    if (type != fromItem?.Type)
    {
      Append(memberExpression.Member.Format(QueryMetadata.IdentifierEscaping));
    }
  }

  private FromItem TrySetFromItemAlias(MemberExpression memberExpression, Type propertyInfo)
  {
    var fromItem = QueryMetadata.Joins.FirstOrDefault(c => c.Type == propertyInfo);

    if (fromItem != null)
      fromItem.Alias = memberExpression.Member.Name;
    else
    {
      fromItem = QueryMetadata.TrySetAlias(memberExpression, (fi, alias) => fi.Alias == alias);
    }

    return fromItem;
  }

  protected void Destructure(MemberExpression memberExpression)
  {
    Visit(memberExpression.Expression);

    var fromItem = QueryMetadata.Joins?.FirstOrDefault(c => c.Type == memberExpression.Member.DeclaringType);

    if (fromItem == null)
      Append("->");

    var memberName = memberExpression.Member.Format(QueryMetadata.IdentifierEscaping);

    Append(memberName);
  }

  internal static object ExtractMemberValue(MemberExpression memberExpression)
  {
    var innerMember = (ConstantExpression)memberExpression.Expression;
    var innerField = innerMember!.Value;

    object outerObj = memberExpression.Member switch
    {
      PropertyInfo propertyInfo => propertyInfo.GetValue(innerField),
      FieldInfo fieldInfo => fieldInfo.GetValue(innerField),
      _ => throw new InvalidOperationException($"Unsupported member type: {memberExpression.Member.GetType()}")
    };

    return outerObj;
  }

  protected override Expression VisitUnary(UnaryExpression unaryExpression)
  {
    if (unaryExpression == null) throw new ArgumentNullException(nameof(unaryExpression));

    switch (unaryExpression.NodeType)
    {
      case ExpressionType.ArrayLength:
        Append("ARRAY_LENGTH(");
        base.Visit(unaryExpression.Operand);
        Append(")");
        return unaryExpression;
      case ExpressionType.Not:
        Append("NOT ");

        return base.VisitUnary(unaryExpression);
      case ExpressionType.Convert:
      case ExpressionType.ConvertChecked:
        return base.Visit(unaryExpression.Operand);
      default:
        return base.VisitUnary(unaryExpression);
    }
  }

  protected static Expression StripQuotes(Expression expression)
  {
    while (expression.NodeType == ExpressionType.Quote)
    {
      expression = ((UnaryExpression)expression).Operand;
    }

    return expression;
  }

  protected void Append(char value)
  {
    stringBuilder.Append(value);
  }

  public void Append(string value)
  {
    stringBuilder.Append(value);
  }

  public void AppendLine(string value)
  {
    stringBuilder.AppendLine(value);
  }

  private protected const string ColumnsSeparator = ", ";

  protected void JoinAppend(IEnumerable enumerable)
  {
    bool isFirst = true;

    foreach (var value in enumerable)
    {
      if (isFirst)
        isFirst = false;
      else
        Append(ColumnsSeparator);

      if (value is ConstantExpression constantExpression)
        Visit(constantExpression);
      else
        Visit(Expression.Constant(value));
    }
  }

  protected void Append(IEnumerable enumerable)
  {
    if (QueryMetadata.IsInContainsScope)
      JoinAppend(enumerable);
    else
      PrintArray(enumerable.OfType<object>().Select(Expression.Constant));
  }
}
