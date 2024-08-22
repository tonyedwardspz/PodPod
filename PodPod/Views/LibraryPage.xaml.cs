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
	public Podcast? SelectedPodcast { get; set; }

	public LibraryPage()
	{
		InitializeComponent();
		BindingContext = this;
		Podcasts = Data.Podcasts.ToObservableCollection() ?? new ObservableCollection<Podcast>();
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
	}

	public async void PrepPodcasts(){
		Opml opml = await FeedsService.ProcessOPMLFile();
		List<Podcast> pods = await FeedsService.CreatePodcastList(opml);
		Podcasts = pods.ToObservableCollection();
		Console.WriteLine("Podcasts loaded: " + Podcasts.Count);
		Data.Podcasts = pods;
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
}
