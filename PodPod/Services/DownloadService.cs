using System;

namespace PodPod.Services;

public static class DownloadService
{
	public static string DownloadPodcastEpisode(string url, string title)
    {
        using (var client = new HttpClient())
        {
            var response = client.GetAsync(new Uri(url)).Result;
            if (response.IsSuccessStatusCode)
            {
                string filePath = Path.Combine(FileSystem.AppDataDirectory, title + ".mp3");
                using (var stream = response.Content.ReadAsStreamAsync().Result)
                {
                    using (var fileStream = File.Create(filePath))
                    {
                        stream.CopyTo(fileStream);
                        
                    }
                }
                return filePath;
            } else {
                return null;
            }
        }
    }
}
