using System;
using System.IO;
using System.Threading.Tasks;
using RankedPlaylist.RankedPlaylistGenerator.Events;
using RankedPlaylist.RankedPlaylistGenerator.Utils;
using ErrorEventArgs = RankedPlaylist.RankedPlaylistGenerator.Events.ErrorEventArgs;

namespace RankedPlaylistGenerator
{
    internal static class Program
    {
	    private static void OnSongAdd(Object sender, SongAddEventArgs eventArgs)
	    {
		    var text = eventArgs.Difficulty == null
			    ? $"{eventArgs.Song.songName} - {eventArgs.Song.levelAuthorName}"
			    : $"{eventArgs.Song.songName} - {eventArgs.Song.levelAuthorName} : {eventArgs.Difficulty.name}";
		    Console.WriteLine(text);
	    }
	    
	    private static void OnError(Object sender, ErrorEventArgs eventArgs)
	    {
		    Console.Error.WriteLine(eventArgs.Exception);
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
        
        private static bool ScanBool(out bool b, string prompt)
        {
	        Console.Write(prompt);
	        var result = Console.ReadLine();
	        if (result == null)
	        {
		        b = false;
		        return true;
	        }

	        if (result.ToLower() == "y")
	        {
		        b = true;
		        return true;
	        }
	        if (result.ToLower() == "n")
	        {
		        b = false;
		        return true;
	        }

	        b = false;
	        return false;
        }

        public static async Task Main(string[] args)
        {
	        float minStar;
	        float maxStar;
	        int size;

	        Filter filter;
	        bool byAcc = false;
	        float acc = 1;
	        string id = "";
	        

	        while (!ScanFloat(out maxStar, "Maximum Star: "))
	        {
		        Console.WriteLine("Invalid input");
	        }

	        while (!ScanFloat(out minStar, "Minimum Star: "))
	        {
		        Console.WriteLine("Invalid input");
	        }

	        bool onlyUnplayed = false;
	        bool onlyPlayed = false;

	        while (!ScanBool(out onlyUnplayed, "Only include maps you have not played? (y/n) "))
	        {
		        Console.WriteLine("Invalid input");
	        }
	        
	        if (!onlyUnplayed)
	        {
		        while (!ScanBool(out onlyPlayed, "Only include maps you have played? (y/n) "))
		        {
			        Console.WriteLine("Invalid input");
		        }
	        }

	        if (onlyUnplayed)
	        {
		        filter = Filter.UnplayedOnly;
	        }
	        else if (onlyPlayed)
	        {
		        filter = Filter.PlayedOnly;
	        }
	        else
	        {
		        filter = Filter.Both;
	        }

	        if (filter != Filter.UnplayedOnly)
	        {
		        while (!ScanFloat(out acc, "Target accuracy on played maps (Enter a negative number to include all): "))
		        {
			        Console.WriteLine("Invalid input");
		        }
		        
		        byAcc = !(acc < 0);

		        if (acc > 2f)
		        {
			        // there is probably no way of getting a acc of 200% (with all the positive modifiers)
			        // we want to use decimal instead of percentage. i.e, 0.95 instead of 95(%)
			        acc /= 100;
		        }
	        }

	        if (filter != Filter.Both || byAcc)
	        {
		        Console.Write("Your ScoreSaber profile ID: ");
		        id = Console.ReadLine();
	        }
	        
	        while (!ScanInt(out size, "How Many?: "))
	        {
		        Console.WriteLine("Invalid input");
	        }
	        
	        
	        var filename = $"__RankedPlaylist_generated_{DateTimeOffset.Now.ToUnixTimeSeconds()} {minStar}-{maxStar}";
	        var generator = new RankedPlaylist.RankedPlaylistGenerator.RankedPlaylistGenerator(minStar, maxStar, size, filename);
	        generator.OnError += OnError;
	        generator.OnSongAdd += OnSongAdd;
	        
	        generator.SetFilters(filter, id, byAcc, acc);

	        var count = await generator.Make();
	        
            
	        Console.WriteLine("\n\n================ Write out bplist ================\n\n");
	        
	        Console.WriteLine($"Size {count}");
	        Console.WriteLine(filename);

	        // Console.Beep();
            Console.WriteLine("Done \a");
            Console.WriteLine("Press Enter to Finish...");
            Console.ReadLine();
        }
    }
}