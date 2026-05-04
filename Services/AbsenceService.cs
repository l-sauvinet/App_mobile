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

    public async Task<List<SchoolClass>> GetTeacherClassesAsync()
    {
        var rows = await _api.GetAsync<List<ClassDto>>("/api/teacher/classes");
        return rows?.Select(r => new SchoolClass { Id = r.Id, Name = r.Name }).ToList() ?? [];
    }

    public async Task<List<SchoolClass>> GetAllClassesAsync()
    {
        var rows = await _api.GetAsync<List<ClassDto>>("/api/teacher/all-classes");
        return rows?.Select(r => new SchoolClass { Id = r.Id, Name = r.Name }).ToList() ?? [];
    }

    public async Task<List<Student>> GetStudentsForCourseAsync(int courseId, int? classId = null)
    {
        var path = classId.HasValue
            ? $"/api/teacher/courses/{courseId}/students?classId={classId}"
            : $"/api/teacher/courses/{courseId}/students";
        var rows = await _api.GetAsync<List<StudentDto>>(path);
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
        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadFromJsonAsync<ErrorDto>();
            throw new Exception(err?.Message ?? "Erreur lors de l'enregistrement.");
        }
        var body = await resp.Content.ReadFromJsonAsync<AbsenceResultDto>();
        return body?.AbsenceId ?? 0;
    }

    public async Task RemoveAbsenceAsync(int absenceId)
    {
        var resp = await _api.DeleteAsync($"/api/teacher/absences/{absenceId}");
        resp.EnsureSuccessStatusCode();
    }

    private record AbsenceResultDto([property: JsonPropertyName("absenceId")] int AbsenceId);
    private record ErrorDto([property: JsonPropertyName("message")] string? Message);
    private record ClassDto(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name);
    private record StudentDto(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("first_name")] string FirstName,
        [property: JsonPropertyName("last_name")] string LastName,
        [property: JsonPropertyName("absence_id")] int? AbsenceId,
        [property: JsonPropertyName("is_late")] int? IsLate);
}
