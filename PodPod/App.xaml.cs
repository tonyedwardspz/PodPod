using PodPod.Views;

namespace PodPod;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();

		Routing.RegisterRoute("library-page", typeof(LibraryPage));
		Routing.RegisterRoute(nameof(PodcastPage), typeof(PodcastPage));
	}
}

