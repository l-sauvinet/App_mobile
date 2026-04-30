using CommunityToolkit.Mvvm.ComponentModel;

namespace AppMobile.Models;

public partial class ReservationStudent : ObservableObject
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public int? AbsenceId { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DelayDisplay))]
    private int _delayMinutes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasRecord))]
    [NotifyPropertyChangedFor(nameof(NoRecord))]
    [NotifyPropertyChangedFor(nameof(IsAbsentOnly))]
    private bool _isAbsent;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAbsentOnly))]
    private bool _isLate;

    public bool HasRecord    => IsAbsent;
    public bool NoRecord     => !IsAbsent;
    public bool IsAbsentOnly => IsAbsent && !IsLate;

    public string DelayDisplay
    {
        get
        {
            if (DelayMinutes <= 0) return string.Empty;
            var h = DelayMinutes / 60;
            var m = DelayMinutes % 60;
            if (h > 0 && m > 0) return $"{h}h{m:00}";
            if (h > 0) return $"{h}h";
            return $"{DelayMinutes}min";
        }
    }
}
