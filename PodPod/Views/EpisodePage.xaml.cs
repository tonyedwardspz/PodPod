using System.Diagnostics;
using FFMpegCore;
using PodPod.Models;
using PodPod.Services;
using Whisper.net;
using Whisper.net.Ggml;

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
		Debug.WriteLine("Navigated to");
        base.OnNavigatedTo(e);
		
		if (Episode.Transcription != null){
			TranscriptionContainer.IsVisible = true;			
			TranscriptionContainer = updateTranscription(Episode);
		} 
	}

	public VerticalStackLayout updateTranscription(Episode episode)
	{
		VerticalStackLayout transcriptionContainer = new VerticalStackLayout();

		foreach (var spanData in Episode.Transcription)
		{
			Label label = new Label();
			label.FormattedText = new FormattedString();
			var span = new Span
			{
				Text = $"{spanData["Start"]}->{spanData["End"]}: {spanData["Text"]}",
			};
			
			label.FormattedText.Spans.Add(span);
			transcriptionContainer.Children.Add(label);
		}

		return transcriptionContainer;
	}

	public Command BackOverrideCommand
	{
		get
		{
            Debug.WriteLine("Back button override pressed");
			return new Command(async () =>
			{
				await Shell.Current.GoToAsync($"{nameof(PodcastPage)}",
                new Dictionary<string, object>
                {
                    ["Podcast"] = Podcast
                });
			});
		}
	}


	
}
