using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.Samples.Models.Movies;

namespace ksqlDB.RestApi.Client.Samples.Json;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Movie))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}