using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PodPod.Helpers;

public class FileHelper
{
	public FileHelper() {}

    public static string SanitizeFilename(string filename, string colonReplacement = " - ")
    {
        if (string.IsNullOrEmpty(filename))
            return string.Empty;

        const int MAX_FILENAME = 200;
        const string REPLACEMENT = "";

        var sanitized = filename;
        sanitized = sanitized.Replace(":", colonReplacement);
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

    public static async Task<string> DownloadImageAsync(string imageUrl, string filePath)
    {
        using (var httpClient = new HttpClient())
        { 
            try{
                if (imageUrl.Contains("?"))
                {
                    imageUrl = imageUrl.Substring(0, imageUrl.IndexOf("?"));
                }

                byte[] imageData = await httpClient.GetByteArrayAsync(imageUrl);

                string fileExtension = Path.GetExtension(imageUrl);
                string fileName = "Cover" + fileExtension;
                string newFilePath = Path.Combine(filePath, fileName);
                await File.WriteAllBytesAsync(newFilePath, imageData);
                return newFilePath;
            } catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }            
        }
    }
}
