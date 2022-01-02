using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Reflection;
using RankedPlaylist.RankedPlaylistGenerator.Models;
using ErrorEventArgs = RankedPlaylist.RankedPlaylistGenerator.Events.ErrorEventArgs;
using RankedPlaylist.RankedPlaylistGenerator.Events;
using RankedPlaylist.RankedPlaylistGenerator.Utils;

namespace RankedPlaylist.RankedPlaylistGenerator
{
	public class RankedPlaylistGenerator
    {
	    private const string _baseURL = "https://scoresaber.com";
        private readonly HttpClient _client = new HttpClient();

        private readonly float _minStar;
        private readonly float _maxStar;

        private readonly int _maxSize;
        private readonly string _filename;

        public Filter Filter { get; private set; } = Filter.Both; // default to both unplayed and played song

        public bool FilterByAcc { get; private set; } = false;
        public double TargetAcc { get; private set; } = 1;

        public string PlayerID { get; private set; } = "";
        
		// save what the player has played for above filters
        private Dictionary<long, double> _playerHistory = new Dictionary<long, double>();  // [leaderboardID]=acc



        private Playlist _playlist;

        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<SongAddEventArgs> OnSongAdd;

        public RankedPlaylistGenerator(float minStar, float maxStar, int size, string filename)
        {
	        _minStar = minStar;
	        _maxStar = maxStar;
	        _maxSize = size;
	        _filename = filename;
        }

        // haha a setter method with all arguments optional
        public void SetFilters(Filter filter = Filter.Both, string playerID = "", bool byAcc = false, double targetAcc = 1)
        {
	        this.Filter = filter;
	        this.PlayerID = playerID;
	        this.FilterByAcc = byAcc;
	        this.TargetAcc = targetAcc;
        }

        public async Task<int> Make()
        {
	        _playlist = new Playlist($"Ranked {_minStar}-{_maxStar}", "RankedPlaylistGenerator", _filename);
	        _playlist.OnSongAdd += OnSongAddBroadcastPassThrough;

	        // Code copied from PlaylistManager and PlaylistsLib
	        using (Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RankedPlaylist.RankedPlaylistGenerator.Assets.PP_Stonks.png"))
	        {
		        if (imageStream != null)
		        {
			        _playlist.SetImage(imageStream);
		        }
	        }

	        /* TODO: Add description
            _playlist.SetDescription("")
            */

	        if (Filter == Filter.PlayedOnly)
	        {
		        await FetchPlayerInfo(true);
	        }
	        else
	        {
		        if (Filter != Filter.Both || FilterByAcc)
		        {
			        // we need the player data in this case
			        await FetchPlayerInfo(false);
		        }
		        await FetchLeaderboards();
	        }
	        _playlist.SavePlaylist();
            return _playlist.Size;
        }

        private async Task FetchPlayerInfo(bool generateFromPlayed)
        {
	        int page = 1;
	        int size = 0;
	        
	        var response = await FetchPlayerInfoPage(page);

	        while (true)  // pepega while true :D
	        {
		        var playerScores = response?["playerScores"]?.ToArray();
		        if (playerScores == null || playerScores.Length == 0)
		        {
			        // we have reached the end
			        // or we encountered an error that no scores are returned
			        break;
		        }
		        
		        /* useful info in a playerScore object
		         {
				    "score": {
				        "baseScore": 920665,
				        "modifiedScore": 920665,
					},
					"leaderboard": {
						"id": 277080,
					    "maxScore": 985435,
					    "ranked": true,
					}
				}*/

		        // process all the levels that this player has played
		        foreach (var playerScore in playerScores)
		        {
			        if (playerScore["leaderboard"]?["ranked"]?.ToObject<bool>() == false)
			        {
				        return; // we have finished processing all the ranked scores
			        }
					
			        var leaderboardID = playerScore["leaderboard"]?["id"]?.ToObject<long>();
			        var maxScore = playerScore["leaderboard"]?["maxScore"]?.ToObject<long>();
			        var baseScore = playerScore["score"]?["baseScore"]?.ToObject<long>();

			        if (leaderboardID == null)
			        {
				        continue;
			        }

			        var acc = (maxScore == null || baseScore == null) ? 0 : baseScore.Value / (double)maxScore.Value;
			        _playerHistory[leaderboardID.Value] = acc;

			        if (generateFromPlayed)
			        {
				        if (!(FilterByAcc && acc > TargetAcc))
				        {
					        // if leaderboardID != null, playerScore["leaderboard"] must be not null
					        AddSong(playerScore["leaderboard"]);
					        
					        size++;
					        if (size >= _maxSize)
					        {
						        return;
					        }
				        }
			        }
		        }
		        
		        // next page
		        page++;
		        response = await FetchPlayerInfoPage(page);
	        }
	        
        }
        
        private async Task<JObject> FetchPlayerInfoPage(int page)
        {
	        try
	        {
		        return JObject.Parse(await _client.GetStringAsync($"{_baseURL}/api/player" +
		                                                          $"/{PlayerID}/scores" +
		                                                          $"?sort=top" +
		                                                          $"&page={page}" +
		                                                          $"&withMetadata=true"));
	        }
	        catch (Exception e)
	        {
		        Console.Error.WriteLine(e);
		        OnErrorBroadcast(e);
		        return null;
	        }
        }

        private async Task FetchLeaderboards()
        {
            // Console.WriteLine("Fetching...");
            
            int size = 0;
            int page = 1;

            var response = await FetchLeaderboardsPage(page);

            while (size <= _maxSize)
            {
                var maps = response?["leaderboards"]?.ToArray();
                if (maps == null || maps.Length == 0)
                {
                    // we have reached the end
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
                response = await FetchLeaderboardsPage(page);
            }

            // Console.WriteLine("Fetch Done");
            // Console.WriteLine($"{size} maps/difficulties");
        }

        private bool CheckFilter(long leaderboardID)
        {
	        switch (Filter)
	        {
		        case Filter.UnplayedOnly:
			        if (_playerHistory.ContainsKey(leaderboardID))
			        {
				        return true;
			        }
			        break;
		        
		        case Filter.PlayedOnly:
			        if (!_playerHistory.ContainsKey(leaderboardID))
			        {
				        return true;
			        }
			        break;
		        
		        case Filter.Both:
		        // do nothing
		        default:
			        break;
	        }

	        var filteredByAcc = FilterByAcc
	                            && Filter != Filter.UnplayedOnly
	                            && _playerHistory.ContainsKey(leaderboardID)
	                            && _playerHistory[leaderboardID] > TargetAcc;

	        return filteredByAcc;
        }

        private void AddSong(JToken map)
        {
	        /* useful info in a leaderboard object
            {
				"id": 38448,
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
				"stars": 7.53
			}
			*/

	        var star = map["stars"]?.ToObject<float>();
	        if (star == null || star > _maxSize || star < _minStar)
	        {
		        // basic star range check
		        return;
	        }

	        var leaderboardID = map["id"]?.ToObject<long>();

	        if (leaderboardID == null)
	        {
		        return; // no leaderboard id? something must've been wrong
	        }

	        // check filters and decided whether to add this map

	        if (CheckFilter(leaderboardID.Value))
	        {
		        return;
	        }

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

            // Console.WriteLine($"{songName} - {author}");
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
        
        private async Task<JObject> FetchLeaderboardsPage(int page)
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
	            Console.Error.WriteLine(e);
	            OnErrorBroadcast(e);
                return null;
            }
        }
        
        private void OnSongAddBroadcastPassThrough(Object sender, SongAddEventArgs eventArgs)
        {
	        try
	        {
		        EventHandler<SongAddEventArgs> handler = OnSongAdd;
		        handler?.Invoke(this, eventArgs);
	        }
	        catch (Exception e)
	        {
		        // Exception during event broadcast
		        Console.Error.WriteLine(e);
	        }
        }
        
        private void OnErrorBroadcast(Exception exception)
        {
	        var eventArgs = new ErrorEventArgs
	        {
		        Exception = exception
	        };
	        try
	        {
		        EventHandler<ErrorEventArgs> handler = OnError;
		        handler?.Invoke(this, eventArgs);
	        }
	        catch (Exception e)
	        {
		        // bruh, Really?
		        Console.Error.WriteLine(e);
	        }
        }

    }
}