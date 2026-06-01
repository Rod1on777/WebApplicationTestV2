using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebAppV3.Models;
using WebAppV3.Services;

namespace WebAppV3.Controllers;

public class HomeController : Controller
{
    
    //Конструктор класса
    private readonly WeatherService _weatherService;
    public HomeController(WeatherService weatherService)
    {
        _weatherService = weatherService;
    }
    
    public async Task<IActionResult> Index()
    {
        // Координаты Сиднея (-33.8688, 151.2093)
        double latitude = -33.8688;
        double longitude = 151.2093;

        // Вызываем наш сервис погоды и ждем (await) результат
        var weather = await _weatherService.GetWeatherAsync(latitude, longitude);

        // Передаем полученный объект weather прямо в метод View().
        // Теперь HTML-страница Index.cshtml сможет прочитать эти данные через свойство @Model.
        return View(weather);
    }

    public IActionResult Privacy()
    {
        return View();
    }
    
    // Ошибки
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}