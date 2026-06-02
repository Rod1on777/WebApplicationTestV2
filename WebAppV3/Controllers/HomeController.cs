using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebAppV3.Models;
using WebAppV3.Services;
using Microsoft.EntityFrameworkCore;
using WebAppV3.Data;

namespace WebAppV3.Controllers;

public class HomeController : Controller
{
    
    //Конструктор класса
    private readonly WeatherService _weatherService;
    private readonly CatFactsService _catFactsService;
    private readonly ApplicationDbContext _context;
    public HomeController(WeatherService weatherService, CatFactsService catFactsService, ApplicationDbContext context)
    {
        _weatherService = weatherService;
        _catFactsService = catFactsService;
        _context = context;
    }
    
    public async Task<IActionResult> Index()
    {
        // Координаты Сиднея (-33.8688, 151.2093)
        double latitude = -33.8688;
        double longitude = 151.2093;

        // Вызываем наш сервис погоды и ждем (await) результат
        var weatherTask = _weatherService.GetWeatherAsync(latitude, longitude);
        var catFactTask = _catFactsService.GetCatFactsAsync();
        
        // Запрашиваем ВСЕ проекты из базы данных асинхронно
        var projectsTask = _context.Projects.ToListAsync();
        
        // Ждем выполнения обоих запросов
        await Task.WhenAll(weatherTask, catFactTask, projectsTask);
        // Упаковываем результаты в нашу общую ViewModel
        var viewModel = new MainPageViewModel
        {
            Weather = await weatherTask,
            CatFact = await catFactTask,
            Projects = await projectsTask
        };

        // Передаем полученный объект weather прямо в метод View().
        // Теперь HTML-страница Index.cshtml сможет прочитать эти данные через свойство @Model.
        return View(viewModel);
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