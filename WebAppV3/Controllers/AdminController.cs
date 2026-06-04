using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppV3.Data;
using WebAppV3.Models;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    // Внедряем контекст базы данных через конструктор
    public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // Главная страница админки — список всех проектов
    public async Task<IActionResult> Index()
    {
        // Вытаскиваем из базы абсолютно все проекты
        var projectsTask = _context.Projects.ToListAsync();
        var skillsTask = _context.Skills.ToListAsync();
        
        // Передаем список в шаблон (View)
        var viewModel = new MainPageViewModel
        {
            Projects = await projectsTask,
            Skills = await skillsTask
        };
        
        return View(viewModel);
    }
    
    // --------------------- Страницы создания проектов и скиллов ---------------------
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpGet]
    public IActionResult SkillCreate()
    {
        return View();
    }
    
    // --------------------- Форма создания проекта ---------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project project, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            // Если пользователь выбрал и загрузил файл
            if (imageFile != null && imageFile.Length > 0)
            {
                // Генерируем уникальное имя файла, чтобы картинки с одинаковым именем (например, "1.png") не перезаписывали друг друга
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            
                // Собираем абсолютный физический путь на сервере: wwwroot/images/projects/имя_файла.png
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "projects");
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Физически сохраняем файл на жесткий диск сервера
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Записываем в модель относительный путь для HTML (то, что будет в теге <img src="...">)
                project.Screenshots = "/images/projects/" + uniqueFileName;
            }

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(project);
    }
    
    // --------------------- Форма создания скилла ---------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SkillCreate(Skills skill)
    {
        if (ModelState.IsValid)
        {
            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(skill);
    }
    
    // --------------------- Страница редактирование проекта ---------------------
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
    
    // --------------------- Страница редактирование скилла ---------------------
    [HttpGet]
    public async Task<IActionResult> SkillsEdit(int id)
    {
        var skill = await _context.Skills.FindAsync(id);
        
        if (skill == null)
        {
            return NotFound();
        }
        
        return View(skill);
    }
    
    // --------------------- Редактирование проекта ---------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Project project, IFormFile? imageFile)
    {
        if (id != project.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                // Если загружен новый файл
                if (imageFile != null && imageFile.Length > 0)
                {
                    // 1. Удаляем старый файл с диска, если он существовал, чтобы не копить мусор
                    if (!string.IsNullOrEmpty(project.Screenshots))
                    {
                        var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, project.Screenshots.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // 2. Сохраняем новую картинку
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "projects");
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    // Обновляем путь в модели
                    project.Screenshots = "/images/projects/" + uniqueFileName;
                }
                else
                {
                    // КРИТИЧЕСКИ ВАЖНО: Если файл не загружался, EF может затереть путь. 
                    // Чтобы этого не произошло, отслеживаем старое значение пути из базы данных.
                    _context.Entry(project).Property(x => x.Screenshots).IsModified = imageFile == null ? false : true;
                }

                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Projects.Any(e => e.Id == project.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(project);
    }
    
    // --------------------- Редактирование скилла ---------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SkillsEdit(int id, Skills skill)
    {
        if (id != skill.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(skill);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Skills.Any(e => e.Id == skill.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(skill);
    }
    
    // --------------------- Удаление проекта ---------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project != null)
        {
            // Если у проекта был скриншот, удаляем его файл с диска
            if (!string.IsNullOrEmpty(project.Screenshots))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, project.Screenshots.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
    
    // --------------------- Удаление скилла ---------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SkillDelete(int id)
    {
        var skill = await _context.Skills.FindAsync(id);
        if (skill != null)
        {
            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
