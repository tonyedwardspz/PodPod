using System;
using System.Diagnostics;
using PodPod.Models;

namespace PodPod.Services;

public static class DownloadService
{
    public static Task<string> DownloadPodcastEpisode(Episode episode, Podcast podcast)
    {
        string filePath = AppPaths.EpisodeFilePath(podcast.FolderName, episode.FileName);

        if (File.Exists(filePath)) return Task.FromResult(filePath);
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
                    return Task.FromResult(filePath);
                }
                else
                {
                    // TODO: Handle error much better than this
                    return Task.FromResult(episode.MediaURL);
                }
            }
        } catch (Exception e)
        {
            // TODO: Handle error much better than this
            Debug.WriteLine(e.Message);
            return Task.FromResult(episode.MediaURL);
        }
    }

}
