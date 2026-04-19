using System.Text.Json.Serialization;

namespace CatBase.Models;

public class CatFactResponse
{
    [JsonPropertyName("fact")]
    public string Fact { get; set; } = string.Empty;

    [JsonPropertyName("length")]
    public int Length { get; set; }
}
