using WebAppV3.Models;

namespace WebAppV3.Services;
using System.Text.Json;

public class WeatherService
{
    private readonly HttpClient _httpClient;

    // Внедряем HttpClient через конструктор (Dependency Injection)
    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CurrentWeather?> GetWeatherAsync(double latitude, double longitude)
    {
        // URL бесплатного API Open-Meteo (по умолчанию берем координаты Сиднея, например)
        var url = $"https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41&current=temperature_2m,weather_code,wind_speed_10m&timezone=Australia%2FSydney";

        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var weatherData = JsonSerializer.Deserialize<WeatherResponse>(jsonString);

            return weatherData?.CurrentWeather;
        }
        catch (Exception)
        {
            // Если что-то пошло не так (нет интернета и т.д.), возвращаем null
            System.Console.WriteLine("Error");
            return null;
        }
    }
}