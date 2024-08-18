using System.Collections.ObjectModel;
using System.Diagnostics;
using OPMLCore.NET;
using PodPod.Models;
using PodPod.Services;

namespace PodPod.Views;

public partial class LibraryPage : ContentPage
{
	public ObservableCollection<Podcast> Podcasts { get; set; } = new ObservableCollection<Podcast>();
	public Podcast? SelectedPodcast;

	public LibraryPage()
	{
		InitializeComponent();
		BindingContext = this;
	}

	protected override void OnNavigatedTo(NavigatedToEventArgs e)
    {
		Debug.WriteLine("Navigated to");
        base.OnNavigatedTo(e);
		PrepPodcasts();
	}

	public async void PrepPodcasts(){
		Opml opml = await FeedsService.ProcessOPMLFile();
		List<Podcast> pods = await FeedsService.CreatePodcastList(opml);
		Console.WriteLine("Podcasts: " + pods.Count);

		// convert to observable collection
		foreach (var podcast in pods)
		{
			Podcasts.Add(podcast);
		}
		Console.WriteLine("Podcasts: " + Podcasts.Count);
	}

	public async void Podcast_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		try
		{
            Debug.WriteLine($"Podcast Selection Changed: {SelectedPodcast}");
            var podcast = e.CurrentSelection.FirstOrDefault() as Podcast;
            Debug.WriteLine($"Selected Podcast: {podcast.Title}");

            await Shell.Current.GoToAsync($"{nameof(PodcastPage)}",
                new Dictionary<string, object>
                {
                    ["Podcast"] = podcast
                });
        } catch (Exception err)
		{
			Debug.WriteLine(err.Message);
		}
	}
}
