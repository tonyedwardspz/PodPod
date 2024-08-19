using System;

namespace PodPod.Services;

public static class DownloadService
{
    public static Task<string> DownloadPodcastEpisode(string url, string title)
    {
        string filePath = Path.Combine(FileSystem.AppDataDirectory, title + ".mp3");

        if (File.Exists(filePath)) return Task.FromResult(filePath);

        using (var client = new HttpClient())
        {
            var response = client.GetAsync(new Uri(url)).Result;
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
                return Task.FromResult(url);
            }
        }
    }

}
