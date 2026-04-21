using AppMobile.Views;

namespace AppMobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("attendance", typeof(AttendancePage));
    }
}
