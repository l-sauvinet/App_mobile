using AppMobile.ViewModels;

namespace AppMobile.Views;

public partial class RoomPage : ContentPage
{
    private readonly RoomViewModel _vm;

    public RoomPage(RoomViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
