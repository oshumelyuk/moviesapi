using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;

namespace Movies.Api.Sdk.Consumer;

public class AuthTokenProvider
{
    private readonly HttpClient _httpClient;
    private string _cachedToken = string.Empty;
    private static readonly SemaphoreSlim Lock = new (1, 1);
    
    public AuthTokenProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetTokenAsync()
    {
        if (!string.IsNullOrEmpty(_cachedToken))
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(_cachedToken);
            var expiryTimeText = jwt.Claims.Single(x => x.Type == "exp").Value;
            var expiryTime = ConvertUnixTimeStringToDateTime(expiryTimeText);
            if (expiryTime > DateTime.UtcNow)
            {
                return _cachedToken;
            }
        }
        
        await Lock.WaitAsync();
        var response = await _httpClient.PostAsJsonAsync("http://localhost:5002/token", new
            {
                email = "o.bor@gmail.com",
                role = "admin",
                userid = "4469d0f0-cfd7-4634-a457-1dee96e2e20b",
                customClaims = new Dictionary<string, object>{
                    {"admin", true}, 
                    {"trusted", false }
                }
        });
        var newToken = await response.Content.ReadAsStringAsync();
        _cachedToken = newToken;
        Lock.Release();
        return _cachedToken;
    }
    
    public DateTime ConvertUnixTimeStringToDateTime(string unixTimeString)
    {
        // Parse the string to a long
        if (long.TryParse(unixTimeString, out long unixTime))
        {
            // Convert Unix time to DateTime
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime).ToLocalTime();
        }
        else
        {
            throw new ArgumentException("Invalid Unix time string format.");
        }
    }
}