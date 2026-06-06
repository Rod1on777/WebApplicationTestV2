namespace WebAppV3.Models;
using System.ComponentModel.DataAnnotations;

public class Certifications
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Название сертификации обязательно")]
    [Display(Name = "Название сертификата")]
    public string Name { get; set; } = string.Empty; // Например: AWS Certified Solutions Architect – Associate

    [Required(ErrorMessage = "Укажите организацию")]
    [Display(Name = "Кто выдал")]
    public string IssuingOrganization { get; set; } = string.Empty; // Например: Amazon Web Services (AWS)

    [Required(ErrorMessage = "Дата выдачи обязательна")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата получения")]
    public DateTime IssueDate { get; set; }
    
    [Display(Name = "Идентификатор сертификата (Credential ID)")]
    public string? CredentialId { get; set; } // Уникальный код AWS (например, b8d3... или из URL Credly)

    [Required(ErrorMessage = "Ссылка на верификацию обязательна")]
    [Url(ErrorMessage = "Неверный формат ссылки")]
    [Display(Name = "Ссылка на Credly (Verification URL)")]
    public string VerificationLink { get; set; } = string.Empty; // Прямой линк на твой публичный бейдж для проверки

    [Display(Name = "Ссылка на иконку/логотип бейджа")]
    public string? BadgeImageUrl { get; set; } // Ссылка на картинку AWS бейджа (можно скачать с Credly и положить в wwwroot/images/certs)

    [Display(Name = "Краткое описание / Проверяемые навыки")]
    public string? Description { get; set; } // Коротко о том, какие навыки подтверждены (Compute, Storage, Networking, Security)
}