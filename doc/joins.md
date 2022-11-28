# JOIN clause - Joining collections

### RightJoin

**v2.1.0**

- Select all records for the right side of the join and the matching records from the left side. If the matching records on the left side are missing, the corresponding columns will contain null values.

```C#
using ksqlDB.RestApi.Client.KSql.Linq;

var query = KSqlDBContext.CreateQueryStream<Lead_Actor>(ActorsTableName)
  .RightJoin(
    Source.Of<Movie>(MoviesTableName),
    actor => actor.Title,
    movie => movie.Title,
    (actor, movie) => new
    {
      movie.Id,
      Title = movie.Title,
      movie.Release_Year,
      Substr = K.Functions.Substring(movie.Title, 2, 4),
      ActorTitle = actor.Title,
    }
  ));
```

```SQL
SELECT movie.Id Id, movie.Title Title, movie.Release_Year Release_Year, SUBSTRING(movie.Title, 2, 4) Substr, actor.Title AS ActorTitle FROM lead_actor_test actor
 RIGHT JOIN movies_test movie
    ON actor.Title = movie.Title
  EMIT CHANGES;
```

### Support explicit message types for Protobuf with multiple definitions

**v2.1.0**

- the following 2 new fields were added to `CreationMetadata`: `KeySchemaFullName` and `ValueSchemaFullName`

```C#
var creationMetadata = new CreationMetadata
{
  KeySchemaFullName = "ProductKey"
  ValueSchemaFullName = "ProductInfo"
};
```
