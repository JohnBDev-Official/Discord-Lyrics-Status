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
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void DoItButton_Click(object sender, RoutedEventArgs e)
    {
        await StartLoopAsync();
    }

    private async Task StartLoopAsync()
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
    }
}