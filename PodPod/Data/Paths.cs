using System;
namespace PodPod;


public static class AppPaths
{
    public static string RootDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "PodPod");

    public static string PodcastsDirectory => Path.Combine(RootDirectory, "Podcasts");

    public static string DataDirectory => Path.Combine(RootDirectory, "Data");

    public static string TempDirectory => Path.Combine(RootDirectory, "Temp");

    public static string SeriesDirectory(string seriesName)
    {
        string path = Path.Combine(PodcastsDirectory, seriesName);

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        return path;
    }

    public static string EpisodeFilePath(string seriesName, string episodeFileName)
    {
        return Path.Combine(SeriesDirectory(seriesName), episodeFileName + ".mp3");
    }

    public static string EpisodeTranscriptionFilePath(string seriesName, string episodeFileName)
    {
        string path = Path.Combine(SeriesDirectory(seriesName), "Transcriptions");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        return Path.Combine(path, episodeFileName + ".json");
    }

    public static void InitDirectories()
    {
        var directories = new[]
        {
            RootDirectory,
            PodcastsDirectory,
            DataDirectory,
            TempDirectory
        };

        try {
            foreach (var directory in directories)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }
        } catch (Exception e){
            Console.WriteLine(e.Message);
        }
    }
}
