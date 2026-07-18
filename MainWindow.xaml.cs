using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Discord_Lyrics_Status.Services;
using Windows.Media.Control;
using System.Net.Http;
using Discord_Lyrics_Status.Models;
using Discord_Lyrics_Status.Parsers;

namespace Discord_Lyrics_Status;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly DiscordPresenceService _discordPresence =
        new("1527876392929333278");
    
    public MainWindow()
    {
        InitializeComponent();
    }

    private CancellationTokenSource? _loopCancellation;

    private async Task StartLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            MediaInfo? media = await WindowsMediaReader.GetCurrentMediaAsync();

            if (media is not null)
            {
                using var httpClient = new HttpClient();
                var lyricsClient = new LrcLibClient(httpClient);

                LrcLibResult? result = await lyricsClient.GetLyricsAsync(
                    media.Title,
                    media.Artist,
                    album: null,
                    media.Duration);

                IReadOnlyList<LyricLine> lines =
                    LrcParser.Parse(result?.SyncedLyrics);

                LyricLine? currentLine =
                    LrcParser.GetCurrentLine(lines, media.Position);

                Console.WriteLine(currentLine?.Text ?? "No lyric available");
                CurrentLyricText.Text = currentLine?.Text;
                
                if (currentLine is not null &&
                    !string.IsNullOrWhiteSpace(currentLine.Text))
                {
                    await _discordPresence.UpdateAsync(
                        media.Title,
                        media.Artist,
                        currentLine.Text);
                }
            }

            await Task.Delay(1000, cancellationToken);
        }
    }

    private async void DoItButton_Click(object sender, RoutedEventArgs e)
    {
        DoItButton.Visibility = Visibility.Hidden;
        StopDoingItButton.Visibility = Visibility.Visible;
        
        _loopCancellation = new CancellationTokenSource();
        
        try
        {
            await StartLoopAsync(_loopCancellation.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected when stopped or restarted.
        }
    }

    private async void StopDoingItButton_Click(object sender, RoutedEventArgs e)
    {
        DoItButton.Visibility = Visibility.Visible;
        StopDoingItButton.Visibility = Visibility.Hidden;
        
        _loopCancellation?.Cancel();
    }
    
    protected override void OnClosed(EventArgs e)
    {
        _discordPresence.Dispose();
        base.OnClosed(e);
    }

    /*private async Task StartLoopAsync()
    {
        while (true)
        {
            MediaInfo? media = await WindowsMediaReader.GetCurrentMediaAsync();

            if (media is not null)
            {
                using var httpClient = new HttpClient();
                var lyricsClient = new LrcLibClient(httpClient);

                LrcLibResult? result = await lyricsClient.GetLyricsAsync(
                    media.Title,
                    media.Artist,
                    album: null,
                    media.Duration);

                IReadOnlyList<LyricLine> lines =
                    LrcParser.Parse(result?.SyncedLyrics);

                LyricLine? currentLine =
                    LrcParser.GetCurrentLine(lines, media.Position);

                Console.WriteLine(currentLine?.Text ?? "No lyric available");
            }

            await Task.Delay(1000);
        }
    }*/
}