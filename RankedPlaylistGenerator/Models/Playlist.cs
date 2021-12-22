using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RankedPlaylistGenerator.Models
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
        private readonly Dictionary<string, Song> _songs = new Dictionary<string, Song>();  // hash: Song

        [JsonIgnore]
        public int Size => _songs.Count;
        
        

        public Playlist(string title, string author)
        {
            playlistAuthor = author;
            playlistTitle = title;
        }
        
        public void AddSong(string name, string author, string hash, string id)
        {
            if (!_songs.ContainsKey(hash))
            {
                _songs[hash] = new Song(name, author, hash, id);
            }
        }

        public void AddSong(string name, string author, string hash, string id, string mode, string difficulty)
        {
            if (_songs.ContainsKey(hash))
            {
                _songs[hash].AddDifficulty(mode, difficulty);
            }
            else
            {
                _songs[hash] = new Song(name, author, hash, id, mode, difficulty);
            }
        }

        private List<Song> GetSongs()
        {
            return new List<Song>(_songs.Values);
        }
    }
    
}