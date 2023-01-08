using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace DataTypes.Model.Events;

internal record Event
{
  [Key]
  public int Id { get; set; }

  public string[] Places { get; set; } = null!;

  //public EventCategory[] Categories { get; init; }
  [IgnoreByInserts]
  public IEnumerable<EventCategory> Categories { get; set; } = null!;
}