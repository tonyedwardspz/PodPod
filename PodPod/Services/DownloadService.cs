using System;
using System.Diagnostics;
using FFMpegCore;
using PodPod.Models;

namespace PodPod.Services;

public static class DownloadService
{
    public static async  Task<bool> DownloadPodcastEpisode(Episode episode, string folderName)
    {
        string filePath = AppPaths.EpisodeFilePath(folderName, episode.FileName);

        if (File.Exists(filePath)) return false;
        try
        {
            episode.DownloadButtonText = "Downloading";

            await Task.Run(() =>
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
                        episode.DownloadButtonText = "Downloaded";
                    }
                }
            });
            return true;
        } catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
        return false;
    }

    public static async Task<string> DownloadImageAsync(string imageUrl, string filePath)
    {
        using (var httpClient = new HttpClient())
        { 
            Debug.WriteLine("Downloading image: " + imageUrl);
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, imageUrl);
                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                byte[] imageData = await response.Content.ReadAsByteArrayAsync();

                // Not all image urls have an image extension, so we may need to determine the file extension from the content type
                string contentType = response.Content.Headers.ContentType.MediaType;
                string fileExtension = Path.GetExtension(imageUrl);
                if (string.IsNullOrEmpty(fileExtension))
                {
                    fileExtension = contentType switch
                    {
                        "image/jpeg" => ".jpg",
                        "image/png" => ".png",
                        "image/gif" => ".gif",
                        _ => ".jpg"
                    };
                }
                string fileName = "cover" + fileExtension;

                // Save the cover in the temporary folder, using a unique name
                string tempFilePath = Path.Combine(AppPaths.TempDirectory, Guid.NewGuid().ToString() + "-cover" + fileExtension);
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
                await File.WriteAllBytesAsync(tempFilePath, imageData);

                // Resize the image to 1500xN
                string newFilePath = Path.Combine(filePath, fileName);
                await FFMpegArguments
                    .FromFileInput(tempFilePath)
                    .OutputToFile(newFilePath, true, options => options
                         .WithVideoFilters(filterOptions => {
                             filterOptions.Scale(1500, -1);
                         }
                    ))
                    .ProcessAsynchronously(true, new FFOptions { BinaryFolder = "/opt/homebrew/bin" });

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
