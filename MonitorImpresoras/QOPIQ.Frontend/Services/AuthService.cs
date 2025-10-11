using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Options;

namespace QOPIQ.Frontend.Services;

public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _factory;
    private readonly ProtectedSessionStorage _storage;
    private readonly FrontendOptions _opt;

    private const string TokenKey = "authToken";

    public AuthService(IHttpClientFactory factory,
        ProtectedSessionStorage storage,
        IOptions<FrontendOptions> opt)
    {
        _factory = factory;
        _storage = storage;
        _opt = opt.Value;
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        var http = _factory.CreateClient("QOPIQ.API");
        var res = await http.PostAsJsonAsync("auth/login", new { username, password });
        if (!res.IsSuccessStatusCode) return null;

        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("token").GetString();

        if (!string.IsNullOrWhiteSpace(token))
        {
            await _storage.SetAsync(TokenKey, token);
        }
        return token;
    }

    public async Task<bool> RegisterAsync(string username, string email, string password)
    {
        var http = _factory.CreateClient("QOPIQ.API");
        var res = await http.PostAsJsonAsync("auth/register", new { username, email, password });
        return res.IsSuccessStatusCode;
    }

    public async Task LogoutAsync()
    {
        await _storage.DeleteAsync(TokenKey);
    }

    public static async Task<string?> GetTokenAsync(ProtectedSessionStorage storage)
    {
        var r = await storage.GetAsync<string>(TokenKey);
        return r.Success ? r.Value : null;
    }
}
