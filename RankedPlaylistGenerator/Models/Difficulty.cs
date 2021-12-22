using Newtonsoft.Json;

namespace RankedPlaylistGenerator.Models
{
    public class Difficulty
    {
        [JsonProperty("characteristic")]
        public readonly string characteristic;
        
        [JsonProperty("name")]
        public readonly string name;
        
        public Difficulty(string mode, string difficulty)
        {
            characteristic = mode;
            name = difficulty;
        }
    }
}