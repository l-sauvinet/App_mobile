namespace AppMobile.Models;

public class RoomReservation
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public DateTime StartDatetime { get; set; }
    public DateTime EndDatetime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string TimeRange => $"{StartDatetime:HH:mm} → {EndDatetime:HH:mm}";
    public string DateDisplay => StartDatetime.ToString("dddd dd MMMM", new System.Globalization.CultureInfo("fr-FR"));
}
