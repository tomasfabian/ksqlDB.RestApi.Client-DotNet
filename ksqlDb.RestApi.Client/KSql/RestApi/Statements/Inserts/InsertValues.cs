using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Inserts;

public class InsertValues<T>
{
  public T Entity { get; }

  public InsertValues(T entity)
  {
    Entity = entity ?? throw new ArgumentNullException(nameof(entity));
  }

  private readonly Dictionary<string, string> propertyValues = new();

  internal IDictionary<string, string> PropertyValues => new ReadOnlyDictionary<string, string>(propertyValues);

  public InsertValues<T> WithValue<TColumn>(Expression<Func<T, TColumn>> getProperty, Expression<Func<TColumn>> provideValue)
  {
    if (getProperty == null) throw new ArgumentNullException(nameof(getProperty));
    if (provideValue == null) throw new ArgumentNullException(nameof(provideValue));

    var propertyName = ExtractPropertyName(getProperty);

    var stringBuilder = new StringBuilder();

    new KSqlCustomFunctionVisitor(stringBuilder, new KSqlQueryMetadata()).Visit(provideValue);

    var propertyValue = stringBuilder.ToString();

    propertyValues[propertyName] = propertyValue;

    return this;
  }

  private static string ExtractPropertyName<TProp>(Expression<Func<T, TProp>> getProperty)
  {
    if (getProperty.Body is not MemberExpression memberExpression)
      throw new ArgumentException($"Expression '{getProperty}' is not a property.");

    PropertyInfo? propertyInfo = memberExpression.Member as PropertyInfo;

    if (propertyInfo == null)
      throw new ArgumentException($"Expression '{getProperty}' is not a property.");

    var propertyName = propertyInfo.Name;

    return propertyName;
  }
}
