using AppMobile.Models;
using System.Text.Json.Serialization;

namespace AppMobile.Services;

public class CourseService
{
    private readonly ApiService _api;

    public CourseService(ApiService api)
    {
        _api = api;
    }

    public async Task<List<Course>> GetTeacherCoursesAsync()
    {
        var rows = await _api.GetAsync<List<CourseDto>>("/api/teacher/courses");
        return rows?.Select(r => new Course
        {
            Id = r.Id,
            SubjectName = r.SubjectName,
            ClassName = r.ClassName,
            RoomName = r.RoomName ?? "",
            StartDatetime = r.StartDatetime,
            EndDatetime = r.EndDatetime
        }).ToList() ?? [];
    }

    private record CourseDto(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("subject_name")] string SubjectName,
        [property: JsonPropertyName("class_name")] string ClassName,
        [property: JsonPropertyName("room_name")] string? RoomName,
        [property: JsonPropertyName("start_datetime")] DateTime StartDatetime,
        [property: JsonPropertyName("end_datetime")] DateTime EndDatetime);
}
