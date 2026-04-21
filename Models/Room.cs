namespace AppMobile.Models;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Building { get; set; } = string.Empty;
    public string Display => string.IsNullOrEmpty(Building) ? Name : $"{Name} ({Building})";
}
