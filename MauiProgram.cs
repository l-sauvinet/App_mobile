using Microsoft.Extensions.Logging;
using AppMobile.Services;
using AppMobile.ViewModels;
using AppMobile.Views;
using System.Globalization;

namespace AppMobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var fr = new CultureInfo("fr-FR");
        CultureInfo.DefaultThreadCurrentCulture = fr;
        CultureInfo.DefaultThreadCurrentUICulture = fr;

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<CourseService>();
        builder.Services.AddSingleton<AbsenceService>();
        builder.Services.AddSingleton<RoomService>();
        builder.Services.AddSingleton<ReservationAbsenceService>();

        // ViewModels
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<CoursesViewModel>();
        builder.Services.AddTransient<AttendanceViewModel>();
        builder.Services.AddSingleton<RoomViewModel>();
        builder.Services.AddTransient<ReservationAttendanceViewModel>();

        // Views
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<CoursesPage>();
        builder.Services.AddTransient<AttendancePage>();
        builder.Services.AddSingleton<RoomPage>();
        builder.Services.AddTransient<ReservationAttendancePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
