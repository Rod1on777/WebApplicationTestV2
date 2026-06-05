namespace WebAppV3.Models;
using System.ComponentModel.DataAnnotations;

public class Work
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string JobTitle { get; set; } = string.Empty;
    
    public string WorkTime { get; set; } = string.Empty;

    public string DetailedInfo { get; set; } = string.Empty;
    
    public string KeyAchievements { get; set; } = string.Empty;
    
    public string TechnologyStack { get; set; } = string.Empty;


    public string CompanyLogo { get; set; } = string.Empty;
}