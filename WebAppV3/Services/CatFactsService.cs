using WebAppV3.Models;

namespace WebAppV3.Services;
using System.Text.Json;

public class CatFactsService
{
    private readonly HttpClient _httpClient;
    
    // Внедряем HttpClient через конструктор (Dependency Injection)
    public CatFactsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<CatFactResponse?> GetCatFactsAsync()
    {
        // URL API
        var url = $"https://catfact.ninja/fact";

        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var catFactData = JsonSerializer.Deserialize<CatFactResponse>(jsonString);

            return catFactData;
        }
        catch (Exception ex)
        {
            // Выведет точную причину ошибки в окно Run / Console в Rider
            Console.WriteLine($"Ошибка при парсинге фактов о котах: {ex.Message}");
            return null;
        }
    }
}