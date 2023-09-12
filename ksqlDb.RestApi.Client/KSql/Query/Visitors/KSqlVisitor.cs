using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDb.RestApi.Client.KSql.Entities;
using DateTime = System.DateTime;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors;

internal class KSqlVisitor : ExpressionVisitor
{
  private readonly StringBuilder stringBuilder;
  private readonly KSqlQueryMetadata queryMetadata;

  internal StringBuilder StringBuilder => stringBuilder;

  public KSqlVisitor(KSqlQueryMetadata queryMetadata)
  {
    this.queryMetadata = queryMetadata ?? throw new ArgumentNullException(nameof(queryMetadata));

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
        VisitConditional((ConditionalExpression) expression);
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

    if(node.IfFalse.NodeType != ExpressionType.Conditional)
      Append(" ELSE ");

    Visit(node.IfFalse);

    return node;
  }

  protected override Expression VisitMemberInit(MemberInitExpression node)
  {
    Append("STRUCT(");

    bool isFirst = true;
    foreach (var memberBinding in node.Bindings.Where(c => c.BindingType == MemberBindingType.Assignment).OfType<MemberAssignment>())
    {
      if (isFirst)
        isFirst = false;
      else
        Append(ColumnsSeparator);

      var memberName = memberBinding.Member.GetMemberName();

      Append($"{memberName} := ");

      Visit(memberBinding.Expression);
    }

    Append(")");

    return node;
  }

  protected override Expression VisitListInit(ListInitExpression listInitExpression)
  {
    var isDictionary = listInitExpression.Type.IsDictionary();

    if (isDictionary)
    {
      //MAP('c' := 2, 'd' := 4)
      Append("MAP(");

      bool isFirst = true;

      foreach (var elementInit in listInitExpression.Initializers)
      {
        if (isFirst)
          isFirst = false;
        else
          Append(ColumnsSeparator);

        Visit(elementInit.Arguments[0]);

        Append(" := ");
        Visit(elementInit.Arguments[1]);
      }

      Append(")");
    }
    else if (listInitExpression.Type.IsList())
    {
      var arguments = listInitExpression.Initializers.SelectMany(c => c.Arguments);

      if (queryMetadata.IsInContainsScope)
        JoinAppend(arguments);
      else
        PrintArray(arguments);
    }

    return listInitExpression;
  }

  protected override Expression VisitNewArray(NewArrayExpression node)
  {
    if (queryMetadata.IsInContainsScope)
      JoinAppend(node.Expressions);
    else
      PrintArray(node.Expressions);

    return node;
  }

  private void PrintArray(IEnumerable<Expression> expressions)
  {
    Append("ARRAY[");
    PrintCommaSeparated(expressions);
    Append("]");
  }

  protected virtual KSqlFunctionVisitor CreateKSqlFunctionVisitor()
  {
    return new KSqlFunctionVisitor(stringBuilder, queryMetadata);
  }

  protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
  {
    new MethodCallVisitor(StringBuilder, queryMetadata).Visit(methodCallExpression);

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

    if (methodCallExpression.Method.DeclaringType == typeof(Enumerable) && methodCallExpression.Method.Name == nameof(Enumerable.Contains))
    {
      PrintArrayContainsForEnumerable(methodCallExpression.Arguments);
    }
  }

  private void PrintArrayContainsForEnumerable(IReadOnlyCollection<Expression> arguments)
  {
    queryMetadata.IsInContainsScope = true;

    Visit(arguments.Last());
    Append(" IN (");
    Visit(arguments.First());
    Append(")");

    queryMetadata.IsInContainsScope = false;
  }

  private void PrintArrayContains(MethodCallExpression methodCallExpression)
  {
    queryMetadata.IsInContainsScope = true;

    Visit(methodCallExpression.Arguments);
    Append(" IN (");
    Visit(methodCallExpression.Object);
    Append(")");

    queryMetadata.IsInContainsScope = false;
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

    var constantExpressionVisitor = new ConstantVisitor(stringBuilder, queryMetadata);

    return constantExpressionVisitor.Visit(constantExpression) ?? constantExpression;
  }

  protected override Expression VisitBinary(BinaryExpression binaryExpression)
  {
    if (binaryExpression == null) throw new ArgumentNullException(nameof(binaryExpression));

    return new BinaryVisitor(StringBuilder, queryMetadata).Visit(binaryExpression) ?? binaryExpression;
  }

  protected override Expression VisitNew(NewExpression newExpression)
  {
    if (newExpression == null) throw new ArgumentNullException(nameof(newExpression));

    if (newExpression.Type.IsAnonymousType())
    {
      bool isFirst = true;

      foreach (var memberWithArguments in newExpression.Members.Zip(newExpression.Arguments,
                 (x, y) => new {First = x, Second = y}))
      {
        if (isFirst)
          isFirst = false;
        else
          Append(ColumnsSeparator);

        switch (memberWithArguments.Second.NodeType)
        {
          case ExpressionType.Not:
          case ExpressionType.TypeAs:
          case ExpressionType.ArrayLength:
          case ExpressionType.Constant:
          case ExpressionType.NewArrayInit:
          case ExpressionType.ListInit:
          case ExpressionType.MemberInit:
          case ExpressionType.Call:
            Visit(memberWithArguments.Second);
            Append(" ");
            break;
          case ExpressionType.MemberAccess:
            if (memberWithArguments.Second is MemberExpression
                {
                  Expression: MemberInitExpression memberInitExpression
                } && memberInitExpression.Type.IsStruct())
            {
              Visit(memberWithArguments.Second);
              Append(" ");
            }
            else if (memberWithArguments.Second.NodeType == ExpressionType.MemberAccess &&
                     memberWithArguments.Second is MemberExpression me5 && me5.Expression?.Type != null &&
                     me5.Expression.Type.IsKsqlGrouping())
            {
              VisitMemberWithArguments(memberWithArguments.First, memberWithArguments.Second);

              continue;
            }

            if (memberWithArguments.Second is MemberExpression {Expression: null} && TryVisitTimeTypes(memberWithArguments.Second))
            {
              Append(" ");
            }

            break;

          case ExpressionType.Conditional:
            Append("CASE");
            Visit(memberWithArguments.Second);
            Append(" END AS ");
            break;
        }

        if (memberWithArguments.Second is BinaryExpression)
        {
          PrintColumnWithAlias(memberWithArguments.First, memberWithArguments.Second);

          continue;
        }

        VisitMemberWithArguments(memberWithArguments.First, memberWithArguments.Second);
      }
    }
    else
      TryVisitTimeTypes(newExpression);

    return newExpression;
  }

  private bool TryVisitTimeTypes(Expression expression)
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

  private void VisitMemberWithArguments(MemberInfo memberInfo, Expression expression)
  {
    if (ShouldAppendAlias(memberInfo, expression))
      PrintColumnWithAlias(memberInfo, expression);
    else
      ProcessVisitNewMember(memberInfo, expression);
  }

  private bool ShouldAppendAlias(MemberInfo memberInfo, Expression expression)
  {
    if (expression is MemberExpression me2 && me2.Member.DeclaringType.IsKsqlGrouping())
      return false;

    return expression.NodeType == ExpressionType.MemberAccess && expression is MemberExpression me &&
           me.Member.Name != memberInfo.Name;
  }

  private void PrintColumnWithAlias(MemberInfo memberInfo, Expression expression)
  {
    Visit(expression);
    Append(" AS ");
    Append(memberInfo.Name);
  }

  protected virtual void ProcessVisitNewMember(MemberInfo memberInfo, Expression expression)
  {
    if (expression is MemberExpression { Expression: MemberExpression me3 } && me3.Expression != null && me3.Expression.Type.IsKsqlGrouping())
    {
      Append(memberInfo.Name);
      return;
    }

    if (expression.NodeType == ExpressionType.New)
    {
      Visit(expression);
      Append(" ");
    }

    if (expression is MemberExpression { Expression: { NodeType: ExpressionType.MemberAccess } } me)
      Destructure(me);
    else if (expression is MemberExpression me2 && me2.Expression?.NodeType == ExpressionType.Constant)
      Visit(expression);
    else
      Append(memberInfo.Name);
  }

  protected override Expression VisitMember(MemberExpression memberExpression)
  {
    if (memberExpression == null) throw new ArgumentNullException(nameof(memberExpression));

    if (memberExpression.Expression == null)
    {
      TryVisitTimeTypes(memberExpression);

      new KSqlWindowBoundsVisitor(StringBuilder, queryMetadata).Visit(memberExpression);

      return memberExpression;
    }

    var memberName = memberExpression.Member.GetMemberName();

    switch (memberExpression.Expression.NodeType)
    {
      case ExpressionType.Parameter:
        AppendVisitMemberParameter(memberExpression);
        break;
      case ExpressionType.MemberInit:
        Destructure(memberExpression);
        break;
      case ExpressionType.MemberAccess:
        HandleMemberAccess(memberExpression, memberName);
        break;
      case ExpressionType.Convert:
      case ExpressionType.ConvertChecked:
        Append(memberName);
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

    if (queryMetadata.Joins?.Any() ?? false)
    {
      fromItem = TrySetFromItemAlias(memberExpression, type);

      string alias = fromItem?.Alias ?? ((ParameterExpression)memberExpression.Expression).Name;
      Append(alias);
      Append(".");
    }

    if (type != fromItem?.Type)
      Append(memberExpression.Member.GetMemberName());
  }

  private FromItem TrySetFromItemAlias(MemberExpression memberExpression, Type propertyInfo)
  {
    var fromItem = queryMetadata.Joins.FirstOrDefault(c => c.Type == propertyInfo);

    if (fromItem != null)
      fromItem.Alias = memberExpression.Member.Name;
    else
    {
      fromItem = queryMetadata.TrySetAlias(memberExpression, (fromItem, alias) => fromItem.Alias == alias);
    }

    return fromItem;
  }

  protected void Destructure(MemberExpression memberExpression)
  {
    Visit(memberExpression.Expression);

    var fromItem = queryMetadata.Joins?.FirstOrDefault(c => c.Type == memberExpression.Member.DeclaringType);

    if (fromItem == null)
      Append("->");

    var memberName = memberExpression.Member.GetMemberName();

    Append(memberName);
  }

  internal static object ExtractMemberValue(MemberExpression memberExpression)
  {
    var innerMember = (ConstantExpression)memberExpression.Expression;
    var innerField = innerMember.Value;

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

  public void Append(string value)
  {
    stringBuilder.Append(value);
  }

  public void AppendLine(string value)
  {
    stringBuilder.AppendLine(value);
  }

  private const string ColumnsSeparator = ", ";

  protected void JoinAppend(IEnumerable enumerable)
  {
    bool isFirst = true;

    foreach (var value in enumerable)
    {
      if (isFirst)
        isFirst = false;
      else
        Append(ColumnsSeparator);

      if(value is ConstantExpression constantExpression)
        Visit(constantExpression);
      else
        Visit(Expression.Constant(value));
    }
  }

  protected void Append(IEnumerable enumerable)
  {
    if (queryMetadata.IsInContainsScope)
      JoinAppend(enumerable);
    else
      PrintArray(enumerable.OfType<object>().Select(Expression.Constant));
  }
}
