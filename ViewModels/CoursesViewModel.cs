using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppMobile.Models;
using AppMobile.Services;
using AppMobile.Views;
using System.Collections.ObjectModel;
using System.Globalization;

namespace AppMobile.ViewModels;

public partial class CoursesViewModel : ObservableObject
{
    private readonly CourseService _courseService;
    private readonly AuthService _auth;
    private readonly RoomService _roomService;
    private readonly IServiceProvider _services;
    private List<Course> _allCourses = [];
    private List<RoomReservation> _allReservations = [];
    private static readonly CultureInfo Fr = new("fr-FR");

    [ObservableProperty] private ObservableCollection<CalendarDay> _weekDays = [];
    [ObservableProperty] private ObservableCollection<Course> _coursesForDay = [];
    [ObservableProperty] private ObservableCollection<RoomReservation> _reservationsForDay = [];
    [ObservableProperty] private DateTime _selectedDate = DateTime.Today;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isEmpty;
    [ObservableProperty] private bool _hasReservationsForDay;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _welcomeMessage = string.Empty;
    [ObservableProperty] private string _monthYearLabel = string.Empty;
    [ObservableProperty] private string _selectedDayLabel = string.Empty;
    [ObservableProperty] private string _daySummaryLabel = string.Empty;

    public CoursesViewModel(CourseService courseService, AuthService auth, RoomService roomService, IServiceProvider services)
    {
        _courseService = courseService;
        _auth = auth;
        _roomService = roomService;
        _services = services;
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
            _allReservations = await _roomService.GetMyReservationsAsync();
            FilterCourses();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors du chargement : {ex.Message}";
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
        var vm = _services.GetRequiredService<AttendanceViewModel>();
        vm.SetCourse(course.Id, course.Display, course.TimeRange);
        var page = new AttendancePage(vm);
        await Shell.Current.Navigation.PushModalAsync(page, animated: true);
        await vm.LoadAsync();
    }

    [RelayCommand]
    private async Task SelectReservationAsync(RoomReservation reservation)
    {
        if (reservation.ClassId == null)
        {
            await Shell.Current.DisplayAlert("Aucune classe", "Cette réservation n'a pas de classe assignée.", "OK");
            return;
        }
        var vm = _services.GetRequiredService<ReservationAttendanceViewModel>();
        vm.SetReservation(reservation);
        var page = new ReservationAttendancePage(vm);
        await Shell.Current.Navigation.PushModalAsync(page, animated: true);
        await vm.LoadAsync();
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
        var courses = _allCourses
            .Where(c => c.StartDatetime.Date == SelectedDate.Date)
            .OrderBy(c => c.StartDatetime)
            .ToList();

        var reservations = _allReservations
            .Where(r => r.StartDatetime.Date == SelectedDate.Date)
            .OrderBy(r => r.StartDatetime)
            .ToList();

        CoursesForDay = new ObservableCollection<Course>(courses);
        ReservationsForDay = new ObservableCollection<RoomReservation>(reservations);
        HasReservationsForDay = reservations.Count > 0;
        IsEmpty = courses.Count == 0 && reservations.Count == 0;

        var parts = new List<string>();
        if (courses.Count > 0) parts.Add(courses.Count == 1 ? "1 cours" : $"{courses.Count} cours");
        if (reservations.Count > 0) parts.Add(reservations.Count == 1 ? "1 réservation" : $"{reservations.Count} réservations");
        DaySummaryLabel = parts.Count > 0 ? string.Join(" · ", parts) : "Aucun cours";
    }
}
