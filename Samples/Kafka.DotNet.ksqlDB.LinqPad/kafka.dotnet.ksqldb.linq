<Query Kind="Program">
  <NuGetReference Prerelease="true">Kafka.DotNet.ksqlDB</NuGetReference>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.Linq</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.Query</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.Query.Context</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.RestApi</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.RestApi.Statements</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.RestApi.Serialization</Namespace>
  <Namespace>System.Reactive.Concurrency</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.Query.Functions</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.RestApi.Extensions</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.Linq.Statements</Namespace>
</Query>

async Task Main()
{
	string url = @"http:\\localhost:8088";
	await using var context = new KSqlDBContext(url);

	var http = new HttpClientFactory(new Uri(url));
	restApiClient = new KSqlDbRestApiClient(http);

	await CreateMoviesStreamAsync();
	
	await CreateOrReplaceStreamAsSelect(context);

	var semaphoreSlim = new SemaphoreSlim(0, 1);

	using var disposable = context.CreateQueryStream<Movie>(MoviesStreamName)
		.Where(p => p.Title != "E.T.")
		.Where(c => K.Functions.Like(c.Title.ToLower(), "%hard%".ToLower()) || c.Id == 1)
		.Where(p => p.RowTime >= 1510923225000)
		.Select(l => new { Id = l.Id, l.Title, l.Release_Year, l.RowTime })
		.Take(2) // LIMIT 2    
		.ToObservable() // client side processing starts here lazily after subscription. Switches to Rx.NET
		.ObserveOn(TaskPoolScheduler.Default)
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

	await InsertMovieAsync(new Movie
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
