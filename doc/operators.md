# Operators

### Operator LIKE - String.StartsWith, String.EndsWith, String.Contains
**v1.3.0**

Match a string with a specified pattern:

```C#
var query = context.CreateQueryStream<Movie>()
  .Where(c => c.Title.ToLower().Contains("hard".ToLower());
```

```SQL
SELECT * FROM Movies
WHERE LCASE(Title) LIKE LCASE('%hard%') EMIT CHANGES;
```

```C#
var query = context.CreateQueryStream<Movie>()
  .Where(c => c.Title.StartsWith("Die");
```

```SQL
SELECT * FROM Movies
WHERE Title LIKE 'Die%' EMIT CHANGES;
```

### Operator IN - `IEnumerable<T>` and `IList<T>` Contains
**v1.0.0**

Specifies multiple OR conditions.
`IList<T>`.Contains:
```C#
var orderTypes = new List<int> { 1, 2, 3 };

Expression<Func<OrderData, bool>> expression = o => orderTypes.Contains(o.OrderType);

```
Enumerable extension:
```C#
IEnumerable<int> orderTypes = Enumerable.Range(1, 3);

Expression<Func<OrderData, bool>> expression = o => orderTypes.Contains(o.OrderType);

```
For both options the following SQL is generated:
```SQL
OrderType IN (1, 2, 3)
```

### Operator (NOT) BETWEEN
**v1.0.0**

KSqlOperatorExtensions - Between - Constrain a value to a specified range in a WHERE clause.

```C#
using ksqlDB.RestApi.Client.KSql.Query.Operators;

IQbservable<Tweet> query = context.CreateQueryStream<Tweet>()
  .Where(c => c.Id.Between(1, 5));
```

Generated KSQL:

```SQL
SELECT * FROM Tweets
WHERE Id BETWEEN 1 AND 5 EMIT CHANGES;
```

### operator Between for Time type values
**v1.5.0**

```C#
var from = new TimeSpan(11, 0, 0);
var to = new TimeSpan(15,0 , 0);

Expression<Func<MyTimeSpan, TimeSpan>> expression = t => t.Ts.Between(from, to);
```

```SQL
Ts BETWEEN '11:00:00' AND '15:00:00'
```

```C#
var from = new TimeSpan(11, 0, 0);
var to = new TimeSpan(15, 0, 0);

var query = context.CreateQueryStream<MyClass>()
  .Where(c => c.Ts.Between(from, to))
  .Select(c => new { c.Ts, to, FromTime = from, DateTime.Now, New = new TimeSpan(1, 0, 0) }
  .ToQueryString();
```

### CASE
**v1.0.0**

- Select a condition from one or more expressions.
```C#
var query = new KSqlDBContext(@"http:\\localhost:8088")
  .CreateQueryStream<Tweet>()
  .Select(c =>
    new
    {
      case_result =
        (c.Amount < 2.0) ? "small" :
        (c.Amount < 4.1) ? "medium" : "large"
    }
  );
```

```KSQL
SELECT 
  CASE 
    WHEN Amount < 2 THEN 'small' 
    WHEN Amount < 4.1 THEN 'medium' 
    ELSE 'large' 
  END AS case_result 
FROM Tweets EMIT CHANGES;
```
