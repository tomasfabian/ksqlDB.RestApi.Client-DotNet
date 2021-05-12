using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Enums;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Generators
{
  public static class StatementGenerator
  {    
    public static string CreateStream<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false)
    {
      var statementContext = new StatementContext
      {
        CreationType = CreationType.Create,
        KSqlEntityType = KSqlEntityType.Stream
      };

      return CreateOrReplace<T>(statementContext, creationMetadata, ifNotExists);
    }

    public static string CreateOrReplaceStream<T>(EntityCreationMetadata creationMetadata)
    {
      var statementContext = new StatementContext
      {
        CreationType = CreationType.CreateOrReplace,
        KSqlEntityType = KSqlEntityType.Stream
      };

      return CreateOrReplace<T>(statementContext, creationMetadata, ifNotExists: null);
    }
    
    public static string CreateTable<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false)
    {
      var statementContext = new StatementContext
      {
        CreationType = CreationType.Create,
        KSqlEntityType = KSqlEntityType.Table
      };

      return CreateOrReplace<T>(statementContext, creationMetadata, ifNotExists);
    }

    public static string CreateOrReplaceTable<T>(EntityCreationMetadata creationMetadata)
    {
      var statementContext = new StatementContext
      {
        CreationType = CreationType.CreateOrReplace,
        KSqlEntityType = KSqlEntityType.Table
      };

      return CreateOrReplace<T>(statementContext, creationMetadata, ifNotExists: null);
    }

    private static string CreateOrReplace<T>(StatementContext statementContext, EntityCreationMetadata creationMetadata, bool? ifNotExists)
    {
      string ksql = new CreateEntity().Print<T>(statementContext, creationMetadata, ifNotExists);
      
      return ksql;
    }
  }
}