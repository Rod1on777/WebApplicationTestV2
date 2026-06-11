using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppV3.Data; 
using WebAppV3.Models;
using WebAppV3.Services;

namespace WebAppV3.Controllers;

public class AiController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly AwsLambdaService _lambdaService;

    public AiController(ApplicationDbContext context, AwsLambdaService lambdaService)
    {
        _context = context;
        _lambdaService = lambdaService;
    }

    // GET: Отобразить страницу с формой анализа
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // POST: Обработать форму асинхронно (через AJAX)
    [HttpPost]
    public async Task<IActionResult> Analyze([FromBody] JobAnalysisRequest request)
    {
        if (string.IsNullOrEmpty(request.JobDescription))
        {
            return BadRequest(new { error = "Описание вакансии не может быть пустым" });
        }

        // 1. Вытаскиваем твои реальные навыки из базы данных (только имена)
        var mySkills = await _context.Skills
            .Select(s => s.SkillName)
            .ToListAsync();

        // Если база пуста, закинем дефолтный стек для подстраховки
        if (!mySkills.Any())
        {
            mySkills = new List<string> { "Java", "C#", "SQL", "Git", "Spring Boot", "Unity", "AWS" };
        }

        // 2. Отправляем данные в Лямбду и ждем вердикт Gemini
        var result = await _lambdaService.AnalyzeJobAsync(request.JobTitle, request.JobDescription, mySkills);

        if (result == null)
        {
            return StatusCode(500, new { error = "Unable to get a response from the AI model" });
        }
        
        return Json(result);
    }
}