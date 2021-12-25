using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RankedPlaylist.RankedPlaylistGenerator.Events;

namespace RankedPlaylist.RankedPlaylistGenerator.Models
{
    public class Playlist
    {
        [JsonProperty("playlistTitle")]
        public readonly string playlistTitle;

        [JsonProperty("playlistAuthor")]
        public readonly string playlistAuthor;

        [JsonProperty("songs")]
        public List<Song> songs => GetSongs();

        [JsonProperty("image")]
        public string image = "";
        
        [JsonProperty("playlistDescription")]
        public string playlistDescription = "";
        
        // I hate duplicates in playlists, so 
        [JsonIgnore]
        private readonly Dictionary<string, Song> _songs = new Dictionary<string, Song>();  // [hash]= Song

        [JsonIgnore]
        public int Size => _songs.Count;
        
        internal event EventHandler<SongAddEventArgs> OnSongAdd;
        
        

        internal Playlist(string title, string author)
        {
            playlistAuthor = author;
            playlistTitle = title;
        }
        
        internal void AddSong(string name, string author, string hash, string id)
        {
            if (!_songs.ContainsKey(hash))
            {
                var song =  new Song(name, author, hash, id);
                _songs[hash] = song;
                OnSongAddBroadcast(song, null);
            }
        }

        internal void AddSong(string name, string author, string hash, string id, string mode, string difficulty)
        {
            Song song;
            if (_songs.ContainsKey(hash))
            {
                song = _songs[hash];
            }
            else
            {
                song = new Song(name, author, hash, id);
                _songs[hash] = song;
            }

            var diff = song.AddDifficulty(mode, difficulty);
            OnSongAddBroadcast(song, diff);

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

        private List<Song> GetSongs()
        {
            return new List<Song>(_songs.Values);
        }
    }
    
}