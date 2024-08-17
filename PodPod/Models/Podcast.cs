namespace PodPod.Models;

public class Podcast
{
	public string Title { get; set; }
	public string FeedUrl { get; set; }

	public Podcast(string title, string feedUrl)
	{
		Title = title;
		FeedUrl = feedUrl;
	}
}

