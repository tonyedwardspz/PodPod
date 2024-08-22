using System.Diagnostics;
using PodPod.Models;
using PodPod.Services;

namespace PodPod.Views;

[QueryProperty(nameof(Podcast), nameof(Podcast))]
public partial class PodcastPage : ContentPage
{
	private Podcast? podcast;
	public Podcast? Podcast
	{
		get { return podcast; }
		set
		{
			podcast = value;
			OnPropertyChanged(nameof(Podcast));
		}
	}

	public Podcast? SelectedPodcast { get; set; }

	bool PageLoaded = false;

	public PodcastPage()
	{
		InitializeComponent();
		BindingContext = this;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		if (Podcast == null) return;
		this.Title = $"Podcast: {Podcast.Title}";
	}

	protected override async void OnNavigatedTo(NavigatedToEventArgs e)
    {
		if (Podcast == null) return;

		Debug.WriteLine("Navigated to Podcast Page");
		base.OnNavigatedTo(e);
		PageLoaded = true; // Prevents a extra write of the json file

		await Task.Run(async () =>
		{
			await FeedsService.FetchFeed(Podcast);
		});
		Debug.WriteLine($"Podcast page: {Podcast.EpisodeCount} episodes of {Podcast.Title} loaded.");
    }

	public async void TranscribeEpisode(object sender, EventArgs e){
		Debug.WriteLine("Transcribe Episode Clicked");

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

					MainThread.BeginInvokeOnMainThread(() => button.IsVisible = false);

					var index = Podcast.Episodes.IndexOf(episode);
					Podcast.Episodes[index] = episode;

					var podIndex = Data.Podcasts.FindIndex(p => p.Title.ToLower() == Podcast.Title.ToLower());
					Data.Podcasts[podIndex] = Podcast;
				}
				catch (Exception err)
				{
					Debug.WriteLine(err.Message);
				}
			}
		}
	}

	public async void ViewEpisode(object sender, EventArgs e)
	{
		Debug.WriteLine($"View episode clicked");

		if (Podcast == null) return;

        if (sender is Button button)
        {
            var episode = button.BindingContext as Episode;
            if (episode != null)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync($"{nameof(EpisodePage)}",
                        new Dictionary<string, object>
                        {
                            ["Episode"] = episode,
                            ["Podcast"] = Podcast
                        });
                });
            }
        }
    }

	public void PlayEpisode(object sender, EventArgs e)
	{
        if (sender is Button button)
        {
            var episode = button.BindingContext as Episode;
            if (episode != null && Podcast != null)
            {
				if (Shell.Current is AppShell shell)
				{
					var nextEpisodes = Podcast.Episodes.SkipWhile(e => e.Id != episode.Id).Skip(1).Take(10).ToList();
					shell.PlayMedia(episode, nextEpisodes, Podcast.Title);
				}
            }
        }
    }

	public void PlayNextEpisode(object sender, EventArgs e)
	{
		if (Podcast == null) return;

		var episode = Podcast.Episodes.FirstOrDefault(e => e.Played == false);
		if (episode != null)
		{
			if (Shell.Current is AppShell shell)
			{
				var nextEpisodes = Podcast.Episodes.SkipWhile(e => e.Id != episode.Id).Skip(1).Take(10).ToList();
				shell.PlayMedia(episode, nextEpisodes, Podcast.Title);
			}
		}
	}

	public void ViewRSSFeed(object sender, EventArgs e)
	{
		Debug.WriteLine("Show RSS Feed Clicked");
		if (Podcast == null) return;
		
		FeedsService.ShowRSSFeed(Podcast.FeedUrl);
	}

	public async void Episode_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!PageLoaded) return;

		var episode = e.CurrentSelection.FirstOrDefault() as Episode;
		if (episode == null || Podcast == null) return;
		
		Debug.WriteLine($"Episode Selected: {episode?.Title}");

		try
		{
			await Shell.Current.GoToAsync($"{nameof(EpisodePage)}",
				new Dictionary<string, object>
				{
					["Episode"] = episode,
					["Podcast"] = Podcast
				});
			SelectedPodcast = null;

		} catch (Exception err)
		{
			Debug.WriteLine(err.Message);
		}
	}
}
