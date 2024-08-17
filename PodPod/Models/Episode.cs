using System;

namespace PodPod.Models;

public class Episode
{
    public string Title { get; set; }
    public string MediaURL { get; set; }
    public string Description { get; set; }
    public int EpisodeNumber { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime Published { get; set; }

    public Episode()
	{

	}
}

