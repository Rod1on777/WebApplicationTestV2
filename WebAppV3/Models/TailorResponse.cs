namespace WebAppV3.Models;

public class TailorResponse
{
    public string JobTitle { get; set; } = string.Empty;
    public string SummaryText { get; set; } = string.Empty;
    public SkillsResponse Skills { get; set; } = new();
}

public class SkillsResponse
{
    public string ProgrammingLanguagesSkill { get; set; } = string.Empty;
    public string CloudComputingSkill { get; set; } = string.Empty;
    public string ToolsAndInvironmentsSkill { get; set; } = string.Empty;
    public string WebAndAppDevelopmentSkill { get; set; } = string.Empty;
    public string DatabasesSkill { get; set; } = string.Empty;
    public string NetworkingAndSecuritySkill { get; set; } = string.Empty;
    public string OtherSkill { get; set; } = string.Empty;
}