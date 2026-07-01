using System.Text;
using System.Text.Json;
using WebAppV3.Models;

namespace WebAppV3.Services;

public class AwsLambdaService
{
    private readonly HttpClient _httpClient;
    private readonly string _lambdaUrl;
    private readonly string _tailorLambdaUrl;


    public AwsLambdaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromMinutes(3);
        
        var url = configuration["AWSLambda:URL"];
        
        if (string.IsNullOrWhiteSpace(url))
        {
            url = Environment.GetEnvironmentVariable("AWSLambda__URL");
        }
        
        if (string.IsNullOrWhiteSpace(url))
        {
            url = null;
        }
    
        _lambdaUrl = url;
        
        
        
        
        var tailorUrl = configuration["AWSLambda:TailorURL"];
        
        if (string.IsNullOrWhiteSpace(tailorUrl))
        {
            tailorUrl = Environment.GetEnvironmentVariable("AWSLambda__TailorURL");
        }
        
        if (string.IsNullOrWhiteSpace(tailorUrl))
        {
            tailorUrl = null;
        }
    
        _tailorLambdaUrl = tailorUrl;
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
            return new JobAnalysisResponse { Summary = $"Error Connecting to AI model: {_lambdaUrl}" };
        }
    }

    public async Task<string> TailorResumeAsync(string jobTitle, string jobDescription, string baseTemplate)
    {
        var payload = new
        {
            jobTitle = jobTitle,
            jobDescription = jobDescription,
            template = baseTemplate
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        // Отправляем строго на URL ВТОРОЙ Лямбды
        var response = await _httpClient.PostAsync(_tailorLambdaUrl, content);
        response.EnsureSuccessStatusCode();

        string rawResponse = await response.Content.ReadAsStringAsync();
        rawResponse = rawResponse.Trim();

        // Страховка от двойного экранирования строк ("{...}")
        if (rawResponse.StartsWith("\"") && rawResponse.EndsWith("\""))
        {
            try
            {
                rawResponse = JsonSerializer.Deserialize<string>(rawResponse) ?? rawResponse;
            }
            catch
            {
            }
        }

        using var doc = JsonDocument.Parse(rawResponse);

        // ВАРИАНТ А: Ответ обернут в AWS Proxy (ищем "body" без учета регистра)
        var root = doc.RootElement;
        if (root.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in root.EnumerateObject())
            {
                if (property.Name.Equals("body", StringComparison.OrdinalIgnoreCase))
                {
                    string innerJson = property.Value.GetString() ?? string.Empty;
                    return innerJson;
                }
            }

            // ВАРИАНТ Б: AWS не оборачивал ответ, и нам пришел прямой JSON от Gemini
            if (root.TryGetProperty("jobTitle", out _) || root.TryGetProperty("skills", out _))
            {
                Console.WriteLine("[INFO]: Лямбда вернула прямой JSON без обертки AWS. Используем его.");
                return rawResponse; // Отдаем весь текст целиком, так как это и есть нужный JSON
            }
        }

        return $"Error: Неизвестный формат ответа. Raw: {rawResponse}";
    }
}