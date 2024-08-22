using System;
using System.Diagnostics;
using FFMpegCore;
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

    public static async Task<string> DownloadImageAsync(string imageUrl, string filePath)
    {
        using (var httpClient = new HttpClient())
        { 
            Debug.WriteLine("Downloading image: " + imageUrl);
            try
            {
                byte[] imageData = await httpClient.GetByteArrayAsync(imageUrl);

                // Get file extension and file name
                string fileExtension = Path.GetExtension(imageUrl);
                string fileName = "Cover" + fileExtension;

                // Save the cover in the temporary folder
                string tempFilePath = Path.Combine(AppPaths.TempDirectory, "cover" + fileExtension);
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
                await File.WriteAllBytesAsync(tempFilePath, imageData);

                // Resize the image to 1000xN
                string newFilePath = Path.Combine(filePath, fileName);
                await FFMpegArguments
                    .FromFileInput(tempFilePath)
                    .OutputToFile(newFilePath, true, options => options
                         .WithVideoFilters(filterOptions => {
                             filterOptions.Scale(1500, -1);
                         }
                    ))
                    .ProcessAsynchronously(true, new FFOptions { BinaryFolder = "/opt/homebrew/bin" });

                // Delete the temporary file if needed
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);

                return newFilePath;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error downloading or processing image: " + imageUrl);
                Debug.WriteLine(e.Message);
                return imageUrl;
            }

        }
    }
}
