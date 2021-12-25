using Newtonsoft.Json;

namespace RankedPlaylist.RankedPlaylistGenerator.Models
{
    public class Difficulty
    {
        [JsonProperty("characteristic")]
        public readonly string characteristic;
        
        [JsonProperty("name")]
        public readonly string name;
        
        internal Difficulty(string mode, string difficulty)
        {
            characteristic = mode;
            name = difficulty;
        }
    }
}