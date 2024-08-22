using System;
using System.Diagnostics;
using OPMLCore.NET;
using Podly.FeedParser;
using PodPod.Helpers;
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

    public static async void FetchFeed(Podcast pod){
		IFeed feed = null;

		await Task.Run(() =>
		{
			Debug.WriteLine("Fetching feed");
			var factory = new HttpFeedFactory();
			feed = factory.CreateFeed(new Uri(pod.FeedUrl)) as Rss20Feed;
			Debug.WriteLine("Feed fetched");
		});

		if (pod.LastChecked != null || feed.LastUpdated != null)
			if (pod.LastChecked > feed.LastUpdated)
			{
				pod.LastChecked = DateTime.Now;
                return;
            }
				

		await Task.Run(() =>
		{
			Debug.WriteLine("Processing podcast data");

			pod.Description = feed.Description;
			pod.Cover = feed.CoverImageUrl;
			pod.LastPublished = feed.LastUpdated;
			pod.LastChecked = DateTime.Now;

			List<Episode> eps = new List<Episode>();
			Debug.WriteLine("Building Episode list");
			foreach (var item in feed.Items)
			{
				eps.Add(new Episode
				{
					Id = item.Id,
					Title = item.Title,
					MediaURL = item.MediaUrl,
					Description = item.Content,
					Published = item.DatePublished,
					Cover = item.Cover,
					Author = item.Author,
					Link = item.Link,
					EpisodeNumber = item.EpisodeNumber,
					Duration = item.MediaLength
				});
			}
			pod.Episodes = eps;
			Debug.WriteLine("Episode list built");

			_ = Task.Run(async () =>{
				var result = await FileHelper.DownloadImageAsync(pod.Cover, AppPaths.SeriesDirectory(pod.FolderName));
				if (result != null)
				{
					pod.Cover = result;
				}
			});

			var index = Data.Podcasts.FindIndex(p => p.Title.ToLower() == pod.Title.ToLower());
			Data.Podcasts[index] = pod;
			Data.SaveToJsonFile(Data.Podcasts, "podcasts");
		});
	}
}

