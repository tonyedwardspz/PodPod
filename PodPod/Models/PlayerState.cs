using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;

namespace PodPod.Models;

public class PlayerState : Base
{
    private MediaSource _source;
    public MediaSource Source
    {
        get => _source;
        set
        {
            _source = value;
            OnPropertyChanged();
        }
    }
    private string _title = "Select Episode";
    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged();
        }
    }
    private Episode currentEpisode = new();
    public Episode CurrentEpisode
    {
        get => currentEpisode;
        set
        {
            currentEpisode = value;
            OnPropertyChanged();
        }
    }
    private string _duration = "00:00:00";
    public string Duration
    {
        get => _duration;
        set
        {
            _duration = value;
            OnPropertyChanged();
        }
    }
    private TimeSpan _position = new();
    public TimeSpan Position
    {
        get => _position;
        set
        {
            _position = value;
            OnPropertyChanged();
        }
    }
    private TimeSpan _seekTo = new();
    public TimeSpan SeekTo
    {
        get => _seekTo;
        set => _seekTo = value;
    }
    
    private string _podcastTitle;
    public string PodcastTitle
    {
        get => _podcastTitle;
        set
        {
            _podcastTitle = value;
            OnPropertyChanged();
        }
    }
    private ObservableCollection<Episode> _playlist = new ObservableCollection<Episode>();
    public ObservableCollection<Episode> Playlist
    {
        get => _playlist;
        set
        {
            _playlist = value;
            OnPropertyChanged();
        }
    }

    private string _playButtonText = "Play";
    public string PlayButtonText
    {
        get => _playButtonText;
        set
        {
            _playButtonText = value;
            OnPropertyChanged();
        }
    }
    private bool _isPlayEnabled = true;
    public bool IsPlayEnabled
    {
        get => _isPlayEnabled;
        set
        {
            _isPlayEnabled = value;
            OnPropertyChanged();
        }
    }
    private bool _isNextEnabled = false;
    public bool IsNextEnabled
    {
        get => _isNextEnabled;
        set
        {
            _isNextEnabled = value;
            OnPropertyChanged();
        }
    }
    private bool _isJumpEnabled = false;
    public bool IsJumpEnabled
    {
        get => _isJumpEnabled;
        set
        {
            _isJumpEnabled = value;
            OnPropertyChanged();
        }
    }
    private bool _isStopEnabled = false;
    public bool IsStopEnabled
    {
        get => _isStopEnabled;
        set
        {
            _isStopEnabled = value;
            OnPropertyChanged();
        }
    }
}

