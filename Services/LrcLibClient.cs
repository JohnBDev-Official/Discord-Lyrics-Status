using System.Net.Http;
using System.Net.Http.Json;
using Discord_Lyrics_Status.Models;

namespace Discord_Lyrics_Status.Services;

public sealed class LrcLibClient
{
    private readonly HttpClient _httpClient;

    public LrcLibClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress ??= new Uri("https://lrclib.net/");
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "JohnBDev-DiscordLyricsStatus/1.0");
    }
    
    public async Task<LrcLibResult?> GetLyricsAsync(
        string title,
        string artist,
        string? album,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string?>
        {
            ["track_name"] = title,
            ["artist_name"] = artist,
            ["album_name"] = album,
            ["duration"] = ((int)Math.Round(duration.TotalSeconds)).ToString()
        };

        string query = string.Join(
            "&",
            parameters
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(x =>
                    $"{Uri.EscapeDataString(x.Key)}=" +
                    $"{Uri.EscapeDataString(x.Value!)}"));

        return await _httpClient.GetFromJsonAsync<LrcLibResult>(
            $"api/get?{query}",
            cancellationToken);
    }
}