using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppV3.Data;
using WebAppV3.Models;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    // Внедряем контекст базы данных через конструктор
    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Главная страница админки — список всех проектов
    public async Task<IActionResult> Index()
    {
        // Вытаскиваем из базы абсолютно все проекты
        var projects = await _context.Projects.ToListAsync();
        
        // Передаем список в шаблон (View)
        System.Console.WriteLine(projects);
        return View(projects);
    }
    
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken] // Защита от CSRF-атак (подделки запросов)
    public async Task<IActionResult> Create(Project project)
    {
        // Проверяем, заполнена ли форма без ошибок (валидация аннотаций [Required])
        if (ModelState.IsValid)
        {
            // Добавляем полученный объект в таблицу Projects через Entity Framework
            _context.Projects.Add(project);
        
            // Физически сохраняем изменения в файл app.db
            await _context.SaveChangesAsync();
        
            // После успешного сохранения перенаправляем администратора обратно на список проектов
            return RedirectToAction(nameof(Index));
        }

        // Если форма заполнена некорректно, возвращаем пользователя на ту же форму с сохраненными текстами ошибок
        return View(project);
    }
}
