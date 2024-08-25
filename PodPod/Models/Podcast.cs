using PodPod.Helpers;

namespace PodPod.Models;

public class Podcast : Base
{
	private string? title;
	public string? Title { 
		get => title;
		set {
			title = value;
			OnPropertyChanged();
		}
	}
	private string feedUrl;
	public string FeedUrl { 
		get => feedUrl;
		set {
			feedUrl = FileHelper.RemoveQueryParams(value);
			OnPropertyChanged();
		}
	}
	public DateTime? LastChecked { get; set; }
	private DateTime? lastPublished;
	public DateTime? LastPublished { 
		get => lastPublished;
		set {
			lastPublished = value;
			OnPropertyChanged();
		}
	}
	public bool NeedsChecking()
    {
        if (LastChecked == null || LastPublished == null)
        {
            return true;
        }
        return LastChecked < LastPublished;
    }
	private string? description;
	public string? Description { 
		get => description;
		set {
			description = value;
			OnPropertyChanged();
		}
	}
	private VerticalStackLayout? descriptionStack;
	public VerticalStackLayout? DescriptionStack { 
		get => descriptionStack;
		set {
			descriptionStack = value;
			OnPropertyChanged();
		}
	}

	private string? cover;
	public string? Cover { 
		get => cover ?? "cover.png";
		set {
			cover = FileHelper.RemoveQueryParams(value);
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
	public string FolderName { get => FileHelper.SanitizeFilename(Title); }

	public Podcast(string title, string feedUrl)
	{
		Title = title;
		FeedUrl = feedUrl;
	}
}
