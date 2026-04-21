using CommunityToolkit.Mvvm.ComponentModel;

namespace AppMobile.Models;

public partial class Student : ObservableObject
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public int? AbsenceId { get; set; }

    [ObservableProperty] private bool _isAbsent;
    [ObservableProperty] private bool _isLate;
}
