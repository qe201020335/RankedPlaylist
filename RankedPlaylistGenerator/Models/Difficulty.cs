using Newtonsoft.Json;

namespace RankedPlaylistGenerator.Models
{
    public class Difficulty
    {
        [JsonProperty]
        public readonly string characteristic;
        
        [JsonProperty]
        public readonly string name;
        
        public Difficulty(string mode, string difficulty)
        {
            characteristic = mode;
            name = difficulty;
        }
    }
}