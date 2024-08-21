using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

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

    public string FileName { 
        get {
            string fileName = Title.Replace("&", "and");
            
		    fileName = System.Text.RegularExpressions.Regex.Replace(fileName, "[^a-zA-Z0-9 ]", "");
            fileName = System.Text.RegularExpressions.Regex.Replace(fileName, @"\s+", " ");
            return fileName;
        }
    }

    public Episode() { }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

    public void SaveTranscription(string seriesName, List<Dictionary<string, object>> data)
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
