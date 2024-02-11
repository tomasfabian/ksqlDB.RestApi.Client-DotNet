using GraphQL.Model;

namespace GraphQL.Movies
{
  [ExtendObjectType(OperationTypeNames.Subscription)]
  public class Subscription
  {
    [Subscribe]
    public Movie MovieAdded([EventMessage] Movie movie)
    {
      return movie;
    }
  }
}
