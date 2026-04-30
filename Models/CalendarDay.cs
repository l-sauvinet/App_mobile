using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;

namespace AppMobile.Models;

public partial class CalendarDay : ObservableObject
{
    public DateTime Date { get; init; }

    [ObservableProperty] private bool _isSelected;

    public bool IsToday => Date.Date == DateTime.Today;
    public string ShortDay => Date.ToString("ddd", new CultureInfo("fr-FR"))[..3].ToUpper();
    public string DayNumber => Date.Day.ToString();
}
