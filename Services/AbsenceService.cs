using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AppMobile.Models;

namespace AppMobile.Services;

public class AbsenceService
{
    private readonly ApiService _api;

    public AbsenceService(ApiService api)
    {
        _api = api;
    }

    public async Task<List<Student>> GetStudentsForCourseAsync(int courseId)
    {
        var rows = await _api.GetAsync<List<StudentDto>>($"/api/teacher/courses/{courseId}/students");
        return rows?.Select(r => new Student
        {
            Id = r.Id,
            FirstName = r.FirstName,
            LastName = r.LastName,
            AbsenceId = r.AbsenceId,
            IsAbsent = r.AbsenceId.HasValue,
            IsLate = r.IsLate == 1
        }).ToList() ?? [];
    }

    public async Task<int> RecordAbsenceAsync(int studentId, int courseId, bool isLate)
    {
        var resp = await _api.PostAsync("/api/teacher/absences", new { studentId, courseId, isLate });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<AbsenceResultDto>();
        return body?.AbsenceId ?? 0;
    }

    public async Task RemoveAbsenceAsync(int absenceId)
    {
        var resp = await _api.DeleteAsync($"/api/teacher/absences/{absenceId}");
        resp.EnsureSuccessStatusCode();
    }

    private record AbsenceResultDto([property: JsonPropertyName("absenceId")] int AbsenceId);
    private record StudentDto(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("first_name")] string FirstName,
        [property: JsonPropertyName("last_name")] string LastName,
        [property: JsonPropertyName("absence_id")] int? AbsenceId,
        [property: JsonPropertyName("is_late")] int? IsLate);
}
