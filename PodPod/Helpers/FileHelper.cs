using System.Diagnostics;
using System.Text.RegularExpressions;
using FFMpegCore;

namespace PodPod.Helpers;

public class FileHelper
{
	public FileHelper() {}

    public static string SanitizeFilename(string filename)
    {
        if (string.IsNullOrEmpty(filename))
            return string.Empty;

        const int MAX_FILENAME = 200;
        const string REPLACEMENT = "";
        string colonReplacement = " - ";

        var sanitized = filename;
        sanitized = sanitized.Replace(":", colonReplacement);
        sanitized = sanitized.Replace("&", "and");
        sanitized = Regex.Replace(sanitized, @"[\/\?<>\\:\*\|""]", REPLACEMENT);
        sanitized = Regex.Replace(sanitized, @"[\x00-\x1f\x80-\x9f]", REPLACEMENT);
        sanitized = Regex.Replace(sanitized, @"^\.+$", REPLACEMENT);
        sanitized = Regex.Replace(sanitized, @"^(con|prn|aux|nul|com[0-9]|lpt[0-9])(\..*)?$", REPLACEMENT, RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @"[\. ]+$", REPLACEMENT);
        sanitized = Regex.Replace(sanitized, @"[\n\r]", REPLACEMENT);
        sanitized = Regex.Replace(sanitized, @"\s+", " ");

        // We may need to tune this for max 255 chars, or something similar, if we go beyond mac.
        // For now, it stops filenames from getting stupidly too long.
        if (sanitized.Length > MAX_FILENAME)
        {
            var extension = System.IO.Path.GetExtension(sanitized);
            var name = System.IO.Path.GetFileNameWithoutExtension(sanitized);
            var truncatedName = name.Substring(0, MAX_FILENAME - extension.Length);
            sanitized = truncatedName + extension;
        }
        return sanitized;
    }

    public static string RemoveQueryParams(string url)
    {
        if (url.Contains("?")) url = url.Substring(0, url.IndexOf("?"));
        return url;
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
