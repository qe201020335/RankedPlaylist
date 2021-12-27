using System;
using System.IO;
using System.Threading.Tasks;
using RankedPlaylist.RankedPlaylistGenerator.Events;
using ErrorEventArgs = RankedPlaylist.RankedPlaylistGenerator.Events.ErrorEventArgs;

namespace RankedPlaylistGenerator
{
    internal static class Program
    {
	    private static void OnSongAdd(Object sender, SongAddEventArgs eventArgs)
	    {
		    Console.WriteLine($"{eventArgs.Song.songName} - {eventArgs.Song.levelAuthorName}");
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

        public static async Task Main(string[] args)
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
	        
	        var filename = $"__RankedPlaylist_generated_{DateTimeOffset.Now.ToUnixTimeSeconds()} {minStar}-{maxStar}";
	        var generator = new RankedPlaylist.RankedPlaylistGenerator.RankedPlaylistGenerator(minStar, maxStar, size, filename);
	        generator.OnError += OnError;
	        generator.OnSongAdd += OnSongAdd;

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