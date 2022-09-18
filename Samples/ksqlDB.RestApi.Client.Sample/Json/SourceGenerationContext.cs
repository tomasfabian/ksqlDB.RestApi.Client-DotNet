using System.Text.Json.Serialization;
using ksqlDB.Api.Client.Samples.Models.Movies;

namespace ksqlDB.Api.Client.Samples.Json;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Movie))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}