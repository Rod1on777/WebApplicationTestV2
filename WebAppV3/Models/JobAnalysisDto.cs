namespace WebAppV3.Models;

// Данные, которые вводит пользователь в форму
public class JobAnalysisRequest
{
    public string JobTitle { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
}

// Структура ответа, которую нам возвращает Gemini через Лямбду
public class JobAnalysisResponse
{
    public int MatchPercentage { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string StrongPoints { get; set; } = string.Empty;
    public string MissingSkills { get; set; } = string.Empty;
}