using System;

namespace RankedPlaylist.RankedPlaylistGenerator.Events
{
    public class ErrorEventArgs : EventArgs
    {
        public Exception Exception { get; internal set; }
    }
}