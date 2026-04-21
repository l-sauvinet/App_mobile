using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppMobile.Models;
using AppMobile.Services;
using System.Collections.ObjectModel;

namespace AppMobile.ViewModels;

[QueryProperty(nameof(CourseId), "courseId")]
[QueryProperty(nameof(CourseName), "courseName")]
[QueryProperty(nameof(TimeRange), "timeRange")]
public partial class AttendanceViewModel : ObservableObject
{
    private readonly AbsenceService _absenceService;

    [ObservableProperty] private int _courseId;
    [ObservableProperty] private string _courseName = string.Empty;
    [ObservableProperty] private string _timeRange = string.Empty;
    [ObservableProperty] private ObservableCollection<Student> _students = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _successMessage = string.Empty;

    public AttendanceViewModel(AbsenceService absenceService)
    {
        _absenceService = absenceService;
    }

    partial void OnCourseIdChanged(int value)
    {
        if (value > 0)
            _ = LoadStudentsAsync();
    }

    [RelayCommand]
    public async Task LoadStudentsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var list = await _absenceService.GetStudentsForCourseAsync(CourseId);
            Students = new ObservableCollection<Student>(list);
            if (Students.Count == 0)
                ErrorMessage = "Aucun élève trouvé pour ce cours.";
        }
        catch (Exception)
        {
            ErrorMessage = "Erreur lors du chargement des élèves.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ToggleAbsentAsync(Student student)
    {
        try
        {
            if (student.IsAbsent && !student.IsLate)
            {
                await _absenceService.RemoveAbsenceAsync(student.AbsenceId!.Value);
                student.AbsenceId = null;
                student.IsAbsent = false;
                student.IsLate = false;
            }
            else
            {
                if (student.AbsenceId.HasValue)
                    await _absenceService.RemoveAbsenceAsync(student.AbsenceId.Value);
                var newId = await _absenceService.RecordAbsenceAsync(student.Id, CourseId, false);
                student.AbsenceId = newId;
                student.IsAbsent = true;
                student.IsLate = false;
            }

            ShowSuccess("Enregistré.");
        }
        catch (Exception)
        {
            ErrorMessage = "Erreur lors de l'enregistrement.";
        }
    }

    [RelayCommand]
    private async Task ToggleLateAsync(Student student)
    {
        try
        {
            if (student.IsLate)
            {
                await _absenceService.RemoveAbsenceAsync(student.AbsenceId!.Value);
                student.AbsenceId = null;
                student.IsAbsent = false;
                student.IsLate = false;
            }
            else
            {
                if (student.AbsenceId.HasValue)
                    await _absenceService.RemoveAbsenceAsync(student.AbsenceId.Value);
                var newId = await _absenceService.RecordAbsenceAsync(student.Id, CourseId, true);
                student.AbsenceId = newId;
                student.IsAbsent = true;
                student.IsLate = true;
            }

            ShowSuccess("Enregistré.");
        }
        catch (Exception)
        {
            ErrorMessage = "Erreur lors de l'enregistrement.";
        }
    }

    private void ShowSuccess(string msg)
    {
        SuccessMessage = msg;
        Task.Delay(2000).ContinueWith(_ => MainThread.BeginInvokeOnMainThread(() => SuccessMessage = string.Empty));
    }
}
