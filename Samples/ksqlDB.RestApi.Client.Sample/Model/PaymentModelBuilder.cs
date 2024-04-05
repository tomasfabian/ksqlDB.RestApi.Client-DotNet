using ksqlDb.RestApi.Client.Metadata;
using ksqlDB.RestApi.Client.Samples.Models.Movies;

namespace ksqlDB.RestApi.Client.Samples.Model
{
  public class PaymentModelBuilder
  {
    private record Payment
    {
      public string Id { get; set; } = null!;
      public decimal Amount { get; set; }
      public string Description { get; set; } = null!;
    }
    private record Account
    {
      public string Id { get; set; } = null!;
      public decimal Balance { get; set; }
    }

    private static void InitModel()
    {
      ModelBuilder builder = new();

      builder.Entity<Account>()
        .HasKey(c => c.Id)
        .Property(b => b.Balance);

      builder.Entity<Payment>()
        .Property(b => b.Amount)
        .Decimal(precision: 10, scale: 2);
    }
  }
}
