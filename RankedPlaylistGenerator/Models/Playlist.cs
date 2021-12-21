﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace RankedPlaylistGenerator.Models
{
    public class Playlist
    {
        [JsonProperty] public readonly string playlistTitle;

        [JsonProperty] public readonly string playlistAuthor;

        [JsonProperty] public List<Song> songs => GetSongs();

        [JsonProperty] public readonly string image = "";

        private readonly Dictionary<string, Song> _songs = new Dictionary<string, Song>();

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