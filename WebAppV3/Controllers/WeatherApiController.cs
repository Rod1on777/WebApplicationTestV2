namespace WebAppV3.Controllers;
using WebAppV3.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/weather")] // Путь к нашему API: /api/weather
public class WeatherApiController : ControllerBase
{
    private readonly WeatherService _weatherService;

    public WeatherApiController(WeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWeather()
    {
        // Координаты Сиднея: -33.8688, 151.2093
        double latitude = 52.52;
        double longitude = 13.41;

        var weather = await _weatherService.GetWeatherAsync(latitude, longitude);

        if (weather == null)
        {
            return StatusCode(500, "Не удалось получить данные о погоде.");
        }

        // Возвращаем JSON ответ
        return Ok(new
        {
            City = "Sydney",
            Temperature = $"{weather.Temperature}°C",
            WindSpeed = $"{weather.WindSpeed} km/h",
            AsOf = DateTime.Now.ToString("g")
        });
    }
}