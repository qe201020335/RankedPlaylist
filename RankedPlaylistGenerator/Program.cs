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
        private readonly HttpClient _client = new HttpClient();

        private readonly float _minStar;
        private readonly float _maxStar;

        private readonly int _maxSize;

        private readonly Playlist _playlist;

        private Program(float minStar, float maxStar, int size)
        {
	        _minStar = minStar;
	        _maxStar = maxStar;
	        _maxSize = size;
	        _playlist = new Playlist($"Ranked {_minStar}-{_maxStar}", "RankedPlaylistGenerator");
        }

        private async Task Make()
        {
            await Fetch();
            
            // generate the playlist

            string filename = $"Ranked {_minStar}-{_maxStar} {DateTimeOffset.Now.ToUnixTimeSeconds()}.bplist";
            
            Console.WriteLine("\n\n================ Write out bplist ================\n\n");
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
                
                // add the maps in this page
                foreach (var map in maps)
                {
	                
	                if (map == null)
	                {
		                // a null map ??
		                continue;
	                }
	                AddSong(map);
	                size++;
                }
                
                // next page
                page++;
                response = await FetchPage(page);
            }

            Console.WriteLine("Fetch Done");
            Console.WriteLine($"{size} maps/difficulties");
        }

        private void AddSong(JToken map)
        {
	        /* example leaderboard object
            {
				"id": 238821,
				"songHash": "CB24CE05B6D0994586E2D4FBE999B304CF8F9D67",
				"songName": "Galaxy Collapse",
				"songSubName": "",
				"songAuthorName": "Kurokotei",
				"levelAuthorName": "Skeelie",
				"difficulty": {
					"leaderboardId": 238821,
					"difficulty": 7,
					"gameMode": "SoloStandard",
					"difficultyRaw": "_Expert_SoloStandard"
				},
				"maxScore": 3023235,
				"createdDate": "2020-05-11T20:26:24.000Z",
				"rankedDate": "2020-05-16T01:17:23.000Z",
				"qualifiedDate": null,
				"lovedDate": null,
				"ranked": true,
				"qualified": false,
				"loved": false,
				"maxPP": -1,
				"stars": 7.99,
				"plays": 2091,
				"dailyPlays": 0,
				"positiveModifiers": false,
				"playerScore": null,
				"coverImage": "https://cdn.scoresaber.com/covers/CB24CE05B6D0994586E2D4FBE999B304CF8F9D67.png",
				"difficulties": null
			}
			*/
	        
            // first get the basic song info
            var songName = map["songName"]?.ToObject<string>();
            var author = map["songAuthorName"]?.ToObject<string>();
            var hash = map["songHash"]?.ToObject<string>()?.ToUpper();

            if (string.IsNullOrEmpty(hash))
            {
	            Console.WriteLine("Map without hash encountered, skipping \a");
	            // Console.Beep();
                // there is nothing without song hash
                return;
            }
            Console.WriteLine($"{songName} - {author}");
            var id = "custom_level_" + hash;
            
            // Then check the difficult
            var diff = map["difficulty"];
            var mode = diff?["gameMode"]?.ToObject<string>();
            var diffInt = diff?["difficulty"]?.ToObject<int>();
            
            if (!string.IsNullOrEmpty(mode) && mode.Substring(0, 4) == "Solo")
            {
	            mode = mode.Substring(4);
	            
                string difficulty = null;
                switch (diffInt)
                {
                    case 1:
	                    difficulty = "Easy";
	                    break;
                    case 3:
	                    difficulty = "Normal";
	                    break;
                    case 5:
	                    difficulty = "Hard";
	                    break;
                    case 7:
	                    difficulty = "Expert";
	                    break;
                    case 9:
	                    difficulty = "ExpertPlus";
	                    break;
                    default:
	                    difficulty = null;
	                    break;
                }

                if (!string.IsNullOrEmpty(difficulty))
                {
                    _playlist.AddSong(songName, author, hash, id, mode, difficulty);
                    return;
                }
            }
            
            // we can't figure out the difficulty, just don't include it 
			_playlist.AddSong(songName, author, hash, id);
	        
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

        private static bool ScanFloat(out float f, string prompt)
        {
	        Console.Write(prompt);
	        var success = float.TryParse(Console.ReadLine(), out var result);
	        f = result;
	        return success;
        }
        
        private static bool ScanInt(out int i, string prompt)
        {
	        Console.Write(prompt);
	        var success = int.TryParse(Console.ReadLine(), out var result);
	        i = result;
	        return success;
        }

        public static void Main(string[] args)
        {
	        float minStar;
	        float maxStar;
	        int size;
			
	        while (!ScanFloat(out maxStar, "Maximum Star:"))
	        {
		        Console.WriteLine("Invalid input");
	        }

	        while (!ScanFloat(out minStar, "Minimum Star:"))
	        {
		        Console.WriteLine("Invalid input");
	        }
	        
	        while (!ScanInt(out size, "Playlist Size (roughly):"))
	        {
		        Console.WriteLine("Invalid input");
	        }
	        
            Program program = new Program(minStar, maxStar, size);
            program.Make().Wait();
            // Console.Beep();
            Console.WriteLine("Done \a");
            Console.WriteLine("Press Enter to Finish...");
            Console.ReadLine();
        }
    }
}