using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using RankedPlaylist.RankedPlaylistGenerator.Models;

namespace RankedPlaylist.RankedPlaylistGenerator
{
	public class RankedPlaylistGenerator
    {
        private const string _baseURL = "http://scoresaber.com";
        private readonly HttpClient _client = new HttpClient();

        private readonly float _minStar;
        private readonly float _maxStar;

        private readonly int _maxSize;

        private Playlist _playlist;

        public RankedPlaylistGenerator(float minStar, float maxStar, int size)
        {
	        _minStar = minStar;
	        _maxStar = maxStar;
	        _maxSize = size;
        }

        public async Task<Playlist> Make()
        {
	        _playlist = new Playlist($"Ranked {_minStar}-{_maxStar}", "RankedPlaylistGenerator");
	        byte[] imageBytes = null;
	        
	        // Code copied from PlaylistManager and PlaylistsLib
	        using (Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RankedPlaylist.RankedPlaylistGenerator.Assets.PP_Stonks.png"))
	        {
		        if (imageStream == null || !imageStream.CanRead)
			        imageBytes = null;
		        else if (imageStream is MemoryStream cast)
		        {
			        imageBytes = cast.ToArray();
		        }
		        else
		        {
			        using (MemoryStream ms = new MemoryStream())
			        {
				        imageStream.CopyTo(ms);
				        imageBytes = ms.ToArray();
			        }
		        }
	        }

	        if (imageBytes != null)
	        {
		        _playlist.image = Convert.ToBase64String(imageBytes);
	        }
	        
	        /* TODO: Add description
            _playlist.playlistDescription = 
            */
            await Fetch();

            return _playlist;
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
    }
}