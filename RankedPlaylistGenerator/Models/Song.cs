﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace RankedPlaylistGenerator.Models
{
    public class Song
    {
        [JsonProperty]
        public readonly string songName;

        [JsonProperty]
        public readonly string levelAuthorName;

        [JsonProperty]
        public readonly string hash;

        [JsonProperty]
        public readonly string levelid;

        [JsonProperty]
        public readonly List<Difficulty> difficulties = new List<Difficulty>();

        public Song(string name, string author, string hash, string id)
        {
            songName = name;
            levelAuthorName = author;
            this.hash = hash;
            levelid = id;
        }
        
        public Song(string name, string author, string hash, string id, string mode, string difficulty)
        {
            songName = name;
            levelAuthorName = author;
            this.hash = hash;
            levelid = id;
            AddDifficulty(mode, difficulty);
        }

        public void AddDifficulty(string mode, string difficulty)
        {
            difficulties.Add(new Difficulty(mode, difficulty));
        }
    }
}