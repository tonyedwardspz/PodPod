using Podly.FeedParser;

namespace PodPod.Models;

public class Podcast
{
	public string Title { get; set; }
	public string FeedUrl { get; set; }
	public DateTime? LastPublished { get; set; }
	public Rss20Feed? Feed { get; set; }
	public string? Description { get; set; }
	private string? cover;
	public string? Cover { 
		get => cover ?? "cover.png";
		set => cover = value; 
	}
	public List<Episode> Episodes { get; set; } = new List<Episode>();
	public int EpisodeCount = 0;

	public Podcast(string title, string feedUrl)
	{
		Title = title;
		FeedUrl = feedUrl;
	}
}

