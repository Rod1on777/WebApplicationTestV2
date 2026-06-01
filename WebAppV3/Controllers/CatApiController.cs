using Microsoft.AspNetCore.Mvc;
using WebAppV3.Services;

namespace WebAppV3.Controllers;

[ApiController]
[Route("api/catfacts")] // Путь к нашему API

public class CatApiController : Controller
{ 
    private readonly CatFactsService _catFactsService;

    public CatApiController(CatFactsService catFactsService)
    {
        _catFactsService = catFactsService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetCats()
    {

        var catFact = await _catFactsService.GetCatFactsAsync();

        if (catFact == null)
        {
            return StatusCode(500, "Не удалось получить данные фактов о котах.");
        }

        // Возвращаем JSON ответ
        return Ok(new
        {
            fact = $"{catFact.CatFact}",
            length = $"{catFact.CatFactLength}"
        });
    }
}