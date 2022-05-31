using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.WorkerService.Models;

namespace ksqlDB.RestApi.Client.WorkerService.ksqlDB;

public interface IMoviesKSqlDbContext : IKSqlDBContext
{
  IQbservable<Movie> Movies { get; }
}