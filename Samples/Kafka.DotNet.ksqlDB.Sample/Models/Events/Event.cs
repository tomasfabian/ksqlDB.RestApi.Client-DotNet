using System.Collections.Generic;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations;

namespace Kafka.DotNet.ksqlDB.Sample.Models.Events
{
  record Event
  {
    [Key]
    public int Id { get; set; }

    public string[] Places { get; set; }

    //public EventCategory[] Categories { get; init; }
    [IgnoreByInserts]
    public IEnumerable<EventCategory> Categories { get; set; }
  }
}