namespace RankedPlaylist.RankedPlaylistGenerator.Models
{
    public class Difficulty
    {
        public readonly string characteristic;
        
        public readonly string name;
        
        internal Difficulty(string mode, string difficulty)
        {
            characteristic = mode;
            name = difficulty;
        }
    }
}