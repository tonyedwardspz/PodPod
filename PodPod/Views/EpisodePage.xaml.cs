using System.Diagnostics;
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

		if (sender is Button button)
		{
			var episode = button.BindingContext as Episode;
			if (episode != null && episode.Transcription == null && Podcast != null)
			{
				try
				{
					MainThread.BeginInvokeOnMainThread(() => button.Text = "Preparing Audio");
					await DownloadService.DownloadPodcastEpisode(episode, Podcast.FolderName);

					MainThread.BeginInvokeOnMainThread(() => button.Text = "Transcribing");
					await TranscriptionService.StartTranscriptionAsync(episode, Podcast.FolderName);

					MainThread.BeginInvokeOnMainThread(() => button.Text = "Transcribed");
					MainThread.BeginInvokeOnMainThread(() => button.IsEnabled = false);
					
                    Data.SaveToJsonFile(Data.Podcasts, "podcasts");
				}
				catch (Exception err)
				{
					Debug.WriteLine(err.Message);
				}
			}
		}
	}

	public async void DownloadEpisode(object sender, EventArgs e)
	{
		Debug.WriteLine($"Download episode clicked");
        if (sender is Button button)
        {
			MainThread.BeginInvokeOnMainThread(() => button.Text = "Downloading");
			await DownloadService.DownloadPodcastEpisode(Episode, Podcast.FolderName);
			MainThread.BeginInvokeOnMainThread(() => button.Text = "Downloaded");
			MainThread.BeginInvokeOnMainThread(() => button.IsEnabled = false);
        }
    }

	public async void PlayEpisode(object sender, EventArgs e)
	{
        if (sender is Button button)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
			{
				if (Shell.Current is AppShell shell)
				{
					var nextEpisodes = Podcast.Episodes.SkipWhile(ep => ep.Title != Episode.Title).Skip(1).Take(10).ToList();
					shell.PlayMedia(episode, nextEpisodes, Podcast.Title);
				}
			});
        }
    }
}
