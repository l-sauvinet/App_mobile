using System.Text.Json.Serialization;
using AppMobile.Models;

namespace AppMobile.Services;

public class RoomService
{
    private readonly ApiService _api;

    public RoomService(ApiService api)
    {
        _api = api;
    }

    public async Task<List<Room>> GetRoomsAsync()
    {
        var rows = await _api.GetAsync<List<RoomDto>>("/api/teacher/rooms");
        return rows?.Select(r => new Room
        {
            Id = r.Id,
            Name = r.Name,
            Capacity = r.Capacity ?? 0,
            Type = r.Type ?? ""
        }).ToList() ?? [];
    }

    public async Task<List<RoomReservation>> GetMyReservationsAsync()
    {
        var rows = await _api.GetAsync<List<ReservationDto>>("/api/teacher/reservations");
        return rows?.Select(r => new RoomReservation
        {
            Id = r.Id,
            RoomId = r.RoomId,
            RoomName = r.RoomName,
            StartDatetime = r.StartDatetime,
            EndDatetime = r.EndDatetime,
            Reason = r.Reason ?? "",
            ClassId = r.ClassId,
            ClassName = r.ClassName ?? ""
        }).ToList() ?? [];
    }

    public async Task CreateReservationAsync(int roomId, DateTime start, DateTime end, string reason, int? classId)
    {
        var resp = await _api.PostAsync("/api/teacher/reservations", new
        {
            roomId,
            startDatetime = start.ToString("yyyy-MM-dd HH:mm:ss"),
            endDatetime = end.ToString("yyyy-MM-dd HH:mm:ss"),
            reason,
            classId
        });

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            throw new Exception(body);
        }
    }

    public async Task DeleteReservationAsync(int reservationId)
    {
        var resp = await _api.DeleteAsync($"/api/teacher/reservations/{reservationId}");
        resp.EnsureSuccessStatusCode();
    }

    private record RoomDto(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("capacity")] int? Capacity,
        [property: JsonPropertyName("type")] string? Type);

    private record ReservationDto(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("room_id")] int RoomId,
        [property: JsonPropertyName("room_name")] string RoomName,
        [property: JsonPropertyName("start_datetime")] DateTime StartDatetime,
        [property: JsonPropertyName("end_datetime")] DateTime EndDatetime,
        [property: JsonPropertyName("reason")] string? Reason,
        [property: JsonPropertyName("class_id")] int? ClassId,
        [property: JsonPropertyName("class_name")] string? ClassName);
}
