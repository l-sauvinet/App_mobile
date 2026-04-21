using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppMobile.Models;
using AppMobile.Services;
using System.Collections.ObjectModel;

namespace AppMobile.ViewModels;

public partial class CoursesViewModel : ObservableObject
{
    private readonly CourseService _courseService;
    private readonly AuthService _auth;

    [ObservableProperty] private ObservableCollection<Course> _courses = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _welcomeMessage = string.Empty;

    public CoursesViewModel(CourseService courseService, AuthService auth)
    {
        _courseService = courseService;
        _auth = auth;
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
            var list = await _courseService.GetTeacherCoursesAsync();
            Courses = new ObservableCollection<Course>(list);
            if (Courses.Count == 0)
                ErrorMessage = "Aucun cours trouvé.";
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
    private async Task SelectCourseAsync(Course course)
    {
        await Shell.Current.GoToAsync($"attendance?courseId={course.Id}&courseName={Uri.EscapeDataString(course.Display)}&timeRange={Uri.EscapeDataString(course.TimeRange)}");
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync("//login");
    }
}
