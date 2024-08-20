using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PodPod.Models;

public class Episode : INotifyPropertyChanged
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    private string? mediaURL;
    public string? MediaURL { 
        get => mediaURL; 
        set {
            mediaURL = value;
            OnPropertyChanged("NeedsDownloading");
        } 
    }
    public bool NeedsDownloading { 
        get {
            if (MediaURL != null && MediaURL.StartsWith("http"))
                return true;
            else
                return false;
        }
    }
    private string? description;
    public string? Description { 
        get => description;
        set {
            description = value;
            OnPropertyChanged();
        }
    }
    public string? Cover { get; set; }
    public string? Author { get; set; }
    public string? Link { get; set; }
    private DateTime? published { get; set; }
    public DateTime? Published { 
        get => published;
        set {
            published = value;
            OnPropertyChanged();
        }
    }
    public string? EpisodeNumber { get; set; }
    private string? duration;
    public string? Duration { 
        get => duration;
        set {
            duration = value;
            OnPropertyChanged();
        }
    }
    private List<Dictionary<string, object>>? transcription;
    public List<Dictionary<string, object>>? Transcription { 
        get => transcription; 
        set {
            transcription = value;
            OnPropertyChanged("NeedsTranscribing");
        }
    }
    public bool NeedsTranscribing { 
        get {
            if (Transcription != null && Transcription.Count > 0)
                return false;
            else
                return true;
        }
    }

    public Episode() { }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
