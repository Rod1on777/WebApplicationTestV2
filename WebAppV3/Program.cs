using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAppV3.Data;
using WebAppV3.Models;
using WebAppV3.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        // Читаем данные, которые мы сохранили через user-secrets (или из appsettings.json)
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    });

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<WeatherService>();
builder.Services.AddHttpClient<CatFactsService>();

builder.Services.ConfigureApplicationCookie(options =>
{
    // Путь к странице входа (замени на свой, если у тебя используется стандартный Identity Razor Pages)
    options.LoginPath = "/Identity/Account/Login"; 
    
    // Путь к странице, если доступ запрещен
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    
    // Время жизни сессии авторизации
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    
    // Разрешать обновлять куки при активности пользователя
    options.SlidingExpiration = true;
});

var app = builder.Build();

app.UseStaticFiles();
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // 1. Создаем список всех необходимых ролей для нашего сайта
    string[] roleNames = { "Admin", "Moderator", "PremiumUser" };

    foreach (var roleName in roleNames)
    {
        // Проверяем каждую роль в базе, и если её нет — создаем
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // 2. Назначаем админа (как и раньше)
    string? adminEmail = builder.Configuration["AdminSettings:Email"];
    if (!string.IsNullOrEmpty(adminEmail))
    {
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
    
    
    // ------ Просто добавление дефолтных скиллов при первой инициализации базы данных ------
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!context.Skills.Any())
    {
        context.Skills.AddRange(
            new Skills { SkillName = "C#" },
            new Skills { SkillName = ".NET Core / ASP.NET MVC" },
            new Skills { SkillName = "Entity Framework Core" },
            new Skills { SkillName = "SQLite" },
            new Skills { SkillName = "Git & GitHub" }
        );

        // Физически сохраняем строки в файл app.db
        await context.SaveChangesAsync();
    }
    
    // ------ Просто добавление дефолтных Работ при первой инициализации базы данных ------
    if (!context.Works.Any())
    {
        context.Works.AddRange(
            new Work { CompanyName = "Wells Fargo", JobTitle = "Software Engineering Simulation Internship", WorkTime = "October 2023 - November 2023", DetailedInfo = "Simulation Internship...", KeyAchievements = "Conducted a thorough analysis", TechnologyStack = "Git, Java" }
        );

        // Физически сохраняем строки в файл app.db
        await context.SaveChangesAsync();
    }
    
    if (!context.Certs.Any())
    {
        context.Certs.AddRange(
            new Certifications
            {
                Name = "AWS Certified Solutions Architect – Associate",
                IssuingOrganization = "Amazon Web Services (AWS)",
                IssueDate = new DateTime(2026, 03, 1), // Март 2026
                CredentialId = "AWS-SAA-C03-SAMPLE", // Сюда потом вставишь свой реальный ID бэйджа
                VerificationLink = "https://www.credly.com/org/amazon-web-services/badge/aws-certified-solutions-architect-associate",
                BadgeImageUrl = "/images/certs/aws-saa.png", // Путь к скачанной с Credly иконке в wwwroot
                Description = "Подтверждает навыки проектирования отказоустойчивых, высокодоступных и экономически эффективных распределенных систем на платформе AWS. Включает работу с Compute, Storage, Database и Networking сервисами."
            });

        // Физически сохраняем строки в файл app.db
        await context.SaveChangesAsync();
    }
}



app.Run();