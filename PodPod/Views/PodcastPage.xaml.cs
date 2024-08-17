using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using Podly.FeedParser;
using PodPod.Models;

namespace PodPod.Views;

[QueryProperty(nameof(Podcast), nameof(Podcast))]
public partial class PodcastPage : ContentPage
{
	private ObservableCollection<Episode> episodes;
	public ObservableCollection<Episode> Episodes
	{
		get { return episodes; }
		set
		{
			episodes = value;
			OnPropertyChanged(nameof(Episodes));
		}
	}

	public Podcast Podcast { get; set; }
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
		this.Title = $"{Podcast.Title} series page";

		var factory = new HttpFeedFactory();
        var feed = factory.CreateFeed(new Uri(Podcast.FeedUrl));

		Episodes = new ObservableCollection<Episode>();
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
	}

	public void Episode_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
	}
}
