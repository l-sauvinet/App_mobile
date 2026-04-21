using System.Net.Http.Json;

namespace AppMobile.Services;

public class ApiService
{
    public readonly HttpClient Http;

    public const string BaseUrl = "http://localhost:3001";

    public ApiService()
    {
        Http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public void SetTeacherId(int teacherId)
    {
        Http.DefaultRequestHeaders.Remove("X-Teacher-Id");
        Http.DefaultRequestHeaders.Add("X-Teacher-Id", teacherId.ToString());
    }

    public async Task<T?> GetAsync<T>(string path)
    {
        var resp = await Http.GetAsync(path);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<T>();
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string path, T body)
        => await Http.PostAsJsonAsync(path, body);

    public async Task<HttpResponseMessage> DeleteAsync(string path)
        => await Http.DeleteAsync(path);
}
