using AppMobile.ViewModels;

namespace AppMobile.Views;

public partial class AttendancePage : ContentPage
{
    public AttendancePage(AttendanceViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
