using System;

namespace PodPod.Models;

public class Transcription: Base
{
    private List<TranscriptionItem> items { get; set; }
    public List<TranscriptionItem> Items { 
        get => items;
        set {
            items = value;
            OnPropertyChanged();
        }
    }

	public Transcription()
	{
       items = new List<TranscriptionItem>();
    }
}

public class TranscriptionItem
{
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
    public string Text { get; set; }
}