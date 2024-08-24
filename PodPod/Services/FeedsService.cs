using System;
using System.Diagnostics;
using OPMLCore.NET;
using Podly.FeedParser;
using PodPod.Models;
namespace PodPod.Services;

public static class FeedsService
{
	public static async Task<Opml> DownloadAndProcessOPMLFile()
	{
		Debug.WriteLine("Getting OPML File");
        string destPath = Path.Combine(AppPaths.DataDirectory, "my-podcast-list.opml");

		if (File.Exists(destPath))
			File.Delete(destPath);

		var file = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Select OPML file"
        });

        if (file != null)
        {	
			try {
				var stream = await file.OpenReadAsync();
				using (var destStream = File.Create(destPath))
				{
					await stream.CopyToAsync(destStream);
				}
			} catch (Exception ex){
				Debug.WriteLine($"Error saving OPML file: {ex.Message}");
			}
		}
		return await ProcessOPMLFile(destPath);
	}

    public async static Task<Opml> ProcessOPMLFile(string destpath = "podcastlist.opml")
    {
        Console.WriteLine("Processing OPML File");

		if (destpath == "podcastlist.opml"){
			Debug.WriteLine("Using default OPML file");
			destpath = Path.Combine(FileSystem.AppDataDirectory, destpath);
		}
        return new Opml(destpath);
    }

    public async static Task<List<Podcast>> CreatePodcastList(Opml opml)
    {
        Console.WriteLine("Creating Podcast List");
        List<Podcast> podcasts = new List<Podcast>();

		try
		{
            foreach (Outline outline in opml.Body.Outlines)
            {
				if (outline.Text.ToLower() == "feeds"){
					foreach (Outline childOutline in outline.Outlines)
					{
						var podcast = new Podcast(childOutline.Text, childOutline.XMLUrl);
						podcasts.Add(podcast);
					}
				} else {
					var podcast = new Podcast(outline.Text, outline.XMLUrl);
					podcasts.Add(podcast);
				}
            }
        }
		catch (Exception e)
		{
			Debug.WriteLine("Error processing OPML file: " +e.Message);
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

    public static async 
    Task
FetchFeed(Podcast pod){
		Rss20Feed feed = new Rss20Feed();

		await Task.Run(() =>
		{
			Debug.WriteLine("Fetching feed");
			var factory = new HttpFeedFactory();
			feed = factory.CreateFeed(new Uri(pod.FeedUrl)) as Rss20Feed;
			Debug.WriteLine("Feed fetched");
		});

		if (pod.LastChecked != null){
			if (pod.LastChecked > feed.LastUpdated)
			{
				pod.LastChecked = DateTime.Now;
                return;
            }
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
				var result = await DownloadService.DownloadImageAsync(pod.Cover, AppPaths.SeriesDirectory(pod.FolderName));
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

