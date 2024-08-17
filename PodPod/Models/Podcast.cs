namespace PodPod.Models;

public class Podcast
{
	public string Title { get; set; }
	public string Description { get; set; }
	public string FeedUrl { get; set; }
	private string? cover;
	public string? Cover { 
		get => cover ?? "cover.png";
		set => cover = value; 
	}

	public Podcast(string title, string feedUrl)
	{
		Title = title;
		FeedUrl = feedUrl;
	}
}

