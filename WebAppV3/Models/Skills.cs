namespace WebAppV3.Models;
using System.ComponentModel.DataAnnotations;

public class Skills
{
    [Key]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Название навыка обязательно для заполнения")]
    [StringLength(100, ErrorMessage = "Название навыка не должно превышать 100 символов")]
    [Display(Name = "Skill Name")]
    public string SkillName { get; set; } = string.Empty;
    
    [Display(Name = "Категория (например: Backend, Frontend, GameDev)")]
    [StringLength(100, ErrorMessage = "Название категории не должно превышать 100 символов")]
    public string Category { get; set; } = "General";
}