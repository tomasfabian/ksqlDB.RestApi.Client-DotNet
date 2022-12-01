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

