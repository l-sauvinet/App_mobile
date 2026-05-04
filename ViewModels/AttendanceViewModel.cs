using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppMobile.Models;
using AppMobile.Services;
using System.Collections.ObjectModel;

namespace AppMobile.ViewModels;

public partial class AttendanceViewModel : ObservableObject
{
    private readonly AbsenceService _absenceService;
    private int _courseId;

    [ObservableProperty] private string _courseName = string.Empty;
    [ObservableProperty] private string _timeRange = string.Empty;
    [ObservableProperty] private ObservableCollection<Student> _students = [];
    [ObservableProperty] private ObservableCollection<SchoolClass> _classes = [];
    [ObservableProperty] private SchoolClass? _selectedClass;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _successMessage = string.Empty;

    public AttendanceViewModel(AbsenceService absenceService)
    {
        _absenceService = absenceService;
    }

    public void SetCourse(int courseId, string courseName, string timeRange)
    {
        _courseId = courseId;
        CourseName = courseName;
        TimeRange = timeRange;
    }

    partial void OnSelectedClassChanged(SchoolClass? value)
    {
        if (_courseId > 0)
            _ = LoadStudentsAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (_courseId == 0) return;
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var studentsTask = _absenceService.GetStudentsForCourseAsync(_courseId);
            var classesTask = _absenceService.GetTeacherClassesAsync();
            await Task.WhenAll(studentsTask, classesTask);
            Students = new ObservableCollection<Student>(studentsTask.Result);
            Classes = new ObservableCollection<SchoolClass>(classesTask.Result);
            if (Students.Count == 0)
                ErrorMessage = "Aucun élève trouvé pour ce cours.";
        }
        catch (Exception ex) { ErrorMessage = $"Erreur : {ex.Message}"; }
        finally { IsLoading = false; }
    }

    private async Task LoadStudentsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var list = await _absenceService.GetStudentsForCourseAsync(_courseId, SelectedClass?.Id);
            Students = new ObservableCollection<Student>(list);
            if (Students.Count == 0)
                ErrorMessage = "Aucun élève trouvé pour cette classe.";
        }
        catch (Exception ex) { ErrorMessage = $"Erreur : {ex.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task MarkAbsentAsync(Student student)
    {
        try
        {
            var id = await _absenceService.RecordAbsenceAsync(student.Id, _courseId, false);
            student.AbsenceId = id;
            student.IsAbsent = true;
            student.IsLate = false;
            ShowSuccess("Absence enregistrée.");
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    [RelayCommand]
    private async Task MarkLateAsync(Student student)
    {
        try
        {
            var id = await _absenceService.RecordAbsenceAsync(student.Id, _courseId, true);
            student.AbsenceId = id;
            student.IsAbsent = true;
            student.IsLate = true;
            ShowSuccess("Retard enregistré.");
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    [RelayCommand]
    private async Task EditAsync(Student student)
    {
        var options = student.IsLate
            ? new[] { "Mettre absent à la place", "Supprimer" }
            : new[] { "Mettre en retard à la place", "Supprimer" };

        var action = await Shell.Current.DisplayActionSheet("Modifier", "Annuler", null, options);
        if (action == null || action == "Annuler") return;

        try
        {
            switch (action)
            {
                case "Mettre absent à la place":
                    await _absenceService.RemoveAbsenceAsync(student.AbsenceId!.Value);
                    var id1 = await _absenceService.RecordAbsenceAsync(student.Id, _courseId, false);
                    student.AbsenceId = id1;
                    student.IsLate = false;
                    ShowSuccess("Modifié.");
                    break;

                case "Mettre en retard à la place":
                    await _absenceService.RemoveAbsenceAsync(student.AbsenceId!.Value);
                    var id2 = await _absenceService.RecordAbsenceAsync(student.Id, _courseId, true);
                    student.AbsenceId = id2;
                    student.IsLate = true;
                    ShowSuccess("Modifié.");
                    break;

                case "Supprimer":
                    await _absenceService.RemoveAbsenceAsync(student.AbsenceId!.Value);
                    student.AbsenceId = null;
                    student.IsAbsent = false;
                    student.IsLate = false;
                    ShowSuccess("Supprimé.");
                    break;
            }
        }
        catch { ErrorMessage = "Erreur lors de la modification."; }
    }

    [RelayCommand]
    private static async Task CloseAsync() =>
        await Shell.Current.Navigation.PopModalAsync();

    private void ShowSuccess(string msg)
    {
        SuccessMessage = msg;
        Task.Delay(2000).ContinueWith(_ => MainThread.BeginInvokeOnMainThread(() => SuccessMessage = string.Empty));
    }
}
