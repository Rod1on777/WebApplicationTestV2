namespace WebAppV3.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppV3.Data;
using WebAppV3.Models;

public class DbAccessController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    
    public DbAccessController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }
    
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
}