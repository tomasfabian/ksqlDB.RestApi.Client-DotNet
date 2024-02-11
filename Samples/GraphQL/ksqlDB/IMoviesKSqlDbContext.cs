using GraphQL.Model;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;

namespace GraphQL.ksqlDB;

public interface IMoviesKSqlDbContext : IKSqlDBContext
{
  IQbservable<Movie> Movies { get; }
}
