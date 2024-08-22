using System;
using OPMLCore.NET;
using PodPod.Models;
namespace PodPod.Services;

public static class FeedsService
{
	
    public async static Task<Opml> ProcessOPMLFile()
    {
        Console.WriteLine("Processing OPML");

        var fileName = "podcastlist.opml";
        string destPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        using (var stream = await FileSystem.OpenAppPackageFileAsync(fileName))
        {
            using (var destStream = File.Create(destPath))
            {
                await stream.CopyToAsync(destStream);
            }
        }

        Opml opml = new Opml(destPath);
        return opml;
    }

    public async static Task<List<Podcast>> CreatePodcastList(Opml opml)
    {
        Console.WriteLine("Creating Podcast List");
        List<Podcast> podcasts = new List<Podcast>();

        foreach (Outline outline in opml.Body.Outlines)
        {
            foreach (Outline childOutline in outline.Outlines)
            {
                var podcast = new Podcast(childOutline.Text, childOutline.XMLUrl);
                podcasts.Add(podcast);
            }
        }
        return podcasts;
    }

    public async static void ShowRSSFeed(string url)
    {
        Console.WriteLine("Showing RSS Feed");
        try
        {
            var uri = new Uri(url);
            MainThread.BeginInvokeOnMainThread(() => Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error opening RSS feed: {ex.Message}");
        }
    }
}

