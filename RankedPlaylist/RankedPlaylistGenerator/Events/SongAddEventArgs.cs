using System;
using RankedPlaylist.RankedPlaylistGenerator.Models;

namespace RankedPlaylist.RankedPlaylistGenerator.Events
{
    public class SongAddEventArgs : EventArgs
    {
        public Song Song { get; internal set; }
        public Difficulty Difficulty { get; internal set; } = null;
    }
}