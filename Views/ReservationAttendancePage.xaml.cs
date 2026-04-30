using AppMobile.ViewModels;

namespace AppMobile.Views;

public partial class ReservationAttendancePage : ContentPage
{
    public ReservationAttendancePage(ReservationAttendanceViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
