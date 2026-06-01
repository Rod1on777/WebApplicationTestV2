namespace WebAppV3.Models;
using System.Text.Json.Serialization;

public class WeatherResponse
{
    [JsonPropertyName("current")]
    public CurrentWeather CurrentWeather { get; set; }
}

public class CurrentWeather
{
    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public double WindSpeed { get; set; }

    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }
}