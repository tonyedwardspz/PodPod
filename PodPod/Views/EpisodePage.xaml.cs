using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using PodPod.Models;
using PodPod.Services;

namespace PodPod.Views;

[QueryProperty(nameof(Episode), nameof(Episode))]
[QueryProperty(nameof(Podcast), nameof(Podcast))]
public partial class EpisodePage : ContentPage
{
	private Episode episode;
	public Episode Episode
	{
		get { return episode; }
		set
		{
			episode = value;
			OnPropertyChanged(nameof(Episode));
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

	public EpisodePage()
	{
		Debug.WriteLine("Episode Page Constructor");
		InitializeComponent();
		BindingContext = this;
	}

	protected async override void OnNavigatedTo(NavigatedToEventArgs e)
    {
		Debug.WriteLine("Navigated to " + Episode.Title);
        base.OnNavigatedTo(e);
		
		if (Episode.Transcription != null){	
			TranscriptionContainer.Children.Clear();
			TranscriptionContainer.Children.Add(updateTranscription(Episode));
		} 
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		this.Title = $"Episode: {Episode.Title}";
	}

	public VerticalStackLayout updateTranscription(Episode episode)
	{
		VerticalStackLayout transcriptionContainer = new VerticalStackLayout();
		transcriptionContainer.HorizontalOptions = LayoutOptions.FillAndExpand;

		try
		{
			bool PartSentence = false;
			Label partLabel = new Label();
            foreach (var spanData in Episode.Transcription)
            {
                Label label = new Label();
				label.FontSize = 16;
				label.HorizontalTextAlignment = TextAlignment.Start;
				label.VerticalTextAlignment = TextAlignment.Center;
                label.FormattedText = new FormattedString();

				TimeSpan start = (TimeSpan)spanData["Start"];
				string formattedTime = start.ToString(@"hh\:mm\:ss");

				if(PartSentence){
					partLabel.FormattedText.Spans[2].Text += $" {spanData["Text"]}";
					label = partLabel;
					PartSentence = false;
				} else {
					var span = new Span{ Text = $"{formattedTime}"};
					label.FormattedText.Spans.Add(span);

					span = new Span{ Text = " - "};
					label.FormattedText.Spans.Add(span);

					span = new Span{ Text = $"{spanData["Text"]}"};
					label.FormattedText.Spans.Add(span);
				}

				string text = spanData["Text"].ToString().Trim();
				if (text.EndsWith(".") || text.EndsWith("?") || text.EndsWith("!") || text.EndsWith("]")){
					PartSentence = false;
					transcriptionContainer.Children.Add(label);
				} else {
					PartSentence = true;
					partLabel = label;
				}
            }
        } catch (Exception e)
		{
			Debug.WriteLine(e.Message);
		}
		return transcriptionContainer;
	}

	public async void TranscribeEpisode(object sender, EventArgs e){
		Debug.WriteLine("Transcribe Episode Clicked at " + new DateTime().ToShortDateString());

		if (sender is Button button)
		{
			MainThread.BeginInvokeOnMainThread(() => button.Text = "Downloading");
			Episode.MediaURL = await DownloadService.DownloadPodcastEpisode(Episode.MediaURL, Episode.Title);

			MainThread.BeginInvokeOnMainThread(() => button.Text = "Transcribing");
			Episode = await TranscriptionService.StartTranslationAsync(Episode.MediaURL, Episode);

			MainThread.BeginInvokeOnMainThread(() => button.Text = "Transcribed");
			MainThread.BeginInvokeOnMainThread(() => button.IsEnabled = false);

			TranscriptionContainer.Clear();
			TranscriptionContainer.Add(updateTranscription(Episode));

            var index = Data.Podcasts.FindIndex(p => p.Title.ToLower() == Podcast.Title.ToLower());
            Data.Podcasts[index] = Podcast;
        }
	}

	public async void DownloadEpisode(object sender, EventArgs e)
	{
		Debug.WriteLine($"Download episode clicked");
        if (sender is Button button)
        {
			MainThread.BeginInvokeOnMainThread(() => button.Text = "Downloading");
			Episode.MediaURL = await DownloadService.DownloadPodcastEpisode(Episode.MediaURL, Episode.Title);
			MainThread.BeginInvokeOnMainThread(() => button.Text = "Downloaded");
			MainThread.BeginInvokeOnMainThread(() => button.IsEnabled = false);
        }
    }

	public async void PlayEpisode(object sender, EventArgs e)
	{
        if (sender is Button button)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
			{
				if (Shell.Current is AppShell shell)
				{
					MediaElement player = shell.GetPlayer();
					shell.CurrentEpisode = Episode;
					player.Source = Episode?.MediaURL;
					player.Play();

					Label details = shell.GetPodcastDetails();
					details.Text = $"Series: {Podcast.Title}";

					Label episodeDetails = shell.GetEpisodeDetails();
					episodeDetails.Text = $"Episode: {episode?.Title}";
				}
			});
        }
    }
}
