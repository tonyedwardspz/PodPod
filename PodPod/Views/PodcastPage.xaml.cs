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

	public Episode? SelectedEpisode { get; set; }

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

		VerticalStackLayout stackLayout = HTMLHelper.ProcessHTML(Podcast.Description);
        DescriptionContainer.Children.Add(stackLayout);

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
					episode.TranscriptionButtonText = "Preparing";
					await DownloadService.DownloadPodcastEpisode(episode, Podcast.FolderName);
					await TranscriptionService.StartTranscriptionAsync(episode, Podcast.FolderName);
					Data.SaveToJsonFile(Data.Podcasts, "podcasts");
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

        var ep = e.CurrentSelection.FirstOrDefault() as Episode;
		Episode episode = Podcast.Episodes.FirstOrDefault(e => e.Id == ep?.Id);

		try
		{
			await Shell.Current.GoToAsync($"{nameof(EpisodePage)}",
				new Dictionary<string, object>
				{
					["Episode"] = episode,
					["Podcast"] = Podcast
				});
			SelectedEpisode = null;
		} catch (Exception err)
		{
			Debug.WriteLine(err.Message);
		}
	}
}
