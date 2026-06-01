using System.Text.Json.Serialization;

namespace WebAppV3.Models;

public class CatFactResponse
{
    [JsonPropertyName("fact")]
    public string CatFact { get; set; }
    
    [JsonPropertyName("length")]
    public int CatFactLength { get; set; }
}
