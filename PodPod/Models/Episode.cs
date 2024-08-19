﻿using System;

namespace PodPod.Models;

public class Episode
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? MediaURL { get; set; }
    public string? Description { get; set; }
    public string? Cover { get; set; }
    public string? Author { get; set; }
    public string? Link { get; set; }
    public DateTime? Published { get; set; }
    public string? EpisodeNumber { get; set; }
    public string? Duration { get; set; }

    public List<Dictionary<string, object>>? Transcription { get; set; }
    public bool IsUnTranscribed { get; set ; } = true;

    public Episode()
	{

	}
}

