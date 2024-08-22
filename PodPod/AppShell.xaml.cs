using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using PodPod.Models;
using PodPod.Services;

namespace PodPod;

public partial class AppShell : Shell
{

	public Episode CurrentEpisode { get; set; }
    public ObservableCollection<Episode> CurrentEpisodeList { get; set; } = new ObservableCollection<Episode>();
    public ObservableCollection<Episode> Playlist { get; set; } = new ObservableCollection<Episode>();

    internal bool playlistSelection = false;
	public AppShell()
	{
		InitializeComponent();
	}

	protected override async void OnNavigating(ShellNavigatingEventArgs args)
    {
        base.OnNavigating(args);
        Debug.WriteLine($"Navigation to {args.Target.Location} started");

        Player.PropertyChanged += Player_PropertyChanged;
    }

	void Player_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == MediaElement.DurationProperty.PropertyName)
        {
            Debug.WriteLine($"Duration: {Player.Duration}" );
            PositionSlider.Maximum = Player.Duration.TotalSeconds;
            DurationLabel.Text = Player.Duration.ToString(@"hh\:mm\:ss");
        }
    }

    async void Slider_DragCompleted(object? sender, EventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);

        var newValue = ((Slider)sender).Value;
        await Player.SeekTo(TimeSpan.FromSeconds(newValue), CancellationToken.None);

        Player.Play();
    }

    void Slider_DragStarted(object sender, EventArgs e)
    {
        Player.Pause();
    }

    void OnPositionChanged(object? sender, MediaPositionChangedEventArgs e)
    {
        PositionSlider.Value = e.Position.TotalSeconds;
        PositionLabel.Text = Player.Position.ToString(@"hh\:mm\:ss");
    }

    void OnPlayPauseClicked(object sender, EventArgs e)
    {
        if (Player.CurrentState == MediaElementState.Playing)
        {
            Player.Pause();
            PlayPause.Text = "Play";
        } else
        {
            Player.Play();
            PlayPause.Text = "Pause";
        }
    }

    void OnStopClicked(object sender, EventArgs e)
    {
        Player.Stop();
    }

    void OnJumpClicked(object sender, EventArgs e)
    {
        Player.SeekTo(Player.Position.Add(TimeSpan.FromSeconds(20)), CancellationToken.None);
    }

    void OnNextClicked(object sender, EventArgs e)
    {
        PlayNextPlaylistItem();
    }

    void PlayNextPlaylistItem()
    {
        var index = Playlist.IndexOf(CurrentEpisode);
        Debug.WriteLine($"Current Index: {index}");
        if (index < Playlist.Count - 1)
        {
            Episode item = Playlist[index + 1];
            Playlist.Remove(CurrentEpisode);
            PlayMedia(item, Playlist.ToList());
        }
    }

    void OnMediaOpened(object? sender, EventArgs e)
    {
        Debug.WriteLine("Media opened.");

        PlayPause.IsEnabled = true;
        Stop.IsEnabled = true;
        JumpForward.IsEnabled = true;
        Next.IsEnabled = true;
        PlayPause.Text = "Pause";

        PositionLabel.Text = Player.Position.ToString(@"hh\:mm\:ss");
        DurationLabel.Text = Player.Duration.ToString(@"hh\:mm\:ss");
    }
    
    void UpdatePlayList(bool playlistSelection)
    {
        ObservableCollection<Episode> tmpEpisodeList = new ObservableCollection<Episode>();
        if (playlistSelection && Playlist.Count > 0)
            tmpEpisodeList = new ObservableCollection<Episode>(Playlist);
        else
            tmpEpisodeList = new ObservableCollection<Episode>(CurrentEpisodeList);

        Playlist.Clear();

        foreach (var episode in tmpEpisodeList)
        {
            if (episode != CurrentEpisode)
            {
                Playlist.Add(episode);

                if (Playlist.Count > 10)
                    break;
            }
        }
        PlaylistArea.Clear();

        var count = 0;
        foreach(var episode in Playlist)
        {
            Label lbl = new Label();
            lbl.Text = episode.Title;
            lbl.TextColor = Color.FromArgb("#000000");
            lbl.Margin = new Thickness(0, 0, 0, 10);
            lbl.MinimumHeightRequest = 37;
            lbl.VerticalTextAlignment = TextAlignment.Center;

            lbl.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OnPlaylistItemClicked(lbl))
            });

            if (episode == CurrentEpisode)
                lbl.FontAttributes = FontAttributes.Bold; 
            
            lbl.BackgroundColor = Playlist.IndexOf(episode) % 2 == 0 ? Color.FromArgb("#F0F0F0") : Color.FromArgb("#D0D0D0");

            PlaylistArea.Add(lbl);

            if (count == 5)
            {
                break;
            }
            count++;
        }
        Debug.WriteLine($"Playlist length: {Playlist.Count}");
    }

    public async void PlayMedia(Episode episode, List<Episode> Episodes, string PodcastTitle = "")
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            CurrentEpisodeList = Episodes.ToObservableCollection();
            CurrentEpisode = episode;
            Player.Source = episode?.MediaURL;
            Player.Play();
            EpisodeDetails.Text = $"Episode: {episode?.Title}";

            if (PodcastTitle != "")
            {
                PodcastDetails.Text = $"Series: {PodcastTitle}";
                UpdatePlayList(true);
            } else
            {
                UpdatePlayList(false);
            }
        });
    }

    void OnMediaEnded(object? sender, EventArgs e)
    {
        Debug.WriteLine("Media ended.");
        CurrentEpisode.Played = true;
        Data.SaveToJsonFile(Data.Podcasts, "podcasts");
        PlayNextPlaylistItem();
    }
    
    private void OnPlaylistItemClicked(object sender)
    {
        var lbl = (Label)sender;
        var episode = Playlist.Where(episode => episode.Title == lbl.Text).FirstOrDefault();
        Debug.WriteLine($"Episode Selected from playlist: {episode?.Title}");
        if (episode != null)
            PlayMedia(episode, Playlist.ToList());
    }
}
