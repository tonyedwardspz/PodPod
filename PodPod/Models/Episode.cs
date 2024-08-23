using System.Text.Json;
using PodPod.Helpers;

namespace PodPod.Models;

public class Episode : Base
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    private string? mediaURL;
    public string? MediaURL { 
        get => mediaURL; 
        set {
            mediaURL = FileHelper.RemoveQueryParams(value);
            OnPropertyChanged();
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
    private string cover;
    public string? Cover { 
        get => cover ?? "cover.png";
        set {
            cover = FileHelper.RemoveQueryParams(value);
            OnPropertyChanged();
        }
    }
    public string? Author { get; set; }
    private string link;
    public string? Link { 
        get => link;
        set {
            link = FileHelper.RemoveQueryParams(value);
            OnPropertyChanged();
        }
    }
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
    private Transcription? transcription;
    public Transcription? Transcription { 
        get => transcription; 
        set {
            transcription = value;
            OnPropertyChanged();
            OnPropertyChanged("NeedsTranscribing");
        }
    }
    public bool NeedsTranscribing { 
        get {
            if (Transcription != null && Transcription.Items.Count > 0)
                return false;
            else
                return true;
        }
    }
    public string FileName { get => FileHelper.SanitizeFilename(Title); }
    public bool Played { get; set; } = false;
    public Episode() { }

    public void SaveTranscription(string seriesName, Transcription data)
    {
        try {
            var jsonString = JsonSerializer.Serialize(data);
            var path = AppPaths.EpisodeTranscriptionFilePath(seriesName, FileName);
            File.WriteAllText(path, jsonString);
        } catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
