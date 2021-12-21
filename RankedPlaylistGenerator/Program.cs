using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RankedPlaylistGenerator.Models;
using System.Net.Http;

namespace RankedPlaylistGenerator
{
    internal class Program
    {
        private const string _baseURL = "http://scoresaber.com";
        private static readonly HttpClient _client = new HttpClient();

        private float _minStar = 5.5f;
        private float _maxStar = 8.5f;

        private int _maxSize = 30;

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
            
            // _playlist.AddSong(
            //     "Daisuki, Evolution", 
            //     "Foxy Boi & Skeelie", 
            //     "092453D3707C8EBDEC45BE1023AA8AF4F4868234", 
            //     "custom_level_092453D3707C8EBDEC45BE1023AA8AF4F4868234"
            // );
            //
            // _playlist.AddSong(
            //     "Extra Mode", 
            //     "Alice", 
            //     "ACAFECF03095054D7B328EA660161C5269994C1A",     
            //     "custom_level_ACAFECF03095054D7B328EA660161C5269994C1A",
            //     "Standard",
            //     "Expert"
            // );
            //
            // _playlist.AddSong(
            //     "Extra Mode", 
            //     "Alice", 
            //     "ACAFECF03095054D7B328EA660161C5269994C1A",     
            //     "custom_level_ACAFECF03095054D7B328EA660161C5269994C1A",
            //     "Standard",
            //     "ExpertPlus"
            // );
            
            // await Task.Delay(1000);

            int size = 0;
            int page = 1;

            var response = await FetchPage(page);

            while (size <= _maxSize)
            {
                var maps = response?["leaderboards"]?.ToArray();
                if (maps == null || maps.Length == 0)
                {
                    // we have reach the end
                    // or we encountered an error that no maps are returned
                    break;
                }
                Console.WriteLine(maps.Length);
                foreach (var map in maps)
                {
                    // add songs
                    
                    
                    
                    
                    size++;
                }
                // next page
                page++;
                response = await FetchPage(page);
            }

            Console.WriteLine("Fetch Done");

        }
        
        private async Task<JObject> FetchPage(int page)
        {
            try
            {
                return JObject.Parse(await _client.GetStringAsync($"{_baseURL}/api/leaderboards" +
                                                                  $"?ranked=true" +
                                                                  $"&minStar={_minStar}" +
                                                                  $"&maxStar={_maxStar}" +
                                                                  $"&category=3" +
                                                                  $"&sort=0" +
                                                                  $"&page={page}" +
                                                                  $"&withMetadata=true"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
            
        }

        public static void Main(string[] args)
        {
            Program program = new Program();
            program.Make().Wait();
        }
    }
}