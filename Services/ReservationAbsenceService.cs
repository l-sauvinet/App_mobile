using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AppMobile.Models;

namespace AppMobile.Services;

public class ReservationAbsenceService
{
    private readonly ApiService _api;

    public ReservationAbsenceService(ApiService api)
    {
        _api = api;
    }

    public async Task<List<ReservationStudent>> GetStudentsAsync(int reservationId)
    {
        var rows = await _api.GetAsync<List<StudentDto>>($"/api/teacher/reservations/{reservationId}/students");
        return rows?.Select(r => new ReservationStudent
        {
            Id = r.Id,
            FirstName = r.FirstName,
            LastName = r.LastName,
            AbsenceId = r.AbsenceId,
            IsAbsent = r.AbsenceId.HasValue,
            IsLate = r.IsLate == 1,
            DelayMinutes = r.DelayMinutes ?? 0
        }).ToList() ?? [];
    }

    public async Task<int> RecordAbsenceAsync(int studentId, int reservationId, bool isLate, int delayMinutes)
    {
        var resp = await _api.PostAsync("/api/teacher/reservation-absences",
            new { studentId, reservationId, isLate, delayMinutes });
        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadFromJsonAsync<ErrorDto>();
            throw new Exception(err?.Message ?? "Erreur lors de l'enregistrement.");
        }
        var body = await resp.Content.ReadFromJsonAsync<ResultDto>();
        return body?.AbsenceId ?? 0;
    }

    public async Task RemoveAbsenceAsync(int absenceId)
    {
        var resp = await _api.DeleteAsync($"/api/teacher/reservation-absences/{absenceId}");
        resp.EnsureSuccessStatusCode();
    }

    private record ResultDto([property: JsonPropertyName("absenceId")] int AbsenceId);
    private record ErrorDto([property: JsonPropertyName("message")] string? Message);
    private record StudentDto(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("first_name")] string FirstName,
        [property: JsonPropertyName("last_name")] string LastName,
        [property: JsonPropertyName("absence_id")] int? AbsenceId,
        [property: JsonPropertyName("is_late")] int? IsLate,
        [property: JsonPropertyName("delay_minutes")] int? DelayMinutes);
}
