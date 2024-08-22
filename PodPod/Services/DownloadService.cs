using System;
using System.Diagnostics;
using PodPod.Models;

namespace PodPod.Services;

public static class DownloadService
{
    public static Task<bool> DownloadPodcastEpisode(Episode episode, string folderName)
    {
        string filePath = AppPaths.EpisodeFilePath(folderName, episode.FileName);

        if (File.Exists(filePath)) return Task.FromResult(false);
        try
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(new Uri(episode.MediaURL)).Result;
                if (response.IsSuccessStatusCode)
                {
                    using (var stream = response.Content.ReadAsStreamAsync().Result)
                    {
                        using (var fileStream = File.Create(filePath))
                        {
                            stream.CopyTo(fileStream);
                        }
                    }
                    episode.MediaURL = filePath;
                    return Task.FromResult(true);
                }
            }
        } catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
        return Task.FromResult(false);
    }
}
