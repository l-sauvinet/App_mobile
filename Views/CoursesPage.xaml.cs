using AppMobile.ViewModels;

namespace AppMobile.Views;

public partial class CoursesPage : ContentPage
{
    private readonly CoursesViewModel _vm;

    public CoursesPage(CoursesViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCoursesCommand.ExecuteAsync(null);
    }
}
