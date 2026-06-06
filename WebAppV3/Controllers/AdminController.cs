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
        var workTask = _context.Works.ToListAsync();
        var certTask = _context.Certs.ToListAsync();
        
        // Передаем список в шаблон (View)
        var viewModel = new MainPageViewModel
        {
            Projects = await projectsTask,
            Skills = await skillsTask,
            Works = await workTask,
            Certs = await certTask
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
    
    [HttpGet]
    public IActionResult WorkCreate()
    {
        return View();
    }
    
    [HttpGet]
    public IActionResult CertCreate()
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
    
    // --------------------- Форма создания сертификата ---------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CertCreate(Certifications cert, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            // Если пользователь выбрал и загрузил файл
            if (imageFile != null && imageFile.Length > 0)
            {
                // Генерируем уникальное имя файла, чтобы картинки с одинаковым именем (например, "1.png") не перезаписывали друг друга
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            
                // Собираем абсолютный физический путь на сервере: wwwroot/images/projects/имя_файла.png
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "certs");
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Физически сохраняем файл на жесткий диск сервера
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Записываем в модель относительный путь для HTML (то, что будет в теге <img src="...">)
                cert.BadgeImageUrl = "/images/certs/" + uniqueFileName;
            }

            _context.Certs.Add(cert);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(cert);
    }
    
    // --------------------- Форма создания работы ---------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WorkCreate(Work work, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            // Если пользователь выбрал и загрузил файл
            if (imageFile != null && imageFile.Length > 0)
            {
                // Генерируем уникальное имя файла, чтобы картинки с одинаковым именем (например, "1.png") не перезаписывали друг друга
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            
                // Собираем абсолютный физический путь на сервере: wwwroot/images/projects/имя_файла.png
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "works");
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Физически сохраняем файл на жесткий диск сервера
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Записываем в модель относительный путь для HTML (то, что будет в теге <img src="...">)
                work.CompanyLogo = "/images/works/" + uniqueFileName;
            }

            _context.Works.Add(work);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(work);
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
    
    // --------------------- Страница редактирование сертификата ---------------------
    [HttpGet]
    public async Task<IActionResult> CertEdit(int id)
    {
        var cert = await _context.Certs.FindAsync(id);
        
        if (cert == null)
        {
            return NotFound();
        }
        
        return View(cert);
    }
    
    // --------------------- Страница редактирования работы ---------------------
    [HttpGet]
    public async Task<IActionResult> WorkEdit(int id)
    {
        var work = await _context.Works.FindAsync(id);
        
        if (work == null)
        {
            return NotFound();
        }
        
        return View(work);
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
    
    // --------------------- Редактирование сертификата ---------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CertEdit(int id, Certifications cert, IFormFile? imageFile)
        {
            if (id != cert.Id) return NotFound();
    
            if (ModelState.IsValid)
            {
                try
                {
                    // Если загружен новый файл
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // 1. Удаляем старый файл с диска, если он существовал, чтобы не копить мусор
                        if (!string.IsNullOrEmpty(cert.BadgeImageUrl))
                        {
                            var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, cert.BadgeImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
    
                        // 2. Сохраняем новую картинку
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "certs");
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
    
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }
    
                        // Обновляем путь в модели
                        cert.BadgeImageUrl = "/images/cert/" + uniqueFileName;
                    }
                    else
                    {
                        // КРИТИЧЕСКИ ВАЖНО: Если файл не загружался, EF может затереть путь. 
                        // Чтобы этого не произошло, отслеживаем старое значение пути из базы данных.
                        _context.Entry(cert).Property(x => x.BadgeImageUrl).IsModified = imageFile == null ? false : true;
                    }
    
                    _context.Update(cert);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Certs.Any(e => e.Id == cert.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cert);
        }
    
    // --------------------- Редактирование работы ---------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WorkEdit(int id, Work work, IFormFile? imageFile)
    {
        if (id != work.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                // Если загружен новый файл
                if (imageFile != null && imageFile.Length > 0)
                {
                    // 1. Удаляем старый файл с диска, если он существовал, чтобы не копить мусор
                    if (!string.IsNullOrEmpty(work.CompanyLogo))
                    {
                        var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, work.CompanyLogo.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // 2. Сохраняем новую картинку
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "works");
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    // Обновляем путь в модели
                    work.CompanyLogo = "/images/works/" + uniqueFileName;
                }
                else
                {
                    // КРИТИЧЕСКИ ВАЖНО: Если файл не загружался, EF может затереть путь. 
                    // Чтобы этого не произошло, отслеживаем старое значение пути из базы данных.
                    _context.Entry(work).Property(x => x.CompanyLogo).IsModified = imageFile == null ? false : true;
                }

                _context.Update(work);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Projects.Any(e => e.Id == work.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(work);
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
    
    // --------------------- Удаление сертификата ---------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CertDelete(int id)
    {
        var cert = await _context.Certs.FindAsync(id);
        if (cert != null)
        {
            // Если у проекта был скриншот, удаляем его файл с диска
            if (!string.IsNullOrEmpty(cert.BadgeImageUrl))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, cert.BadgeImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Certs.Remove(cert);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
    
    // --------------------- Удаление работы ---------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WorkDelete(int id)
    {
        var work = await _context.Works.FindAsync(id);
        if (work != null)
        {
            // Если у проекта был скриншот, удаляем его файл с диска
            if (!string.IsNullOrEmpty(work.CompanyLogo))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, work.CompanyLogo.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Works.Remove(work);
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
