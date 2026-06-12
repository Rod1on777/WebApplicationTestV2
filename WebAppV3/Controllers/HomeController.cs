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
        var skillsTask = _context.Skills.ToListAsync();
        var worksTask = _context.Works.ToListAsync();
        var certTask = _context.Certs.ToListAsync();
        
        // Ждем выполнения обоих запросов
        await Task.WhenAll(weatherTask, catFactTask, projectsTask, skillsTask, worksTask, certTask);
        // Упаковываем результаты в нашу общую ViewModel
        var viewModel = new MainPageViewModel
        {
            Weather = await weatherTask,
            CatFact = await catFactTask,
            Projects = await projectsTask,
            Skills = await skillsTask,
            Works = await worksTask,
            Certs = await certTask
        };

        // Передаем полученный объект weather прямо в метод View().
        // Теперь HTML-страница ApiDoc.cshtml сможет прочитать эти данные через свойство @Model.
        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }
    
    public IActionResult Terraform()
    {
        return View();
    }
    
    public IActionResult Github()
    {
        return View();
    }
    
    public IActionResult Architecture()
    {
        return View();
    }
    
    public IActionResult ApiDoc()
    {
        return View();
    }
    
    // Контроллер страницы деталей проекта
    public async Task<IActionResult> Details(int id)
    {
        // Ищем проект в базе данных по его уникальному ID
        var project = await _context.Projects.FindAsync(id);

        // Если проекта с таким ID нет (например, пользователь подставил цифру в адресную строку вручную)
        if (project == null)
        {
            return NotFound(); // Вернет стандартную страницу ошибки 404
        }

        // Передаем найденный проект в HTML-шаблон
        return View(project);
    }
    
    // Ошибки
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}