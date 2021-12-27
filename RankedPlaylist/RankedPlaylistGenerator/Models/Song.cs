using System.Collections.Generic;

namespace RankedPlaylist.RankedPlaylistGenerator.Models
{
    public class Song
    {
        public readonly string songName;

        public readonly string levelAuthorName;

        public readonly string hash;

        public readonly string levelid;

        public readonly List<Difficulty> difficulties = new List<Difficulty>();

        internal Song(string name, string author, string hash, string id)
        {
            songName = name;
            levelAuthorName = author;
            this.hash = hash;
            levelid = id;
        }
        
        internal Song(string name, string author, string hash, string id, string mode, string difficulty)
        {
            songName = name;
            levelAuthorName = author;
            this.hash = hash;
            levelid = id;
            AddDifficulty(mode, difficulty);
        }

        internal Difficulty AddDifficulty(string mode, string difficulty)
        {
            var diff = new Difficulty(mode, difficulty);
            difficulties.Add(diff);
            return diff;
        }
    }
}