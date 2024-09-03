using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Core.Extensions;
using PodPod.Helpers;
using PodPod.Models;
using PodPod.Services;

namespace PodPod.Views;

public partial class LatestPage : ContentPage
{
	private ObservableCollection<Episode> latestPodcasts = new ObservableCollection<Episode>();
	public ObservableCollection<Episode> LatestPodcasts {
		get => latestPodcasts;
		set{
			latestPodcasts = value;
			OnPropertyChanged(nameof(LatestPodcasts));
		}
	}

	public LatestPage()
	{
		InitializeComponent();
		BindingContext = this;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Debug.WriteLine("Appearing");
		LatestPodcasts = PodcastHelper.GetLatestEpisodes().ToObservableCollection<Episode>();
	}

	public void PlayEpisode(object sender, EventArgs e)
	{
        if (sender is Button button)
        {
            var episode = button.BindingContext as Episode;
            if (episode != null)
            {
				if (Shell.Current is AppShell shell)
				{
					var nextEpisodes = LatestPodcasts.SkipWhile(e => e.Id != episode.Id).Skip(1).Take(10);
					ObservableCollection<Episode> eps = new ObservableCollection<Episode>(nextEpisodes);
					shell.PlayMedia(episode, eps);
				}
            }
        }
    }

	public async void ViewEpisode(object sender, EventArgs e)
	{
		Debug.WriteLine($"View episode clicked. Not implenetd yet");

		// if (Podcast == null) return;

        // if (sender is Button button)
        // {
        //     var episode = button.BindingContext as Episode;
        //     if (episode != null)
        //     {
        //         await MainThread.InvokeOnMainThreadAsync(async () =>
        //         {
        //             await Shell.Current.GoToAsync($"{nameof(EpisodePage)}",
        //                 new Dictionary<string, object>
        //                 {
        //                     ["Episode"] = episode,
        //                     ["Podcast"] = Podcast
        //                 });
        //         });
        //     }
        // }
    }
}
