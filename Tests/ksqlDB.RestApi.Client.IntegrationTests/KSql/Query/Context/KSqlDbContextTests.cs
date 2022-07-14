﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ksqlDB.Api.Client.IntegrationTests.KSql.RestApi;
using ksqlDB.Api.Client.IntegrationTests.Models.Movies;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.IntegrationTests.KSql.Query.Context;

[TestClass]
public class KSqlDbContextTests : Infrastructure.IntegrationTests
{
  private const string EntityName = "movies_test112";

  [ClassInitialize]
  public static async Task ClassInitialize(TestContext context)
  {
    var restApiClient = KSqlDbRestApiProvider.Create();
      
    await restApiClient.CreateStreamAsync<Movie>(new EntityCreationMetadata(EntityName, 1) { EntityName = EntityName, ShouldPluralizeEntityName = false });
  }

  [TestMethod]
  public async Task AddAndSaveChangesAsync()
  {
    //Arrange
    var config = new InsertProperties { EntityName = EntityName, ShouldPluralizeEntityName = false };
    var entity1 = new Movie { Id = 1, Title = "T1" };
    var entity2 = new Movie { Id = 2, Title = "T2" };

    //Act
    Context.Add(entity1, config);
    Context.Add(entity2, config);

    var response = await Context.SaveChangesAsync();
    var c = await response.Content.ReadAsStringAsync();

    //Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [TestMethod]
  public async Task AddDbContextFactory_ContextFactoryCreate_SaveChangesAsync()
  {
    //Arrange
    var serviceCollection = new ServiceCollection();

    serviceCollection.AddDbContext<IKSqlDBContext, KSqlDBContext>(options => options.UseKSqlDb(KSqlDbRestApiProvider.KsqlDbUrl), ServiceLifetime.Transient);
    serviceCollection.AddDbContextFactory<IKSqlDBContext>(factoryLifetime: ServiceLifetime.Scoped);

    var contextFactory = serviceCollection.BuildServiceProvider().GetRequiredService<IKSqlDBContextFactory<IKSqlDBContext>>();

    var config = new InsertProperties { EntityName = EntityName, ShouldPluralizeEntityName = false };
    var entity1 = new Movie { Id = 3, Title = "T3" };
    var entity2 = new Movie { Id = 4, Title = "T4" };

    //Act
    await using var context = contextFactory.Create();

    context.Add(entity1, config);
    context.Add(entity2, config);

    var response = await context.SaveChangesAsync();
    var c = await response.Content.ReadAsStringAsync();

    //Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  #region Time types

  private record TimeTypes
  {
    public DateTime Dt { get; set; }
    public TimeSpan Ts { get; set; }
    public DateTimeOffset DtOffset { get; set; }
  }

  private readonly EntityCreationMetadata metadata = new EntityCreationMetadata
  {
    KafkaTopic = nameof(TimeTypes),
    Partitions = 1,
    Replicas = 1,
    ValueFormat = SerializationFormats.Json
  };

  [TestMethod]
  public async Task TimeTypes_InsertValues_ValuesReceived()
  {
    //Arrange
    var serviceCollection = new ServiceCollection();

    serviceCollection.AddDbContext<IKSqlDBContext, KSqlDBContext>(options => options.UseKSqlDb(KSqlDbRestApiProvider.KsqlDbUrl), ServiceLifetime.Transient);

    var buildServiceProvider = serviceCollection.BuildServiceProvider();
    var httpResponseMessage = await buildServiceProvider.GetRequiredService<IKSqlDbRestApiClient>().CreateStreamAsync<TimeTypes>(metadata);
    var statementResponses = await httpResponseMessage.ToStatementResponsesAsync().ConfigureAwait(false);

    await using var context = buildServiceProvider.GetRequiredService<IKSqlDBContext>();

    var semaphoreSlim = new SemaphoreSlim(0, 1);

    var receivedValues = new List<TimeTypes>();

    //Act
    using var subscription = context.CreateQueryStream<TimeTypes>()
      .Take(1)
      .Subscribe(value =>
        {
          receivedValues.Add(value);
        }, error =>
        {
          semaphoreSlim.Release();
        },
        () =>
        {
          semaphoreSlim.Release();
        });

    var value = new TimeTypes
    {
      Dt = new DateTime(2021, 4, 1),
      Ts = new TimeSpan(1,2,3),
      DtOffset = new DateTimeOffset(2021, 7, 4, 13, 29, 45, 447, TimeSpan.Zero)
      //DtOffset = new DateTimeOffset(2021, 7, 4, 13, 29, 45, 447, TimeSpan.FromHours(4))
    };

    context.Add(value);

    var response = await context.SaveChangesAsync();

    await semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(5));

    //Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    receivedValues[0].Dt.Should().Be(value.Dt);
    receivedValues[0].Ts.Should().Be(value.Ts);
      
    //TODO: rest api bug? missing offset
    //["2021-04-01","01:02:03","2021-07-04T09:29:45.447"]
    //receivedValues[0].DtOffset.Should().Be(value.DtOffset);

//       string json = @"{
// ""DT"": ""2021-04-01""
// ,""TS"": ""01:02:03""
// ,""DTOFFSET"": ""2021-07-04T09:29:45.447""
// }
// ";
  }

  #endregion
}