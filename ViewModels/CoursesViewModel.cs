using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppMobile.Models;
using AppMobile.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace AppMobile.ViewModels;

public partial class CoursesViewModel : ObservableObject
{
    private readonly CourseService _courseService;
    private readonly AuthService _auth;
    private List<Course> _allCourses = [];
    private static readonly CultureInfo Fr = new("fr-FR");

    [ObservableProperty] private ObservableCollection<CalendarDay> _weekDays = [];
    [ObservableProperty] private ObservableCollection<Course> _coursesForDay = [];
    [ObservableProperty] private DateTime _selectedDate = DateTime.Today;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _welcomeMessage = string.Empty;
    [ObservableProperty] private string _monthYearLabel = string.Empty;
    [ObservableProperty] private string _selectedDayLabel = string.Empty;
    [ObservableProperty] private string _daySummaryLabel = string.Empty;

    public CoursesViewModel(CourseService courseService, AuthService auth)
    {
        _courseService = courseService;
        _auth = auth;
        RebuildCalendar();
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        RebuildCalendar();
        FilterCourses();
    }

    [RelayCommand]
    public async Task LoadCoursesAsync()
    {
        if (_auth.CurrentUser == null) return;

        WelcomeMessage = $"Bonjour, {_auth.CurrentUser.FirstName}";
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            _allCourses = await _courseService.GetTeacherCoursesAsync();
            FilterCourses();
        }
        catch (Exception)
        {
            ErrorMessage = "Erreur lors du chargement des cours.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SelectDay(CalendarDay day) => SelectedDate = day.Date;

    [RelayCommand]
    private void PreviousWeek() => SelectedDate = SelectedDate.AddDays(-7);

    [RelayCommand]
    private void NextWeek() => SelectedDate = SelectedDate.AddDays(7);

    [RelayCommand]
    private async Task SelectCourseAsync(Course course)
    {
        await Shell.Current.GoToAsync(
            $"attendance?courseId={course.Id}" +
            $"&courseName={Uri.EscapeDataString(course.Display)}" +
            $"&timeRange={Uri.EscapeDataString(course.TimeRange)}");
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync("//login");
    }

    private void RebuildCalendar()
    {
        MonthYearLabel = Fr.TextInfo.ToTitleCase(SelectedDate.ToString("MMMM yyyy", Fr));

        SelectedDayLabel = SelectedDate.Date == DateTime.Today
            ? "Aujourd'hui"
            : Fr.TextInfo.ToTitleCase(SelectedDate.ToString("dddd d MMMM", Fr));

        var start = SelectedDate.AddDays(-3);
        var days = new ObservableCollection<CalendarDay>();
        for (int i = 0; i < 7; i++)
        {
            var date = start.AddDays(i);
            days.Add(new CalendarDay { Date = date, IsSelected = date.Date == SelectedDate.Date });
        }
        WeekDays = days;
    }

    private void FilterCourses()
    {
        var filtered = _allCourses
            .Where(c => c.StartDatetime.Date == SelectedDate.Date)
            .OrderBy(c => c.StartDatetime)
            .ToList();

        CoursesForDay = new ObservableCollection<Course>(filtered);
        DaySummaryLabel = filtered.Count switch
        {
            0 => "Aucun cours",
            1 => "1 cours",
            _ => $"{filtered.Count} cours"
        };
    }
}
