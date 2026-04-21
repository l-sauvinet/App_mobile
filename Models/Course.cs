namespace AppMobile.Models;

public class Course
{
    public int Id { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public DateTime StartDatetime { get; set; }
    public DateTime EndDatetime { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string Display => $"{SubjectName} - {ClassName}";
    public string TimeRange => $"{StartDatetime:HH:mm} → {EndDatetime:HH:mm}";
    public string DateDisplay => StartDatetime.ToString("dddd dd MMMM", new System.Globalization.CultureInfo("fr-FR"));
}
