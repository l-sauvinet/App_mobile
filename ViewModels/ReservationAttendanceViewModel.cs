using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppMobile.Models;
using AppMobile.Services;
using System.Collections.ObjectModel;

namespace AppMobile.ViewModels;

public partial class ReservationAttendanceViewModel : ObservableObject
{
    private readonly ReservationAbsenceService _service;
    private int _reservationId;

    [ObservableProperty] private string _roomName = string.Empty;
    [ObservableProperty] private string _timeRange = string.Empty;
    [ObservableProperty] private string _className = string.Empty;
    [ObservableProperty] private ObservableCollection<ReservationStudent> _students = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _successMessage = string.Empty;

    public ReservationAttendanceViewModel(ReservationAbsenceService service)
    {
        _service = service;
    }

    public void SetReservation(RoomReservation reservation)
    {
        _reservationId = reservation.Id;
        RoomName = reservation.RoomName;
        TimeRange = reservation.TimeRange;
        ClassName = reservation.ClassName;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (_reservationId == 0) return;
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var list = await _service.GetStudentsAsync(_reservationId);
            Students = new ObservableCollection<ReservationStudent>(list);
            if (Students.Count == 0)
                ErrorMessage = "Aucun élève trouvé pour cette classe.";
        }
        catch (Exception ex) { ErrorMessage = $"Erreur : {ex.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task MarkAbsentAsync(ReservationStudent student)
    {
        try
        {
            var id = await _service.RecordAbsenceAsync(student.Id, _reservationId, false, 0);
            student.AbsenceId = id;
            student.IsAbsent = true;
            student.IsLate = false;
            student.DelayMinutes = 0;
            ShowSuccess("Absence enregistrée.");
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    [RelayCommand]
    private async Task MarkLateAsync(ReservationStudent student)
    {
        var result = await Shell.Current.DisplayPromptAsync(
            "Durée du retard",
            "Entrez la durée en minutes :",
            "Enregistrer", "Annuler",
            placeholder: "ex: 15",
            keyboard: Keyboard.Numeric);

        if (result == null) return;

        var minutes = int.TryParse(result, out var m) ? Math.Max(0, m) : 0;

        try
        {
            var id = await _service.RecordAbsenceAsync(student.Id, _reservationId, true, minutes);
            student.AbsenceId = id;
            student.IsAbsent = true;
            student.IsLate = true;
            student.DelayMinutes = minutes;
            ShowSuccess("Retard enregistré.");
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    [RelayCommand]
    private async Task EditAsync(ReservationStudent student)
    {
        var options = student.IsLate
            ? new[] { "Modifier la durée", "Mettre absent à la place", "Supprimer" }
            : new[] { "Mettre en retard à la place", "Supprimer" };

        var action = await Shell.Current.DisplayActionSheet("Modifier", "Annuler", null, options);
        if (action == null || action == "Annuler") return;

        try
        {
            switch (action)
            {
                case "Modifier la durée":
                    var r1 = await Shell.Current.DisplayPromptAsync(
                        "Nouvelle durée", "Durée en minutes :",
                        "Enregistrer", "Annuler",
                        initialValue: student.DelayMinutes.ToString(),
                        keyboard: Keyboard.Numeric);
                    if (r1 == null) return;
                    var newMin = int.TryParse(r1, out var nm) ? Math.Max(0, nm) : 0;
                    await _service.RemoveAbsenceAsync(student.AbsenceId!.Value);
                    var id1 = await _service.RecordAbsenceAsync(student.Id, _reservationId, true, newMin);
                    student.AbsenceId = id1;
                    student.DelayMinutes = newMin;
                    ShowSuccess("Modifié.");
                    break;

                case "Mettre absent à la place":
                    await _service.RemoveAbsenceAsync(student.AbsenceId!.Value);
                    var id2 = await _service.RecordAbsenceAsync(student.Id, _reservationId, false, 0);
                    student.AbsenceId = id2;
                    student.IsLate = false;
                    student.DelayMinutes = 0;
                    ShowSuccess("Modifié.");
                    break;

                case "Mettre en retard à la place":
                    var r2 = await Shell.Current.DisplayPromptAsync(
                        "Durée du retard", "Durée en minutes :",
                        "Enregistrer", "Annuler",
                        placeholder: "ex: 15",
                        keyboard: Keyboard.Numeric);
                    if (r2 == null) return;
                    var min2 = int.TryParse(r2, out var m2) ? Math.Max(0, m2) : 0;
                    await _service.RemoveAbsenceAsync(student.AbsenceId!.Value);
                    var id3 = await _service.RecordAbsenceAsync(student.Id, _reservationId, true, min2);
                    student.AbsenceId = id3;
                    student.IsLate = true;
                    student.DelayMinutes = min2;
                    ShowSuccess("Modifié.");
                    break;

                case "Supprimer":
                    await _service.RemoveAbsenceAsync(student.AbsenceId!.Value);
                    student.AbsenceId = null;
                    student.IsAbsent = false;
                    student.IsLate = false;
                    student.DelayMinutes = 0;
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
