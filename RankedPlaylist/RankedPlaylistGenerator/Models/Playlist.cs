using System;
using System.Collections.Generic;
using System.IO;
using BeatSaberPlaylistsLib;
using RankedPlaylist.RankedPlaylistGenerator.Events;

namespace RankedPlaylist.RankedPlaylistGenerator.Models
{
    public class Playlist
    {
        // [JsonProperty("playlistTitle")]
        public readonly string playlistTitle;

        // [JsonProperty("playlistAuthor")]
        public readonly string playlistAuthor;

        // [JsonProperty("songs")]
        // public List<Song> songs => GetSongs();

        // [JsonProperty("image")]
        // public string image = "";
        
        // [JsonProperty("playlistDescription")]
        // public string playlistDescription = "";
        
        // I hate duplicates in playlists, so 
        // [JsonIgnore]
        // private readonly Dictionary<string, Song> _songs = new Dictionary<string, Song>();  // [hash]= Song

        // [JsonIgnore]
        public int Size => _playlist.Count;

        public string FileName
        {
            get => _playlist.Filename;
            set
            {
                _playlist.Filename = value;
            }
        }

        internal event EventHandler<SongAddEventArgs> OnSongAdd;
        
        private PlaylistManager _playlistManager = PlaylistManager.DefaultManager;

        private BeatSaberPlaylistsLib.Types.IPlaylist _playlist;


        internal Playlist(string title, string author, string filename)
        {
            playlistAuthor = author;
            playlistTitle = title;

            _playlist = _playlistManager.GetPlaylist(filename);
            
            if (_playlist == null)
            {
                // Console.WriteLine("no previous");
                _playlist = _playlistManager.CreatePlaylist(filename, title, author, "");
            }
            else
            {
                // clear the existing one for new songs
                _playlist.Clear();
                // Console.WriteLine("previous cleared");
                
                _playlist.Title = title;
                _playlist.Author = author;
            }

            // I hate duplicates in playlists, so 
            _playlist.AllowDuplicates = false;
        }

        internal void SetImage(Stream stream)
        {
            _playlist.SetCover(stream);
        }

        internal void SavePlaylist()
        {
            _playlistManager.StorePlaylist(_playlist);
            // _playlistManager.RequestRefresh("Ranked Playlist");
        }

        internal void AddSong(string name, string author, string hash, string id)
        {
            // if (!_songs.ContainsKey(hash))
            // {
            //     var song =  new Song(name, author, hash, id);
            //     _songs[hash] = song;
            //     OnSongAddBroadcast(song, null);
            // }
        }

        internal void AddSong(string name, string author, string hash, string id, string mode, string difficulty)
        {
            // Song song;
            // if (_songs.ContainsKey(hash))
            // {
            //     song = _songs[hash];
            // }
            // else
            // {
            //     song = new Song(name, author, hash, id);
            //     _songs[hash] = song;
            // }
            //
            // var diff = song.AddDifficulty(mode, difficulty);
            // OnSongAddBroadcast(song, diff);

        }

        private void OnSongAddBroadcast(Song song, Difficulty difficulty)
        {
            var songAddEventArgs = new SongAddEventArgs
            {
                Difficulty = difficulty,
                Song = song
            };
            try
            {
                EventHandler<SongAddEventArgs> handler = OnSongAdd;
                handler?.Invoke(this, songAddEventArgs);
            }
            catch (Exception e)
            {
                // bruh, Exception during event broadcast
                Console.Error.WriteLine(e);
            }
        }

        // private List<Song> GetSongs()
        // {
        //     return new List<Song>(_songs.Values);
        // }
    }
    
}