using System;
using System.Linq.Expressions;
using System.Text;
using Kafka.DotNet.ksqlDB.KSql.Query.Windows;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Visitors
{
  internal class KSqlWindowsVisitor : KSqlVisitor
  {
    public KSqlWindowsVisitor(StringBuilder stringBuilder)
      : base(stringBuilder, useTableAlias: false)
    {
    }

    protected override Expression VisitConstant(ConstantExpression constantExpression)
    {
      if (constantExpression == null) throw new ArgumentNullException(nameof(constantExpression));
      var windowedBy = (TimeWindows)constantExpression.Value;

      TryGenerateWindowAggregation(windowedBy);

      return constantExpression;
    }

    private void TryGenerateWindowAggregation(TimeWindows windowedBy)
    {
      if (windowedBy == null)
        return;

      var windowType = windowedBy switch
      {
        HoppingWindows _ => "HOPPING",
        SessionWindow _ => "SESSION",
        _ => "TUMBLING"
      };

      string size = windowType == "SESSION" ? String.Empty : "SIZE ";

      Append($" WINDOW {windowType} ({size}{windowedBy.Duration.Value} {windowedBy.Duration.TimeUnit}");

      if(windowedBy is HoppingWindows {AdvanceBy: { }} hoppingWindows)
        Append($", ADVANCE BY {hoppingWindows.AdvanceBy.Value} {hoppingWindows.AdvanceBy.TimeUnit}");

      if(windowedBy is HoppingWindows {Retention: { }} hoppingWindows2)
        Append($", RETENTION {hoppingWindows2.Retention.Value} {hoppingWindows2.Retention.TimeUnit}");
      
      if (windowedBy.GracePeriod != null)
        Append($", GRACE PERIOD {windowedBy.GracePeriod.Value} {windowedBy.GracePeriod.TimeUnit}");

      Append(")");
    }
  }
}