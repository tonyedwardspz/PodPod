using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using Podly.FeedParser;
using PodPod.Models;
using PodPod.Services;

namespace PodPod.Views;

[QueryProperty(nameof(Podcast), nameof(Podcast))]
public partial class PodcastPage : ContentPage
{
	private ObservableCollection<Episode> episodes = new ObservableCollection<Episode>();
	public ObservableCollection<Episode> Episodes
	{
		get { return episodes; }
		set
		{
			episodes = value;
			OnPropertyChanged(nameof(Episodes));
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

	public Podcast SelectedPodcast { get; set; }

	bool PageLoaded = false;

	public PodcastPage()
	{
		InitializeComponent();
		BindingContext = this;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		this.Title = $"Podcast: {Podcast.Title}";
	}

	protected override async void OnNavigatedTo(NavigatedToEventArgs e)
    {
		Debug.WriteLine("Navigated to Podcast Page");
		PageLoaded = true;

        base.OnNavigatedTo(e);

		if (Podcast.Episodes.Count > 0)
		{
			Episodes = new ObservableCollection<Episode>(Podcast.Episodes);
			return;
		} else {
			
			IFeed feed = null;

			await Task.Run(() =>
			{
                Debug.WriteLine("Fetching feed");
                var factory = new HttpFeedFactory();
				feed = factory.CreateFeed(new Uri(Podcast.FeedUrl)) as Rss20Feed;
				Debug.WriteLine("Feed fetched");
			});

			await Task.Run(() =>
			{
                Debug.WriteLine("Updating to Podcast Page");

				Podcast pod = Podcast;

                pod.Description = feed.Description;
				pod.Cover = feed.CoverImageUrl;
				pod.LastPublished = feed.LastUpdated;

				List<Episode> eps = new List<Episode>();
                Debug.WriteLine("Building Episode list");
                foreach (var item in feed.Items)
				{
					eps.Add(new Episode
					{
						Id = item.Id,
						Title = item.Title,
						MediaURL = item.MediaUrl,
						Description = item.Content,
						Published = item.DatePublished,
						Cover = item.Cover,
						Author = item.Author,
						Link = item.Link,
						EpisodeNumber = item.EpisodeNumber,
						Duration = item.MediaLength
					});
				}
				pod.Episodes = eps;
				Episodes = new ObservableCollection<Episode>(eps);
				
                Debug.WriteLine("Episode list built");

				var index = Data.Podcasts.FindIndex(p => p.Title.ToLower() == pod.Title.ToLower());
				Data.Podcasts[index] = pod;
				Podcast = pod;
			});
			Debug.WriteLine($"Podcast page: {Podcast.EpisodeCount} episodes of {Podcast.Title} loaded.");
		}
    }

	public async void TranscribeEpisode(object sender, EventArgs e){
		Debug.WriteLine("Transcribe Episode Clicked");

		if (sender is Button button)
		{
			var episode = button.BindingContext as Episode;
			if (episode != null && episode.Transcription == null)
			{
				try
				{
					MainThread.BeginInvokeOnMainThread(() => button.Text = "Preparing Audio");
					episode.MediaURL = await DownloadService.DownloadPodcastEpisode(episode.MediaURL, episode.Title);

					MainThread.BeginInvokeOnMainThread(() => button.Text = "Transcribing");
					episode = await TranscriptionService.StartTranslationAsync(episode.MediaURL, episode);

					MainThread.BeginInvokeOnMainThread(() => button.IsVisible = false);

					var index = Episodes.IndexOf(episode);
					Episodes[index] = episode;
					Podcast.Episodes = Episodes.ToList();
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

	public async void PlayEpisode(object sender, EventArgs e)
	{
        if (sender is Button button)
        {
            var episode = button.BindingContext as Episode;
            if (episode != null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (Shell.Current is AppShell shell)
					{
						MediaElement player = shell.GetPlayer();
						shell.CurrentEpisodeList = Episodes;
						shell.CurrentEpisode = episode;
						player.Source = episode?.MediaURL;
						player.Play();

						Label details = shell.GetPodcastDetails();
						details.Text = $"Series: {Podcast.Title}";

						Label episodeDetails = shell.GetEpisodeDetails();
						episodeDetails.Text = $"Episode: {episode?.Title}";
					}
                });
            }
        }
    }

	public async void Episode_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!PageLoaded) return;

		var episode = e.CurrentSelection.FirstOrDefault() as Episode;
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
