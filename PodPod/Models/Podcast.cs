using System.ComponentModel;
using System.Runtime.CompilerServices;
using Podly.FeedParser;

namespace PodPod.Models;

public class Podcast : INotifyPropertyChanged
{
	public string Title { get; set; }
	public string FeedUrl { get; set; }
	public DateTime? LastPublished { get; set; }
	public string? Description { get; set; }
	private string? cover;
	public string? Cover { 
		get => cover ?? "cover.png";
		set {
			cover = value; 
			OnPropertyChanged();
		}
	}
	public List<Episode> Episodes { get; set; } = new List<Episode>();
	public int EpisodeCount = 0;

	public Podcast(string title, string feedUrl)
	{
		Title = title;
		FeedUrl = feedUrl;
	}

	public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

	// public override bool Equals(object? obj)
    // {
    //     if (obj is null || obj is not Podcast)
    //         return false;

    //     var otherPodcast = (Podcast)obj;

    //     if (Title != otherPodcast.Title || Cover != otherPodcast.Cover || LastPublished != otherPodcast.LastPublished || Description != otherPodcast.Description || FeedUrl != otherPodcast.FeedUrl)
    //         return false;

    //     return true;
    // }

    // public static bool operator ==(Podcast x, Podcast y)
    // {
    //     return x.Equals(y);
    // }

    // public static bool operator !=(Podcast x, Podcast y)
    // {
    //     return !x.Equals(y);
    // }
}

