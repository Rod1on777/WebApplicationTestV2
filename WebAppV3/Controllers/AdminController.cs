using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppV3.Data;
using WebAppV3.Models;

[Authorize(Roles = "Admin")]
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
    
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var project = await _context.Projects.FindAsync(id);
        
        if (project == null)
        {
            return NotFound();
        }
        
        return View(project);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Project project)
    {
        if (id != project.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Говорим Entity Framework, что этот проект изменен
                _context.Update(project);
                // Сохраняем изменения в файл app.db
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Projects.Any(e => e.Id == project.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(project);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        // Находим проект в базе
        var project = await _context.Projects.FindAsync(id);
    
        if (project != null)
        {
            // Удаляем проект из таблицы Projects
            _context.Projects.Remove(project);
            // Фиксируем удаление в файле базы данных
            await _context.SaveChangesAsync();
        }
    
        // Возвращаем администратора на обновленный список проектов
        return RedirectToAction(nameof(Index));
    }
}
