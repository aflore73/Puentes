using System.Diagnostics;
using VoiceAssistant.Core.Interfaces;

namespace VoiceAssistant.Core.Services;

public class MusicService
{
    private Process? _currentProcess;
    private readonly List<string> _musicFolders = new();
    private readonly List<string> _playlist = new();
    private int _currentIndex = 0;
    private bool _isPlaying = false;
    private readonly Random _random = new();
    private List<string> _allSongs = new();

    public MusicService(string? musicFolder = null)
    {
        if (!string.IsNullOrEmpty(musicFolder))
        {
            _musicFolders.Add(musicFolder);
        }
        
        var commonFolders = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Musica"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MÃºsica"),
            @"C:\Musica",
            @"C:\MÃºsica",
            @"C:\Music"
        };

        foreach (var folder in commonFolders)
        {
            if (Directory.Exists(folder) && !_musicFolders.Contains(folder))
            {
                _musicFolders.Add(folder);
            }
        }
    }

    public List<string> GetMusicFolders()
    {
        return _musicFolders;
    }

    public async Task<List<string>> GetAvailableSongsAsync()
    {
        var songs = new List<string>();
        var extensions = new[] { "*.mp3", "*.wav", "*.m4a", "*.flac" };

        foreach (var folder in _musicFolders)
        {
            try
            {
                foreach (var ext in extensions)
                {
                    songs.AddRange(Directory.GetFiles(folder, ext, SearchOption.AllDirectories));
                }
            }
            catch { }
        }

        _allSongs = songs.Distinct().ToList();
        return await Task.FromResult(_allSongs);
    }

    public async Task<List<string>> SearchSongsAsync(string searchTerm)
    {
        var songs = await GetAvailableSongsAsync();
        
        if (string.IsNullOrEmpty(searchTerm))
        {
            return songs;
        }
        
        return songs
            .Where(s => Path.GetFileNameWithoutExtension(s).ToLower().Contains(searchTerm.ToLower()))
            .ToList();
    }

    public async Task<string> PlayMusicAsync(string? songName = null)
    {
        var songs = await GetAvailableSongsAsync();
        
        if (songs.Count == 0)
        {
            return "No encontre musica en: " + string.Join(", ", _musicFolders);
        }

        var filteredSongs = songs;
        
        if (!string.IsNullOrEmpty(songName))
        {
            filteredSongs = songs
                .Where(s => Path.GetFileNameWithoutExtension(s).ToLower().Contains(songName.ToLower()))
                .ToList();
        }

        if (filteredSongs.Count == 0)
        {
            return "No encontre '" + songName + "'. Disponible:\n" + 
                string.Join("\n", songs.Take(10).Select(s => "- " + Path.GetFileNameWithoutExtension(s)));
        }

        _playlist.Clear();
        _playlist.AddRange(filteredSongs.OrderBy(x => _random.Next()));
        _currentIndex = 0;

        return await PlayCurrentAsync();
    }

    private async Task<string> PlayCurrentAsync()
    {
        if (_playlist.Count == 0)
        {
            return "No hay musica en la lista.";
        }

        StopMusic();

        var songPath = _playlist[_currentIndex];
        var songName = Path.GetFileNameWithoutExtension(songPath);

        try
        {
            _currentProcess = new Process();
            _currentProcess.StartInfo = new ProcessStartInfo
            {
                FileName = songPath,
                UseShellExecute = true
            };
            _currentProcess.Start();
            _isPlaying = true;
        }
        catch (Exception ex)
        {
            return "Error al reproducir: " + ex.Message;
        }

        return await Task.FromResult("Reproduciendo: " + songName);
    }

    public string StopMusic()
    {
        try
        {
            if (_currentProcess != null)
            {
                if (!_currentProcess.HasExited)
                {
                    _currentProcess.Kill();
                }
                _currentProcess.Dispose();
                _currentProcess = null;
            }
        }
        catch { }
        
        _isPlaying = false;
        return "Musica detenida.";
    }

    public async Task<string> NextSongAsync()
    {
        if (_playlist.Count == 0)
        {
            var songs = await GetAvailableSongsAsync();
            if (songs.Count == 0) return "No hay musica.";
            _playlist.AddRange(songs.OrderBy(x => _random.Next()));
        }

        _currentIndex = (_currentIndex + 1) % _playlist.Count;
        return await PlayCurrentAsync();
    }
}