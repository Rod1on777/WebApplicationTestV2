using System.Text.Json.Serialization;

namespace WebAppV3.Models;

public class TailorResponse
{
    [JsonPropertyName("jobTitle")]
    public string JobTitle { get; set; } = string.Empty;
    
    [JsonPropertyName("summaryText")]
    public string SummaryText { get; set; } = string.Empty;
    
    [JsonPropertyName("skills")]
    public SkillsResponse Skills { get; set; } = new();
    
    [JsonPropertyName("coverLetter")]
    public string CoverLetter { get; set; } = string.Empty;
}

public class SkillsResponse
{
    [JsonPropertyName("programming-languages-skill")]
    public string ProgrammingLanguagesSkill { get; set; } = string.Empty;
    [JsonPropertyName("cloud-computing-skill")]
    public string CloudComputingSkill { get; set; } = string.Empty;
    [JsonPropertyName("tools-and-invironments-skill")]
    public string ToolsAndInvironmentsSkill { get; set; } = string.Empty;
    [JsonPropertyName("web-and-app-development-skill")]
    public string WebAndAppDevelopmentSkill { get; set; } = string.Empty;
    [JsonPropertyName("databases-skill")]
    public string DatabasesSkill { get; set; } = string.Empty;
    [JsonPropertyName("networking-and-security-skill")]
    public string NetworkingAndSecuritySkill { get; set; } = string.Empty;
    [JsonPropertyName("other-skill")]
    public string OtherSkill { get; set; } = string.Empty;
}