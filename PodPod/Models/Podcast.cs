using System.ComponentModel;
using System.Runtime.CompilerServices;
using Podly.FeedParser;

namespace PodPod.Models;

public class Podcast : INotifyPropertyChanged
{
	private string? title;
	public string? Title { 
		get => title;
		set {
			title = value;
			OnPropertyChanged();
		}
	}
	public string FeedUrl { get; set; }
	public DateTime? LastPublished { get; set; }
	private string? description;
	public string? Description { 
		get => description;
		set {
			description = value;
			OnPropertyChanged();
		}
	}
	private string? cover;
	public string? Cover { 
		get => cover ?? "cover.png";
		set {
			cover = value; 
			OnPropertyChanged();
		}
	}
	private List<Episode> episodes = new List<Episode>();
	public List<Episode> Episodes { 
		get => episodes;
		set {
			episodes = value;
			OnPropertyChanged("EpisodeCount");
			OnPropertyChanged();
		}
	} 
	public int EpisodeCount { get => Episodes.Count; }

	public Podcast(string title, string feedUrl)
	{
		Title = title;
		FeedUrl = feedUrl;
	}

	public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
