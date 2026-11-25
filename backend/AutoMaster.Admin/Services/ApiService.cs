using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AutoMaster.Admin.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public ApiService(HttpClient http, AuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    private void Authorize()
    {
        _http.DefaultRequestHeaders.Authorization = _auth.AccessToken is null
            ? null
            : new AuthenticationHeaderValue("Bearer", _auth.AccessToken);
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        Authorize();
        return await _http.GetFromJsonAsync<T>(url);
    }

    public async Task<T?> PostAsync<T>(string url, object data)
    {
        Authorize();
        var response = await _http.PostAsJsonAsync(url, data);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<T?> PutAsync<T>(string url, object data)
    {
        Authorize();
        var response = await _http.PutAsJsonAsync(url, data);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<T?> PatchAsync<T>(string url)
    {
        Authorize();
        var response = await _http.PatchAsync(url, new StringContent(""));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task PostAsync(string url)
    {
        Authorize();
        var response = await _http.PostAsync(url, new StringContent(""));
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string url)
    {
        Authorize();
        var response = await _http.DeleteAsync(url);
        response.EnsureSuccessStatusCode();
    }
}
