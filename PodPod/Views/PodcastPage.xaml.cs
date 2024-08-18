using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using FFMpegCore;
using Podly.FeedParser;
using PodPod.Models;
using Whisper.net.Ggml;
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

	protected override void OnNavigatedTo(NavigatedToEventArgs e)
    {
		Debug.WriteLine("Navigated to Podcast Page");
        base.OnNavigatedTo(e);

        var factory = new HttpFeedFactory();
		var feed = factory.CreateFeed(new Uri(Podcast.FeedUrl)) as Rss20Feed;

		Debug.WriteLine($"Feed for {feed.Title} retrieved");

		var description = feed.Description;
        PodcastDescription.Text = "Description:  " + description;

		Podcast.Cover = feed.CoverImageUrl;
		Cover.Source = Podcast.Cover;

        foreach (var item in feed.Items)
        {
            Episodes.Add(new Episode
			{
				Title = item.Title,
				MediaURL = item.MediaUrl,
				Description = item.Content,
				Published = item.DatePublished
			});
        }

		EpisodeCount.Text = $"Episodes: {Episodes.Count}";

		
    }

	// Implement the click event
	//<Button Text="Transcribe" Clicked="TranscribeEpisode" />
	public async void TranscribeEpisode(object sender, EventArgs e){
		Debug.WriteLine("Transcribe Episode Clicked");

		// get the selected episode
		

		// call the TranscribePodcastEpisode
		TranscriptionService.TranscribePodcastEpisode(Episodes[0]);
	}

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

			Label details = shell.GetPodcastDetails();
			details.Text = $"Series: {Podcast.Title}";

			Label episodeDetails = shell.GetEpisodeDetails();
			episodeDetails.Text = $"Episode: {episode?.Title}";
        }

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
