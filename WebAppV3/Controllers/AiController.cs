using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppV3.Data; 
using WebAppV3.Models;
using WebAppV3.Services;
using HtmlAgilityPack;
using System.Text.Json;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Runtime.InteropServices;

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
    
    [HttpPost]
    public async Task<IActionResult> TailorResume([FromBody] JobAnalysisRequest model)
    {
        try
        {
            // 1. Читаем оригинальный HTML-файл твоего резюме
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "resume-template.html");
            if (!System.IO.File.Exists(filePath))
            {
                return BadRequest(new { error = "Исходный файл резюме не найден в wwwroot/templates/." });
            }
            string originalHtml = await System.IO.File.ReadAllTextAsync(filePath);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(originalHtml);

            // Находим элемент <body>
            var bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");
            if (bodyNode == null)
            {
                return BadRequest(new { error = "В шаблоне резюме не найден тег <body>." });
            }

            // Вытаскиваем ТОЛЬКО контент внутри body (без стилей из head)
            string bodyHtmlOnly = bodyNode.InnerHtml;

            // 3. Отправляем в Лямбду ТОЛЬКО контент body
            Console.WriteLine($"[INFO]: Отправка в Лямбду. Размер body: {bodyHtmlOnly.Length} символов (вместо {originalHtml.Length})");
            
            // 2. Отправляем запрос в Лямбду и получаем ЧИСТЫЙ JSON (благодаря нашему новому сервису)
            string aiJsonResponse = await _lambdaService.TailorResumeAsync(model.JobTitle, model.JobDescription, originalHtml);
            aiJsonResponse = aiJsonResponse.Trim();

            // Защита: Проверяем, что пришел именно JSON
            if (!aiJsonResponse.StartsWith("{"))
            {
                return Json(new { success = false, error = $"Облако вернуло ошибку вместо данных: {aiJsonResponse}" });
            }

            // 3. Десериализуем ответ от ИИ в C#-модель
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var aiData = JsonSerializer.Deserialize<TailorResponse>(aiJsonResponse);

            if (aiData == null)
            {
                return Json(new { success = false, error = "Не удалось распарсить структуру JSON от ИИ." });
            }

            // 4. Загружаем оригинальный HTML в HtmlAgilityPack DOM
            htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(originalHtml);

            // 5. ТОЧЕЧНАЯ ПРАВКА: Ищем элементы по классам и меняем только текст внутри них

            // Обновляем заголовок вакансии
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//*[contains(@class, 'job-title')]");
            if (titleNode != null && !string.IsNullOrEmpty(aiData.JobTitle))
            {
                titleNode.InnerHtml = aiData.JobTitle;
            }

            // Обновляем блок Summary (О себе)
            var summaryNode = htmlDoc.DocumentNode.SelectSingleNode("//*[contains(@class, 'summary-text')]");
            if (summaryNode != null && !string.IsNullOrEmpty(aiData.SummaryText))
            {
                summaryNode.InnerHtml = aiData.SummaryText;
            }
            
            Console.WriteLine($"[INFO]: пытаемся адаптировать скиллы CloudComputingSkill... {aiData.Skills.ProgrammingLanguagesSkill}");

            // Карта соответствия классов в HTML и свойств из JSON
            var skillUpdates = new Dictionary<string, string>
            {
                { "programming-languages-skill", aiData.Skills.ProgrammingLanguagesSkill },
                { "cloud-computing-skill", aiData.Skills.CloudComputingSkill },
                { "tools-and-invironments-skill", aiData.Skills.ToolsAndInvironmentsSkill },
                { "web-and-app-development-skill", aiData.Skills.WebAndAppDevelopmentSkill },
                { "databases-skill", aiData.Skills.DatabasesSkill },
                { "networking-and-security-skill", aiData.Skills.NetworkingAndSecuritySkill },
                { "other-skill", aiData.Skills.OtherSkill }
            };

            
            Console.WriteLine($"[INFO]: пытаемся адаптировать скиллы... {string.Join(", ", skillUpdates.Values)}");
            // Внедряем адаптированные навыки в HTML
            foreach (var update in skillUpdates)
            {
                if (!string.IsNullOrEmpty(update.Value))
                {
                    var skillNode = htmlDoc.DocumentNode.SelectSingleNode($"//*[contains(@class, '{update.Key}')]");
                    if (skillNode != null)
                    {
                        Console.WriteLine($"[INFO]: обновлён скилл {update.Key} = {update.Value}");
                        skillNode.InnerHtml = update.Value;
                    }
                }
            }

            // 6. Генерируем финальную HTML-строку со всеми изменениями
            string tailoredHtml = htmlDoc.DocumentNode.OuterHtml;

            var coverTemplatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "cover-template.html");
            string tailoredCoverHtml = string.Empty;

            if (System.IO.File.Exists(coverTemplatePath))
            {
                string originalCoverHtml = await System.IO.File.ReadAllTextAsync(coverTemplatePath);
                var coverDoc = new HtmlDocument();
                coverDoc.LoadHtml(originalCoverHtml);

                var coverMainNode = coverDoc.DocumentNode.SelectSingleNode("//*[contains(@class, 'cover-letter-main')]");
                if (coverMainNode != null && !string.IsNullOrEmpty(aiData.CoverLetter))
                {
                    // Форматируем текст: превращаем обычные переносы строк \n в HTML-теги <br/>
                    // Также заменяем переносы с дефисами на списки, если это необходимо
                    string formattedText = aiData.CoverLetter
                        .Replace("\r\n", "\n")
                        .Replace("\n", "<br/>");

                    coverMainNode.InnerHtml = formattedText;
                }
                tailoredCoverHtml = coverDoc.DocumentNode.OuterHtml;
            }
            
            // 🚀 ОТПРАВЛЯЕМ ГОТОВЫЙ HTML НА ФРОНТЕНД
            return Json(new { 
                success = true, 
                html = tailoredHtml, 
                coverHtml = tailoredCoverHtml 
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> TailorAndGeneratePdf([FromBody] JobAnalysisRequest model)
    {
        try
        {
            // 1. Читаем исходный файл разметки резюме
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "resume-template.html");
            string htmlContent = await System.IO.File.ReadAllTextAsync(filePath);

            // 2. Отправляем в нашу новую AWS Лямбду и получаем структурированный JSON изменений
            string aiJsonResponse = await _lambdaService.TailorResumeAsync(model.JobTitle, model.JobDescription, htmlContent);
            
            // 3. Парсим ответ от ИИ
            using var responseDoc = JsonDocument.Parse(aiJsonResponse);
            var root = responseDoc.RootElement;

            // 4. Загружаем исходный HTML в парсер DOM-дерева
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // 5. Правим данные «на лету» по классам и тегам
            
            // Правим заголовок вакансии (h2 class="job-title")
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h2[contains(@class, 'job-title')]");
            if (titleNode != null) titleNode.InnerHtml = root.GetProperty("jobTitle").GetString();

            // Правим текст "о себе" (p class="summary-text")
            var summaryNode = htmlDoc.DocumentNode.SelectSingleNode("//p[contains(@class, 'summary-text')]");
            if (summaryNode != null) summaryNode.InnerHtml = root.GetProperty("summaryText").GetString();

            // Правим блоки скиллов внутри секции skills
            var skillsObj = root.GetProperty("skills");
            string[] skillClasses = new[] {
                "programming-languages-skill", "cloud-computing-skill", "tools-and-invironments-skill",
                "web-and-app-development-skill", "databases-skill", "networking-and-security-skill", "other-skill"
            };

            foreach (var skillClass in skillClasses)
            {
                // Ищем любой элемент (обычно span или div) с соответствующим классом
                var skillNode = htmlDoc.DocumentNode.SelectSingleNode($"//*[contains(@class, '{skillClass}')]");
                if (skillNode != null && skillsObj.TryGetProperty(skillClass, out var updatedSkillText))
                {
                    skillNode.InnerHtml = updatedSkillText.GetString();
                }
            }

            // Получаем итоговую строку измененного HTML
            string finalHtml = htmlDoc.DocumentNode.OuterHtml;

            // 6. 🖨️ Конвертация измененного HTML в PDF файл
            byte[] pdfBytes = ConvertHtmlToPdf(finalHtml);

            // Возвращаем файл рекрутеру прямо в браузер в виде скачиваемого вложения!
            return File(pdfBytes, "application/pdf", $"Resume_{model.JobTitle.Replace(" ", "_")}.pdf");
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }
    
    
    private byte[] ConvertHtmlToPdf(string htmlContent)
    {
        // Скачиваем браузер Chromium при первом запуске (выполняется один раз)
        var browserFetcher = new BrowserFetcher();
        browserFetcher.DownloadAsync().Wait();

        using var browser = Puppeteer.LaunchAsync(new LaunchOptions { Headless = true }).Result;
        using var page = browser.NewPageAsync().Result;
    
        // Передаем наш отредактированный ИИ HTML-код в память вкладки
        page.SetContentAsync(htmlContent).Wait();
    
        // Генерируем PDF в массив байтов (A4, с отступами)
        var pdfOptions = new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true, // чтобы сохранились цвета фона и стили
            MarginOptions = new MarginOptions { Top = "10mm", Bottom = "10mm", Left = "10mm", Right = "10mm" }
        };

        return page.PdfDataAsync(pdfOptions).Result;
    }
    
    [HttpPost]
    public async Task<IActionResult> DownloadPdf([FromForm] string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            return BadRequest("HTML-контент пуст или не передан.");
        }

        try
        {
            string chromePath = "";

            // 1. Автоматически определяем ОС для кроссплатформенности
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Путь для твоей локальной Windows-машины
                chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
                if (!System.IO.File.Exists(chromePath))
                {
                    chromePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Стандартные пути для Chromium в Linux (AWS EC2)
                string[] linuxPaths = {
                    "/usr/bin/chromium",
                    "/usr/bin/chromium-browser",
                    "/usr/bin/google-chrome"
                };

                foreach (var path in linuxPaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        chromePath = path;
                        break;
                    }
                }
            }

            // Если ни на одной ОС браузер не нашли
            if (string.IsNullOrEmpty(chromePath) || !System.IO.File.Exists(chromePath))
            {
                return StatusCode(500, $"Браузер для рендеринга PDF не найден. Текущая ОС: {RuntimeInformation.OSDescription}");
            }

            // 2. Настраиваем запуск Puppeteer с флагами безопасности для Linux
            var launchOptions = new LaunchOptions 
            { 
                Headless = true,
                ExecutablePath = chromePath,
                // ⚠️ ВАЖНО ДЛЯ LINUX/EC2: Без этих аргументов Chrome внутри Linux-сервера 
                // от имени root или службы systemd может отказаться запускаться
                Args = new[] { 
                    "--no-sandbox", 
                    "--disable-setuid-sandbox", 
                    "--disable-dev-shm-usage" 
                }
            };
            
            await using var browser = await Puppeteer.LaunchAsync(launchOptions);
            await using var page = await browser.NewPageAsync();

            // 3. Загружаем изолированный HTML-код
            await page.SetContentAsync(htmlContent);

            // 4. Печать в PDF
            var pdfOptions = new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "0mm", Bottom = "0mm", Left = "0mm", Right = "0mm"
                }
            };

            byte[] pdfBytes = await page.PdfDataAsync(pdfOptions);

            string fileName = $"Resume_Tailored_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Критическая ошибка при генерации PDF на сервере: {ex.Message}");
        }
    }
}