using System.Net.Http.Json;
using AutoMaster.Admin.Models;
using Microsoft.JSInterop;

namespace AutoMaster.Admin.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    public string? AccessToken { get; private set; }
    public CurrentUser? CurrentUser { get; private set; }

    public event Action? Changed;

    public AuthService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task RestoreAsync()
    {
        AccessToken = await _js.InvokeAsync<string?>("localStorage.getItem", "am_admin_token");
        var userJson = await _js.InvokeAsync<string?>("localStorage.getItem", "am_admin_user");
        if (!string.IsNullOrEmpty(userJson))
        {
            CurrentUser = System.Text.Json.JsonSerializer.Deserialize<CurrentUser>(userJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        Changed?.Invoke();
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("auth/login", new { email, password });
        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (result is null || result.User.Role != "Admin") return false;

        AccessToken = result.AccessToken;
        CurrentUser = result.User;
        await _js.InvokeVoidAsync("localStorage.setItem", "am_admin_token", AccessToken);
        await _js.InvokeVoidAsync("localStorage.setItem", "am_admin_user",
            System.Text.Json.JsonSerializer.Serialize(result.User));
        Changed?.Invoke();
        return true;
    }

    public async Task LogoutAsync()
    {
        AccessToken = null;
        CurrentUser = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", "am_admin_token");
        await _js.InvokeVoidAsync("localStorage.removeItem", "am_admin_user");
        Changed?.Invoke();
    }
}
