using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using OPMLCore.NET;
using PodPod.Models;
using PodPod.Services;
using PodPod.Views;

namespace PodPod;

public partial class AppShell : Shell
{
    private PlayerState _playerState = new PlayerState();
    public PlayerState playerState {
        get => _playerState;
        set {
            _playerState = value;
            OnPropertyChanged();
        }
    }

    internal bool playlistSelection = false;

	public AppShell()
	{
		InitializeComponent();
        BindingContext = this;
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
            playerState.Duration = Player.Duration.ToString(@"hh\:mm\:ss");
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
        playerState.Position = Player.Position;
        playerState.Duration = Player.Duration.ToString(@"hh\:mm\:ss");
    }

    void OnPlayPauseClicked(object sender, EventArgs e)
    {
        if (Player.CurrentState == MediaElementState.Playing)
        {
            Player.Pause();
            playerState.PlayButtonText = "Play";
        } else
        {
            Player.Play();
            playerState.PlayButtonText = "Pause";
        }
    }

    void OnStopClicked(object sender, EventArgs e)
    {
        playerState.PlayButtonText = "Play";
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
        Debug.WriteLine("Next Playlist Item");
        var index = playerState.Playlist.IndexOf(playerState.CurrentEpisode);
        Debug.WriteLine($"Current Index: {index}");
        if (index < playerState.Playlist.Count - 1)
        {
            Episode item = playerState.Playlist[index + 1];
            playerState.Playlist.Remove(playerState.CurrentEpisode);
            PlayMedia(item, playerState.Playlist.ToList());
        }
    }

    void OnMediaOpened(object? sender, EventArgs e)
    {
        Debug.WriteLine("Media opened.");

        playerState.IsPlayEnabled = true;
        playerState.IsStopEnabled = true;
        playerState.IsJumpEnabled = true;
        playerState.IsNextEnabled = true;
        playerState.PlayButtonText = "Pause";
        playerState.Duration = Player.Duration.ToString(@"hh\:mm\:ss");
        playerState.Duration = Player.Position.ToString(@"hh\:mm\:ss");
    }

    public async void PlayMedia(Episode episode, List<Episode> Episodes, string PodcastTitle = "", TimeSpan timestamp = new TimeSpan())
    {
        Debug.WriteLine("Play Media");

        if (playerState.Source != episode?.MediaURL)
        {
            playerState.Source = episode?.MediaURL;
            playerState.CurrentEpisode = episode;
            playerState.Title = episode?.Title;
            playerState.PodcastTitle = PodcastTitle;
            playerState.CurrentEpisode = episode;
            playerState.Playlist = Episodes.ToObservableCollection();
            PositionSlider.Value = 0;
        }

        if (timestamp != new TimeSpan()){
            await Task.Delay(150);
            await Player.SeekTo(timestamp, CancellationToken.None);
        }
    }

    void OnMediaEnded(object? sender, EventArgs e)
    {
        Debug.WriteLine("Media ended.");
        playerState.CurrentEpisode.Played = true;
        Data.SaveToJsonFile(Data.Podcasts, "podcasts");
        PlayNextPlaylistItem();
    }
    
    private void PlaylistItem_Clicked(object sender, SelectionChangedEventArgs e)
    {
        Debug.WriteLine("Playlist Item Clicked");

        var episode = e.CurrentSelection.FirstOrDefault() as Episode;
        Debug.WriteLine($"Episode Selected from playlist: {episode?.Title}");
        if (episode != null){
            PlayMedia(episode, playerState.Playlist.ToList());
        }
    }

    public async void ImportOPML_Clicked(object sender, EventArgs e)
    {
        Debug.WriteLine("Import OPML Clicked");

        Opml opml = await FeedsService.DownloadAndProcessOPMLFile();
        Data.Podcasts = await FeedsService.CreatePodcastList(opml);

        var currentPage = Shell.Current.CurrentPage;
        if (currentPage is LibraryPage)
        {
            LibraryPage page = (LibraryPage)currentPage;
            page.Podcasts = Data.Podcasts.ToObservableCollection();
            page.ScrollToTop();
        }
        else
        {
            await Shell.Current.GoToAsync($"{nameof(LibraryPage)}");
        }
        Console.WriteLine("Podcasts loaded from import: " + Data.Podcasts.Count);
    }

    public async void JumpToTimeStamp(Episode ep, TimeSpan timestamp, string podcastTitle)
    {
        Debug.WriteLine($"Jumping to timestamp {timestamp}");
        PlayMedia(ep, playerState.Playlist.ToList(), podcastTitle, timestamp);        
    }
}


