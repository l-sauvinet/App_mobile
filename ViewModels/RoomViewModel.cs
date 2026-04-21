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

    [ObservableProperty] private ObservableCollection<Room> _rooms = [];
    [ObservableProperty] private ObservableCollection<RoomReservation> _myReservations = [];
    [ObservableProperty] private Room? _selectedRoom;
    [ObservableProperty] private DateTime _selectedDate = DateTime.Today;
    [ObservableProperty] private TimeSpan _startTime = new(8, 0, 0);
    [ObservableProperty] private TimeSpan _endTime = new(10, 0, 0);
    [ObservableProperty] private string _reason = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _successMessage = string.Empty;

    public RoomViewModel(RoomService roomService)
    {
        _roomService = roomService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var rooms = await _roomService.GetRoomsAsync();
            Rooms = new ObservableCollection<Room>(rooms);
            await LoadReservationsAsync();
        }
        catch (Exception)
        {
            ErrorMessage = "Erreur lors du chargement.";
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
            await _roomService.CreateReservationAsync(SelectedRoom.Id, start, end, Reason);
            await LoadReservationsAsync();
            Reason = string.Empty;
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
