using System.Globalization;
using System.Text.RegularExpressions;

namespace Discord_Lyrics_Status.Parsers;

public sealed record LyricLine(TimeSpan Timestamp, string Text);

public static partial class LrcParser
{
    [GeneratedRegex(
        @"^\[(?<minutes>\d+):(?<seconds>\d+(?:\.\d+)?)\]\s*(?<text>.*)$")]
    private static partial Regex TimestampRegex();

    public static IReadOnlyList<LyricLine> Parse(string? lrc)
    {
        if (string.IsNullOrWhiteSpace(lrc))
            return [];

        var lines = new List<LyricLine>();

        foreach (string rawLine in lrc.Split('\n'))
        {
            Match match = TimestampRegex().Match(rawLine.Trim());

            if (!match.Success)
                continue;

            int minutes = int.Parse(
                match.Groups["minutes"].Value,
                CultureInfo.InvariantCulture);

            double seconds = double.Parse(
                match.Groups["seconds"].Value,
                CultureInfo.InvariantCulture);

            lines.Add(new LyricLine(
                TimeSpan.FromMinutes(minutes) +
                TimeSpan.FromSeconds(seconds),
                match.Groups["text"].Value.Trim()));
        }

        return lines.OrderBy(line => line.Timestamp).ToArray();
    }

    public static LyricLine? GetCurrentLine(
        IReadOnlyList<LyricLine> lyrics,
        TimeSpan position)
    {
        LyricLine? current = null;

        foreach (LyricLine line in lyrics)
        {
            if (line.Timestamp > position)
                break;

            current = line;
        }

        return current;
    }
}