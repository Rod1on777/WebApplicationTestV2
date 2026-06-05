namespace WebAppV3.Models;

public class MainPageViewModel
{
    public CurrentWeather? Weather { get; set; }
    public CatFactResponse? CatFact { get; set; }
    
    public List<Project> Projects { get; set; } = new List<Project>();
    
    public List<Skills> Skills { get; set; } = new List<Skills>();
    
    public List<Work> Works { get; set; } = new List<Work>();
}