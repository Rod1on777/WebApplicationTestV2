using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace WebAppV3.Data;
using WebAppV3.Models;

public class ApplicationDbContext : IdentityDbContext
{
    // Конструктор принимает настройки (например, строку подключения) от системы
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    // Это свойство — наша будущая таблица проектов в базе данных
    public DbSet<Project> Projects { get; set; }
    
    public DbSet<Skills> Skills { get; set; }
    
    public DbSet<Work> Works { get; set; }
    
    public DbSet<Certifications> Certs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        base.OnModelCreating(modelBuilder);

        // Наполняем таблицу стартовыми данными
        modelBuilder.Entity<Project>().HasData(
            new Project
            {
                Id = 1,
                Name = "Celestial Light",
                InfoPreview = "RPG Minecraft server with complex custom mechanics and custom Java plugins.",
                DetailedInfo = "Celestial Light is a feature-rich RPG Minecraft server. It utilizes advanced custom plugins to handle economies, bespoke combat encounters, and unique item generation.",
                TechnologyStack = "Java, Paper/Spigot API, MySQL, MythicMobs, ItemsAdder",
                SourceCodeLink = "https://github.com/yourusername/celestial-light",
                Screenshots = "/images/projects/celestial.png",
                TeamMembersCount = 1,
                RoleInTeam = "Lead Administrator / Core Backend Developer",
                DevelopmentTime = "6 months",
                ProjectFeatures = "Custom stats system, automated item interaction signs, complex mob AI skills."
            },
            new Project
            {
                Id = 2,
                Name = "Surculus",
                InfoPreview = "A solo-developed RPG combining souls-like combat elements with cozy farming simulators.",
                DetailedInfo = "Surculus is a hybrid single-player RPG game developed in Unity. It seamlessly blends challenging, precise souls-like combat mechanics with defensive building and immersive farming systems.",
                TechnologyStack = "C#, Unity, AR Foundation, Blender",
                SourceCodeLink = "https://github.com/yourusername/surculus-rpg",
                Screenshots = "/images/projects/surculus.png",
                TeamMembersCount = 1,
                RoleInTeam = "Solo Game Designer & Developer",
                DevelopmentTime = "12 months",
                ProjectFeatures = "Custom physics-based character controller, advanced state-machine enemy AI, dynamic grid farming."
            }
        );
    }
}