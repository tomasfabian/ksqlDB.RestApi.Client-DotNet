<Query Kind="Program">
  <NuGetReference>ksqlDB.RestApi.Client</NuGetReference>
  <Namespace>ksqlDB.RestApi.Client.KSql.Linq</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.Linq.Statements</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.Query</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.Query.Context</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.Query.Functions</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.RestApi</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.RestApi.Extensions</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.RestApi.Http</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.RestApi.Statements</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Reactive.Concurrency</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.RestApi.Serialization</Namespace>
</Query>

async Task Main()
{
	string ksqlDbUrl = @"http://localhost:8088";
	await using var context = new KSqlDBContext(ksqlDbUrl);

	var httpClient = new HttpClient
	{
		BaseAddress = new Uri(ksqlDbUrl)
	};

	var httpClientFactory = new HttpClientFactory(httpClient);

	restApiClient = new KSqlDbRestApiClient(httpClientFactory);

	await CreateMoviesStreamAsync();
	
	await CreateOrReplaceStreamAsSelect(context);

	var semaphoreSlim = new SemaphoreSlim(0, 1);

	using var disposable = context.CreatePushQuery<Movie>(MoviesStreamName)
		.Where(p => p.Title != "E.T.")
		.Where(c => K.Functions.Like(c.Title.ToLower(), "%hard%".ToLower()) || c.Id == 1)
		.Where(p => p.RowTime >= 1510923225000)
		.Select(l => new { Id = l.Id, l.Title, l.Release_Year, l.RowTime })
		.Take(2) // LIMIT 2    
		.ToObservable() // client side processing starts here lazily after subscription. Switches to Rx.NET
		//.ObserveOn(TaskPoolScheduler.Default)
		.Subscribe(onNext: movie =>
		{
			$"{nameof(Movie)}: {movie.Id} - {movie.Title} - {movie.RowTime}".Dump("OnNext");
		}, onError: error =>
		{
			semaphoreSlim.Release();
			$"Exception: {error.Message}".Dump("OnError"); 
		}, 
		onCompleted: () =>
		{
			semaphoreSlim.Release();
			"Completed".Dump("OnCompleted");
		});

	await restApiClient.InsertIntoAsync(new Movie
	{
		Id = 1,
		Release_Year = 1986,
		Title = "Aliens"
	});

	await InsertMovieAsync(new Movie
	{
		Id = 2,
		Release_Year = 1998,
		Title = "Die Hard"
	});
	
	semaphoreSlim.Wait();
	
	"Finished".Dump();
}

IKSqlDbRestApiClient restApiClient;

string MoviesStreamName = "my_movies_stream";

async Task CreateOrReplaceStreamAsync()
{
	EntityCreationMetadata metadata = new(kafkaTopic: nameof(Movie))
	{
		Partitions = 1,
		Replicas = 1,
		EntityName = MoviesStreamName
	};

	string ksqlDbUrl = @"http://localhost:8088";
	
	var httpClient = new HttpClient
	{
		BaseAddress = new Uri(ksqlDbUrl)
	};
	
	var httpClientFactory = new HttpClientFactory(httpClient);
	var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

	var httpResponseMessage = await restApiClient.CreateOrReplaceStreamAsync<Movie>(metadata);
}

async Task CreateMoviesStreamAsync()
{
	var createMoviesTable = $@"CREATE OR REPLACE STREAM {MoviesStreamName} (
        title VARCHAR KEY,
        id INT,
        release_year INT
      ) WITH (
        KAFKA_TOPIC='{MoviesStreamName}',
        PARTITIONS=1,
        VALUE_FORMAT = 'JSON'
      );";

	KSqlDbStatement ksqlDbStatement = new(createMoviesTable);

	var httpResponseMessage = await restApiClient.ExecuteStatementAsync(ksqlDbStatement);

	string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
}

private async Task CreateOrReplaceStreamAsSelect(IKSqlDBStatementsContext context)
{
	var httpResponseMessage = await context.CreateOrReplaceStreamStatement(streamName: "MyMoviesStream")
		.With(new CreationMetadata
		{
			KafkaTopic = MoviesStreamName,
			KeyFormat = SerializationFormats.Json,
			ValueFormat = SerializationFormats.Json,
			Replicas = 1,
			Partitions = 1
		})
		.As<Movie>(MoviesStreamName)
		.Where(c => c.Release_Year < 2000)
		.Select(c => new { c.Title, ReleaseYear = c.Release_Year })
		.PartitionBy(c => c.Title)
		.ExecuteStatementAsync();

	if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
	{
		var statementResponses = httpResponseMessage.ToStatementResponses();

		statementResponses.Dump();
	}
	else
	{
		string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
		
		responseContent.Dump();
	}
}

async Task<HttpResponseMessage> InsertMovieAsync(Movie movie)
{
	string insert =
		$"INSERT INTO {MoviesStreamName} ({nameof(Movie.Id)}, {nameof(Movie.Title)}, {nameof(Movie.Release_Year)}) VALUES ({movie.Id}, '{movie.Title}', {movie.Release_Year});";

	KSqlDbStatement ksqlDbStatement = new(insert);

	var httpResponseMessage = await restApiClient.ExecuteStatementAsync(ksqlDbStatement);

	string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
	
	return httpResponseMessage;
}

public class Movie : Record
{
	public string Title { get; set; }
	public int Id { get; set; }
	public int Release_Year { get; set; }
}
