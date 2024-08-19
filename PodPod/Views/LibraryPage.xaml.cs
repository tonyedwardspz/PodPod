using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Core.Extensions;
using OPMLCore.NET;
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
	public Podcast SelectedPodcast { get; set; }

	public LibraryPage()
	{
		InitializeComponent();
		BindingContext = this;
		Podcasts = new ObservableCollection<Podcast>();
	}

	protected override void OnNavigatedTo(NavigatedToEventArgs e)
    {
		Debug.WriteLine("Navigated to Library Page");
        base.OnNavigatedTo(e);

		if (Data.Podcasts.Count > 0)
		{
			Podcasts = Data.Podcasts.ToObservableCollection();

		}
		else
		{
			PrepPodcasts();
		}
	}

	public async void PrepPodcasts(){
		Opml opml = await FeedsService.ProcessOPMLFile();
		List<Podcast> pods = await FeedsService.CreatePodcastList(opml);

		foreach (var podcast in pods)
		{
			Podcasts.Add(podcast);
		}
		Console.WriteLine("Podcasts: " + Podcasts.Count);
		Data.Podcasts = pods;
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
			SelectedPodcast = null;
        } catch (Exception err)
		{
			Debug.WriteLine(err.Message);
		}
	}
}
