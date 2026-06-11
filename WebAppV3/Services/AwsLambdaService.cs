using System.Text;
using System.Text.Json;
using WebAppV3.Models;

namespace WebAppV3.Services;

public class AwsLambdaService
{
    private readonly HttpClient _httpClient;
    private readonly string _lambdaUrl;

    public AwsLambdaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _lambdaUrl = configuration["AWSLambda:URL"] 
                     ?? throw new ArgumentNullException("AWSLambda:URL не найден в конфигурации appsettings.json или User Secrets");
    }

    public async Task<JobAnalysisResponse?> AnalyzeJobAsync(string title, string description, List<string> skills)
    {
        var payload = new
        {
            jobTitle = title,
            jobDescription = description,
            developerSkills = skills
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            // Делаем синхронный POST-запрос к Лямбде (ждем ответ от Gemini)
            var response = await _httpClient.PostAsync(_lambdaUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                return new JobAnalysisResponse { Summary = $"AWS Lambda Error: {response.StatusCode}" };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            return JsonSerializer.Deserialize<JobAnalysisResponse>(jsonResponse, options);
        }
        catch (Exception ex)
        {
            return new JobAnalysisResponse { Summary = $"Error Connecting to AI model: {ex.Message}" };
        }
    }
}