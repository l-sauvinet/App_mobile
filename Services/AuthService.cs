using AppMobile.Models;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AppMobile.Services;

public class AuthService
{
    private readonly ApiService _api;
    public User? CurrentUser { get; private set; }

    public AuthService(ApiService api)
    {
        _api = api;
    }

    public async Task<(bool Success, string Error)> LoginAsync(string login, string password)
    {
        var resp = await _api.PostAsync("/api/teacher/login", new { login, password });

        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadFromJsonAsync<ErrorDto>();
            return (false, err?.Message ?? "Identifiants incorrects ou accès non autorisé.");
        }

        var body = await resp.Content.ReadFromJsonAsync<LoginResponseDto>();
        if (body?.Teacher == null)
            return (false, "Réponse invalide du serveur.");

        var t = body.Teacher;
        CurrentUser = new User
        {
            Id = t.UserId,
            Login = t.Login,
            FirstName = t.FirstName,
            LastName = t.LastName,
            Email = t.Email,
            RoleId = t.RoleId,
            TeacherId = t.TeacherId
        };

        _api.SetTeacherId(t.TeacherId);
        return (true, string.Empty);
    }

    public async Task LogoutAsync()
    {
        CurrentUser = null;
        await _api.PostAsync("/api/teacher/logout", new { });
    }

    private record ErrorDto([property: JsonPropertyName("message")] string Message);
    private record TeacherDto(
        [property: JsonPropertyName("userId")] int UserId,
        [property: JsonPropertyName("teacherId")] int TeacherId,
        [property: JsonPropertyName("login")] string Login,
        [property: JsonPropertyName("firstName")] string FirstName,
        [property: JsonPropertyName("lastName")] string LastName,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("roleId")] int RoleId);
    private record LoginResponseDto([property: JsonPropertyName("teacher")] TeacherDto? Teacher);
}
