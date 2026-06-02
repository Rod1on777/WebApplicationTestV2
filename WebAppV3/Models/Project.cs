namespace WebAppV3.Models;
using System.ComponentModel.DataAnnotations;

public class Project
{
    [Key] // Указывает, что это первичный ключ (ID), он будет автоинкрементироваться
    public int Id { get; set; }

    [Required] // Поле обязательно для заполнения
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(250)]
    public string InfoPreview { get; set; } = string.Empty;

    public string DetailedInfo { get; set; } = string.Empty;

    public string TechnologyStack { get; set; } = string.Empty;

    public string SourceCodeLink { get; set; } = string.Empty;

    public string Screenshots { get; set; } = string.Empty;

    public int TeamMembersCount { get; set; }

    public string RoleInTeam { get; set; } = string.Empty;

    public string DevelopmentTime { get; set; } = string.Empty;

    public string ProjectFeatures { get; set; } = string.Empty;
}