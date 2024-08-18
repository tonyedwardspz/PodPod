using PodPod.Views;

namespace PodPod;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();

		Routing.RegisterRoute(nameof(LibraryPage), typeof(LibraryPage));
		Routing.RegisterRoute(nameof(PodcastPage), typeof(PodcastPage));
		Routing.RegisterRoute(nameof(EpisodePage), typeof(EpisodePage));
	}
}

