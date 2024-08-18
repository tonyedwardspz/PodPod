using System.Diagnostics;
using FFMpegCore;
using PodPod.Models;
using PodPod.Services;
using Whisper.net;
using Whisper.net.Ggml;

namespace PodPod.Views;

[QueryProperty(nameof(Episode), nameof(Episode))]
[QueryProperty(nameof(Podcast), nameof(Podcast))]
public partial class EpisodePage : ContentPage
{
	private Episode episode;
	public Episode Episode
	{
		get { return episode; }
		set
		{
			episode = value;
			OnPropertyChanged(nameof(Episode));
		}
	}

	private Podcast podcast;
	public Podcast Podcast
	{
		get { return podcast; }
		set
		{
			podcast = value;
			OnPropertyChanged(nameof(Podcast));
		}
	}

	public EpisodePage()
	{
		Debug.WriteLine("Episode Page Constructor");
		InitializeComponent();
		BindingContext = this;
	}

	protected async override void OnNavigatedTo(NavigatedToEventArgs e)
    {
		Debug.WriteLine("Navigated to");
        base.OnNavigatedTo(e);
		
		var filePath = DownloadService.DownloadPodcastEpisode(Episode.MediaURL, Episode.Title);
		if (filePath != null)
		{
			Console.WriteLine("Downloaded " + Episode.Title + " to " + filePath);
			TranscribePodcastEpisode(filePath);
		}
		else
		{
			Console.WriteLine("Failed to download " + Episode.Title);
		}
	}

	public Command BackOverrideCommand
	{
		get
		{
            Debug.WriteLine("Back button override pressed");
			return new Command(async () =>
			{
				await Shell.Current.GoToAsync($"{nameof(PodcastPage)}",
                new Dictionary<string, object>
                {
                    ["Podcast"] = Podcast
                });
			});
		}
	}


	
}
