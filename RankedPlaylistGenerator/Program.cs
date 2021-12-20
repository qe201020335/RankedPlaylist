using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RankedPlaylistGenerator.Models;

namespace RankedPlaylistGenerator
{
    internal class Program
    {

        private float _minStar = 5.5f;
        private float _maxStar = 8.5f;

        private int _maxSize = 500;

        private List<string> _maps = new List<string>();

        private Playlist _playlist = new Playlist("test title", "test author");

        private async Task Make()
        {
            await Fetch();
            
            // generate the playlist

            string filename = $"Ranked {_minStar}-{_maxStar} {DateTimeOffset.Now.ToUnixTimeSeconds()}.bplist";
            
            Console.WriteLine(filename);

            JObject bplist = JObject.FromObject(_playlist);
            
            File.WriteAllText(filename, bplist.ToString());
            
        }

        private async Task Fetch()
        {
            Console.WriteLine("Fetching...");
            
            _playlist.AddSong(
                "Daisuki, Evolution", 
                "Foxy Boi & Skeelie", 
                "092453D3707C8EBDEC45BE1023AA8AF4F4868234", 
                "custom_level_092453D3707C8EBDEC45BE1023AA8AF4F4868234"
            );
            
            _playlist.AddSong(
                "Extra Mode", 
                "Alice", 
                "ACAFECF03095054D7B328EA660161C5269994C1A",     
                "custom_level_ACAFECF03095054D7B328EA660161C5269994C1A",
                "Standard",
                "Expert"
            );
            
            _playlist.AddSong(
                "Extra Mode", 
                "Alice", 
                "ACAFECF03095054D7B328EA660161C5269994C1A",     
                "custom_level_ACAFECF03095054D7B328EA660161C5269994C1A",
                "Standard",
                "ExpertPlus"
            );
            
            await Task.Delay(1000);
            Console.WriteLine("Fetch Done");

        }

        public static void Main(string[] args)
        {
            Program program = new Program();
            program.Make().Wait();
        }
    }
}