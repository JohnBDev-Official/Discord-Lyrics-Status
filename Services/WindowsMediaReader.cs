using Windows.Media.Control;

namespace Discord_Lyrics_Status.Services;

public sealed record MediaInfo(
    string Title,
    string Artist,
    TimeSpan Position,
    TimeSpan Duration,
    bool IsPlaying);

public static class WindowsMediaReader
{
    public static async Task<MediaInfo?> GetCurrentMediaAsync()
    {
        GlobalSystemMediaTransportControlsSessionManager manager =
            await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        
        GlobalSystemMediaTransportControlsSession? session =
            manager.GetCurrentSession();

        if (session is null)
            return null;

        GlobalSystemMediaTransportControlsSessionMediaProperties properties =
            await session.TryGetMediaPropertiesAsync();

        GlobalSystemMediaTransportControlsSessionTimelineProperties timeline =
            session.GetTimelineProperties();

        GlobalSystemMediaTransportControlsSessionPlaybackInfo playback =
            session.GetPlaybackInfo();

        TimeSpan position = timeline.Position;
        
        if (playback.PlaybackStatus ==
            GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            TimeSpan elapsed = now - timeline.LastUpdatedTime;

            if (elapsed > TimeSpan.Zero)
                position += elapsed;
        }

        return new MediaInfo(
            properties.Title,
            properties.Artist,
            position,
            timeline.EndTime,
            playback.PlaybackStatus ==
            GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing);
    }
}