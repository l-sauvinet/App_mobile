using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppMobile.Services;

namespace AppMobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _auth;

    [ObservableProperty] private string _login = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isPasswordVisible;
    [ObservableProperty] private string _eyeIcon = "eye.svg";

    [RelayCommand]
    private void TogglePassword()
    {
        IsPasswordVisible = !IsPasswordVisible;
        EyeIcon = IsPasswordVisible ? "eye_off.svg" : "eye.svg";
    }

    public LoginViewModel(AuthService auth)
    {
        _auth = auth;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Veuillez remplir tous les champs.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var (success, error) = await _auth.LoginAsync(Login, Password);
            if (success)
                await Shell.Current.GoToAsync("//courses");
            else
                ErrorMessage = error;
        }
        catch (Exception)
        {
            ErrorMessage = "Impossible de se connecter à la base de données.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
