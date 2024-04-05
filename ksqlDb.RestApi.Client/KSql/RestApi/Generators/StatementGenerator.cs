using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDb.RestApi.Client.Metadata;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Generators
{
  /// <summary>
  /// Contains functionality for generating DDL statements.
  /// </summary>
  public class StatementGenerator
  {
    private readonly ModelBuilder modelBuilder;

    public StatementGenerator(ModelBuilder modelBuilder)
      : this()
    {
      this.modelBuilder = modelBuilder;
    }

    public StatementGenerator()
    {
      modelBuilder ??= new ModelBuilder();
    }

    /// <summary>
    /// Creates a KSQL Stream.
    /// </summary>
    /// <param name="creationMetadata">Metadata for the stream to be created.</param>
    /// <param name="ifNotExists">Indicates whether to not create the stream if it already exists.</param>
    /// <typeparam name="T">Type of the stream entity.</typeparam>
    /// <returns>A string representing a CREATE STREAM KSQL statement.</returns>
    public string CreateStream<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false)
    {
      var statementContext = new StatementContext
      {
        CreationType = CreationType.Create,
        KSqlEntityType = KSqlEntityType.Stream
      };

      return CreateOrReplace<T>(statementContext, creationMetadata, ifNotExists);
    }

    /// <summary>
    /// Creates or replaces a KSQL Stream.
    /// </summary>
    /// <param name="creationMetadata">Metadata for the stream to be created or replaced.</param>
    /// <typeparam name="T">Type of the stream entity.</typeparam>
    /// <returns>A string representing a CREATE OR REPLACE STREAM KSQL statement.</returns>
    public string CreateOrReplaceStream<T>(EntityCreationMetadata creationMetadata)
    {
      var statementContext = new StatementContext
      {
        CreationType = CreationType.CreateOrReplace,
        KSqlEntityType = KSqlEntityType.Stream
      };

      return CreateOrReplace<T>(statementContext, creationMetadata, ifNotExists: null);
    }

    /// <summary>
    /// Creates a KSQL Table.
    /// </summary>
    /// <param name="creationMetadata">Metadata for the table to be created.</param>
    /// <param name="ifNotExists">Indicates whether to not create the table if it already exists.</param>
    /// <typeparam name="T">Type of the table entity.</typeparam>
    /// <returns>A string representing a CREATE TABLE KSQL statement.</returns>
    public string CreateTable<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false)
    {
      if (creationMetadata == null) throw new ArgumentNullException(nameof(creationMetadata));

      var statementContext = new StatementContext
      {
        CreationType = CreationType.Create,
        KSqlEntityType = KSqlEntityType.Table
      };

      return CreateOrReplace<T>(statementContext, creationMetadata, ifNotExists);
    }

    /// <summary>
    /// Creates or replaces a KSQL Table.
    /// </summary>
    /// <param name="creationMetadata">Metadata for the table to be created or replaced.</param>
    /// <typeparam name="T">Type of the table entity.</typeparam>
    /// <returns>A string representing a CREATE OR REPLACE TABLE KSQL statement.</returns>
    public string CreateOrReplaceTable<T>(EntityCreationMetadata creationMetadata)
    {
      var statementContext = new StatementContext
      {
        CreationType = CreationType.CreateOrReplace,
        KSqlEntityType = KSqlEntityType.Table
      };

      return CreateOrReplace<T>(statementContext, creationMetadata, ifNotExists: null);
    }

    /// <summary>
    /// Generates a KSQL command for creating or replacing a stream or table.
    /// </summary>
    /// <param name="statementContext">Context for the creation statement.</param>
    /// <param name="creationMetadata">Metadata for the stream or table to be created or replaced.</param>
    /// <param name="ifNotExists">Indicates whether to not create the stream or table if it already exists.</param>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <returns>A string representing a KSQL statement for creating or replacing a stream or table.</returns>
    private string CreateOrReplace<T>(StatementContext statementContext, EntityCreationMetadata creationMetadata, bool? ifNotExists)
    {
      string ksql = new CreateEntity(modelBuilder).Print<T>(statementContext, creationMetadata, ifNotExists);

      return ksql;
    }
  }
}
