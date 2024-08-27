using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Core.Extensions;
using Podly.FeedParser;
using PodPod.Models;
using PodPod.Services;

namespace PodPod.Views;

public partial class LibraryPage : ContentPage
{
	private ObservableCollection<Podcast> podcasts;
	public ObservableCollection<Podcast> Podcasts { 
		get {
			return podcasts;
		}
		set {
			podcasts = value;
			OnPropertyChanged(nameof(Podcasts));
		}
	}
	public Podcast? SelectedPodcast { get; set; }

	public LibraryPage()
	{
		InitializeComponent();
		BindingContext = this;
		Podcasts = Data.Podcasts.ToObservableCollection();
	}

	protected override void OnNavigatedTo(NavigatedToEventArgs e)
    {
		Debug.WriteLine("Navigated to Library Page");
        base.OnNavigatedTo(e);

		if (Podcasts.Count == 0)
		{
			Debug.WriteLine("No podcasts found, loading from OPML");
			PrepPodcasts();
		}

		FeedsService.DownloadAllFeeds();
	}

	public async void PrepPodcasts()
	{
		Opml opml = await FeedsService.ProcessOPMLFile();
		Data.Podcasts = await FeedsService.CreatePodcastList(opml);
		Podcasts = Data.Podcasts.ToObservableCollection(); // TODO: This is still poor form. Refactor to obervable collection?
		Console.WriteLine("Podcasts loaded: " + Podcasts.Count);
	}

	public async void Podcast_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		try
		{
            Debug.WriteLine($"Podcast Selection Changed: {SelectedPodcast}");

            await Shell.Current.GoToAsync($"{nameof(PodcastPage)}",
                new Dictionary<string, object>
                {
                    ["Podcast"] = SelectedPodcast
                });
			SelectedPodcast = null;
        } catch (Exception err)
		{
			Debug.WriteLine(err.Message);
		}
	}

	public void ScrollToTop() => PodcastCollection.ScrollTo(0);
}
