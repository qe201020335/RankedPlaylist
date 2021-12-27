using System;
using System.Collections.Generic;
using System.IO;
using BeatSaberPlaylistsLib;
using BeatSaberPlaylistsLib.Types;
using RankedPlaylist.RankedPlaylistGenerator.Events;

namespace RankedPlaylist.RankedPlaylistGenerator.Models
{
    public class Playlist
    {
        public int Size => _playlist.Count;
        
        internal event EventHandler<SongAddEventArgs> OnSongAdd;
        
        private PlaylistManager _playlistManager = PlaylistManager.DefaultManager;

        private BeatSaberPlaylistsLib.Types.IPlaylist _playlist;

        internal Playlist(string title, string author, string filename)
        {
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

            IPlaylistSong _song = _playlist.Add(hash, name, null, author);

            if (_song != null)
            {
                var song = new Song(name, author, hash, id);
                OnSongAddBroadcast(song, null);
            }
            
            // if (!_songs.ContainsKey(hash))
            // {
            //     var song =  new Song(name, author, hash, id);
            //     _songs[hash] = song;
            //     OnSongAddBroadcast(song, null);
            // }
        }

        internal void AddSong(string name, string author, string hash, string id, string mode, string difficulty)
        {
            
            IPlaylistSong _song = _playlist.Add(hash, name, null, author);

            if (_song == null)
            {
                return;
            }

            if (_song.Difficulties == null)
            {
                _song.Difficulties = new List<BeatSaberPlaylistsLib.Types.Difficulty>();
            }

            var diffStruct = new BeatSaberPlaylistsLib.Types.Difficulty
            {
                Characteristic = mode,
                Name = difficulty
            };
            _song.Difficulties.Add(diffStruct);
            
            var song = new Song(name, author, hash, id);
            var diff = song.AddDifficulty(mode, difficulty);
            OnSongAddBroadcast(song, diff);

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