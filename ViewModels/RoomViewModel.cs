using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppMobile.Models;
using AppMobile.Services;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace AppMobile.ViewModels;

public partial class RoomViewModel : ObservableObject
{
    private readonly RoomService _roomService;
    private readonly AbsenceService _absenceService;

    [ObservableProperty] private ObservableCollection<Room> _rooms = [];
    [ObservableProperty] private ObservableCollection<RoomReservation> _myReservations = [];
    [ObservableProperty] private ObservableCollection<SchoolClass> _classes = [];
    [ObservableProperty] private Room? _selectedRoom;
    [ObservableProperty] private SchoolClass? _selectedClass;
    [ObservableProperty] private DateTime _selectedDate = DateTime.Today;
    [ObservableProperty] private TimeSpan _startTime = new(8, 0, 0);
    [ObservableProperty] private TimeSpan _endTime = new(10, 0, 0);
    [ObservableProperty] private string _reason = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _successMessage = string.Empty;
    [ObservableProperty] private bool _isAvailableVisible;
    [ObservableProperty] private bool _isUnavailableVisible;

    private CancellationTokenSource? _availabilityCts;

    public RoomViewModel(RoomService roomService, AbsenceService absenceService)
    {
        _roomService = roomService;
        _absenceService = absenceService;
    }

    partial void OnSelectedRoomChanged(Room? value) => _ = TriggerAvailabilityCheckAsync();
    partial void OnSelectedDateChanged(DateTime value) => _ = TriggerAvailabilityCheckAsync();
    partial void OnStartTimeChanged(TimeSpan value) => _ = TriggerAvailabilityCheckAsync();
    partial void OnEndTimeChanged(TimeSpan value) => _ = TriggerAvailabilityCheckAsync();

    private async Task TriggerAvailabilityCheckAsync()
    {
        _availabilityCts?.Cancel();
        _availabilityCts = new CancellationTokenSource();
        var token = _availabilityCts.Token;

        IsAvailableVisible = false;
        IsUnavailableVisible = false;

        if (SelectedRoom == null) return;

        var start = SelectedDate.Date + StartTime;
        var end = SelectedDate.Date + EndTime;
        if (end <= start) return;

        try
        {
            await Task.Delay(500, token);
            if (token.IsCancellationRequested) return;

            var available = await _roomService.CheckAvailabilityAsync(SelectedRoom.Id, start, end);
            if (token.IsCancellationRequested) return;

            IsAvailableVisible = available;
            IsUnavailableVisible = !available;
        }
        catch (OperationCanceledException) { }
        catch { }
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var roomsTask = _roomService.GetRoomsAsync();
            var classesTask = _absenceService.GetTeacherClassesAsync();
            await Task.WhenAll(roomsTask, classesTask);

            Rooms = new ObservableCollection<Room>(roomsTask.Result);
            Classes = new ObservableCollection<SchoolClass>(classesTask.Result);
            await LoadReservationsAsync();
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
    public async Task LoadReservationsAsync()
    {
        try
        {
            var list = await _roomService.GetMyReservationsAsync();
            MyReservations = new ObservableCollection<RoomReservation>(list);
        }
        catch { }
    }

    [RelayCommand]
    private async Task ReserveAsync()
    {
        if (SelectedRoom == null)
        {
            ErrorMessage = "Veuillez sélectionner une salle.";
            return;
        }

        var start = SelectedDate.Date + StartTime;
        var end = SelectedDate.Date + EndTime;

        if (end <= start)
        {
            ErrorMessage = "L'heure de fin doit être après l'heure de début.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            await _roomService.CreateReservationAsync(SelectedRoom.Id, start, end, Reason, SelectedClass?.Id);
            await LoadReservationsAsync();
            Reason = string.Empty;
            SelectedClass = null;
            ShowSuccess("Réservation confirmée !");
        }
        catch (Exception ex)
        {
            try
            {
                var json = JsonDocument.Parse(ex.Message);
                ErrorMessage = json.RootElement.GetProperty("message").GetString() ?? ex.Message;
            }
            catch
            {
                ErrorMessage = "Erreur lors de la réservation.";
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteReservationAsync(RoomReservation reservation)
    {
        bool confirm = await Shell.Current.DisplayAlert("Annuler", $"Annuler la réservation de {reservation.RoomName} ?", "Oui", "Non");
        if (!confirm) return;

        try
        {
            await _roomService.DeleteReservationAsync(reservation.Id);
            await LoadReservationsAsync();
        }
        catch (Exception)
        {
            ErrorMessage = "Erreur lors de la suppression.";
        }
    }

    private void ShowSuccess(string msg)
    {
        SuccessMessage = msg;
        Task.Delay(2000).ContinueWith(_ => MainThread.BeginInvokeOnMainThread(() => SuccessMessage = string.Empty));
    }
}
