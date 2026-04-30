using System.Net.Http.Json;

namespace AppMobile.Services;

public class ApiService
{
    public readonly HttpClient Http;

#if ANDROID
    public const string BaseUrl = "http://10.0.2.2:3001";
#else
    public const string BaseUrl = "http://localhost:3001";
#endif

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
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{(int)resp.StatusCode}: {body}", null, resp.StatusCode);
        }
        return await resp.Content.ReadFromJsonAsync<T>();
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string path, T body)
        => await Http.PostAsJsonAsync(path, body);

    public async Task<HttpResponseMessage> DeleteAsync(string path)
        => await Http.DeleteAsync(path);
}
