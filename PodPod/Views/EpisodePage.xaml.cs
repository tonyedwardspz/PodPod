using System.Diagnostics;
using System.Windows.Input;
using PodPod.Models;
using PodPod.Services;

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
		Debug.WriteLine("Navigated to " + Episode.Title);
        base.OnNavigatedTo(e);

		VerticalStackLayout stackLayout = HTMLHelper.ProcessHTML(Episode.Description);
        DescriptionContainer.Children.Add(stackLayout);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		this.Title = $"Episode: {Episode.Title}";
	}

	public async void TranscribeEpisode(object sender, EventArgs e){
		Debug.WriteLine("Transcribe Episode Clicked at " + DateTime.Now);

		if (Episode != null && Episode.Transcription == null && Podcast != null)
		{
			try
			{
				Episode.TranscriptionButtonText = "Preparing";
				await DownloadService.DownloadPodcastEpisode(Episode, Podcast.FolderName);
				await TranscriptionService.StartTranscriptionAsync(Episode, Podcast.FolderName);
				Data.SaveToJsonFile(Data.Podcasts, "podcasts");
			}
			catch (Exception err)
			{
				Debug.WriteLine(err.Message);
			}
		}
	}

	public async void DownloadEpisode(object sender, EventArgs e)
	{
		Debug.WriteLine($"Download episode clicked");
        await DownloadService.DownloadPodcastEpisode(Episode, Podcast.FolderName);
    }

	public async void PlayEpisode(object sender, EventArgs e)
	{
		if (Shell.Current is AppShell shell)
		{
			var nextEpisodes = Podcast.Episodes.SkipWhile(ep => ep.Title != Episode.Title).Skip(1).Take(10).ToList();
			shell.PlayMedia(episode, nextEpisodes, Podcast.Title);
		}
    }

	public ICommand TimeTappedCommand => new Command<string>(async (timestamp) => {
		Debug.WriteLine($"Timestamp Clicked: {timestamp} of {Episode.Title}");

		if (timestamp == "00:00:00")
			timestamp = "00:00:01";

		TimeSpan parsedTime;
		if (TimeSpan.TryParse(timestamp, out parsedTime))
		{
			if (Shell.Current is AppShell shell)
				shell.JumpToTimeStamp(Episode, TimeSpan.Parse(timestamp), Podcast.Title);
		}
		else
		{
			Debug.WriteLine("Could not jump to timestamp");
		}
	});

	public async void Timestamp_Clicked(object sender, EventArgs e)
	{
		Debug.WriteLine("Timestamp Clicked");
		var label = (Label)sender;
		var timestamp = label.Text;
		Debug.WriteLine($"Timestamp: {timestamp}");

		if (Shell.Current is AppShell shell)
		{
			shell.JumpToTimeStamp(Episode, TimeSpan.Parse(timestamp), Podcast.Title);
		}
	}
}
