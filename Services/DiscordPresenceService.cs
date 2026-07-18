namespace Discord_Lyrics_Status.Services;

using DiscordRPC;
using System.Diagnostics;

public sealed class DiscordPresenceService : IDisposable
{
    private static readonly TimeSpan MinimumUpdateInterval =
        TimeSpan.FromSeconds(10);

    private readonly DiscordRpcClient _client;
    private readonly SemaphoreSlim _updateLock = new(1, 1);
    private readonly Stopwatch _updateTimer = Stopwatch.StartNew();

    private bool _hasUpdated;
    private bool _disposed;

    public DiscordPresenceService(string applicationId)
    {
        _client = new DiscordRpcClient(applicationId);
        _client.Initialize();
    }
    
    public async Task UpdateAsync(
        string songTitle,
        string artist,
        string lyric,
        CancellationToken cancellationToken = default)
    {
        await _updateLock.WaitAsync(cancellationToken);

        try
        {
            ThrowIfDisposed();
            
            if (_hasUpdated &&
                _updateTimer.Elapsed < MinimumUpdateInterval)
            {
                return;
            }

            _client.SetPresence(new RichPresence
            {
                Name = Truncate(lyric, 128),
                Type = ActivityType.Listening,
                Details = Truncate(lyric, 128),
                State = Truncate($"{songTitle} — {artist}", 128),
                Assets = new Assets
                {
                    LargeImageKey = "spotify",
                    LargeImageText = Truncate(
                        $"{songTitle} by {artist}",
                        128)
                }
            });

            _hasUpdated = true;
            _updateTimer.Restart();
        }
        finally
        {
            _updateLock.Release();
        }
    }

    public void Clear()
    {
        ThrowIfDisposed();
        _client.ClearPresence();
    }

    private static string Truncate(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "No lyric available";

        return value.Length <= maximumLength
            ? value
            : value[..maximumLength];
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _client.ClearPresence();
        _client.Dispose();
        _updateLock.Dispose();
    }
}