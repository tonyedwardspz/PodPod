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

	public PodcastPage()
	{
		InitializeComponent();
		BindingContext = this;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Debug.WriteLine("Podcast Page Appearing");
		Debug.WriteLine($"Podcast: {Podcast.Title}");
		this.Title = $"{Podcast.Title}";
	}

	protected override async void OnNavigatedTo(NavigatedToEventArgs e)
    {
		Debug.WriteLine("Navigated to Podcast Page");
        base.OnNavigatedTo(e);

		if (Podcast.Description != null)
		{
			MainThread.BeginInvokeOnMainThread(() => PodcastDescription.Text = "Description:  " + Podcast.Description);
		}

		if (Podcast.Cover != "cover.png")
		{
			MainThread.BeginInvokeOnMainThread(() => Cover.Source = Podcast.Cover);
        }

		if (Podcast.Episodes.Count > 0)
		{
			Episodes = new ObservableCollection<Episode>(Podcast.Episodes);
			MainThread.BeginInvokeOnMainThread(() => EpisodeCount.Text = Podcast.EpisodeCount.ToString());
			return;
		} else {

			await Task.Run(() =>
			{
                Debug.WriteLine("Creating factory");
                var factory = new HttpFeedFactory();
				Podcast.Feed = factory.CreateFeed(new Uri(Podcast.FeedUrl)) as Rss20Feed;
				Debug.WriteLine("Factory Created");
			});

			await Task.Run(() =>
			{
                Debug.WriteLine("Updating to Podcast Page");

                Podcast.Description = Podcast.Feed.Description;
                MainThread.BeginInvokeOnMainThread(() => PodcastDescription.Text = "Description:  " + Podcast.Description);

                Podcast.Cover = Podcast.Feed.CoverImageUrl;
				MainThread.BeginInvokeOnMainThread(() => Cover.Source = Podcast.Cover);

				Podcast.EpisodeCount = Podcast.Feed.Items.Count;
				MainThread.BeginInvokeOnMainThread(() => EpisodeCount.Text = "Episodes: " + Podcast.EpisodeCount);

                Debug.WriteLine("Building Episode list");
                foreach (var item in Podcast.Feed.Items)
				{
					Episodes.Add(new Episode
					{
						Title = item.Title,
						MediaURL = item.MediaUrl,
						Description = item.Content,
						Published = item.DatePublished
					});
				}
				Podcast.Episodes = Episodes.ToList();
                Debug.WriteLine("Episode list built");
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
					MainThread.BeginInvokeOnMainThread(() => button.Text = "Downloading episode...");
					episode.MediaURL = await DownloadService.DownloadPodcastEpisode(episode.MediaURL, episode.Title);

					MainThread.BeginInvokeOnMainThread(() => button.Text = "Transcribing episode...");
					episode = await TranscriptionService.StartTranslationAsync(episode.MediaURL, episode);
					episode.IsUnTranscribed = false;

					MainThread.BeginInvokeOnMainThread(() => button.IsVisible = false);

					var index = Episodes.IndexOf(episode);
					Episodes[index] = episode;
					Podcast.Episodes = Episodes.ToList();
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


    private bool isPlaying = false;
	public async void Episode_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		
		var episode = e.CurrentSelection.FirstOrDefault() as Episode;
		Debug.WriteLine($"Episode Selected: {episode?.Title}");

		if (Shell.Current is AppShell shell)
        {
            MediaElement player = shell.GetPlayer();
			shell.CurrentEpisodeList = Episodes;
			shell.CurrentEpisode = episode;
			player.Source = episode?.MediaURL;
			player.Play();
			isPlaying = true;

			Label details = shell.GetPodcastDetails();
			details.Text = $"Series: {Podcast.Title}";

			Label episodeDetails = shell.GetEpisodeDetails();
			episodeDetails.Text = $"Episode: {episode?.Title}";
        }

		if (isPlaying){
			try
			{
				await Shell.Current.GoToAsync($"{nameof(EpisodePage)}",
					new Dictionary<string, object>
					{
						["Episode"] = episode,
						["Podcase"] = Podcast
					});

			} catch (Exception err)
			{
				Debug.WriteLine(err.Message);
			}
		}
	}
}
