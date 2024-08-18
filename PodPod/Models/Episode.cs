using System;

namespace PodPod.Models;

public class Episode
{
    public string? Title { get; set; }
    public string? MediaURL { get; set; }
    public string? Description { get; set; }
    public int? EpisodeNumber { get; set; }
    public TimeSpan? Duration { get; set; }
    public DateTime? Published { get; set; }
    public List<Dictionary<string, object>>? Transcription { get; set; }
    public bool IsUnTranscribed { get; set ; } = true;

    public Episode()
	{

	}
}

