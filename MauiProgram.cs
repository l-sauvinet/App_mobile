using Microsoft.Extensions.Logging;
using AppMobile.Services;
using AppMobile.ViewModels;
using AppMobile.Views;

namespace AppMobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
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

        // ViewModels
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<CoursesViewModel>();
        builder.Services.AddTransient<AttendanceViewModel>();
        builder.Services.AddSingleton<RoomViewModel>();

        // Views
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<CoursesPage>();
        builder.Services.AddTransient<AttendancePage>();
        builder.Services.AddSingleton<RoomPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
