using System.Collections.Generic;
using Newtonsoft.Json;

namespace RankedPlaylist.RankedPlaylistGenerator.Models
{
    public class Song
    {
        [JsonProperty("songName")]
        public readonly string songName;

        [JsonProperty("levelAuthorName")]
        public readonly string levelAuthorName;

        [JsonProperty("hash")]
        public readonly string hash;

        [JsonProperty("levelid")]
        public readonly string levelid;

        [JsonProperty("difficulties")]
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