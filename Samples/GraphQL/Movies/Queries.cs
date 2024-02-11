using GraphQL.Model;

namespace GraphQL.Movies;

[ExtendObjectType(OperationTypeNames.Query)]
public class Queries
{
  public IEnumerable<Movie> GetMovies()
  {
    return [new Movie { Title = "Matrix", Id = 1 }];
  }
}
