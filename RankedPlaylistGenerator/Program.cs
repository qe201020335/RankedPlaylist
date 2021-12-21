using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RankedPlaylistGenerator
{
    internal static class Program
    {
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
	        
	        var generator = new RankedPlaylistGenerator(minStar, maxStar, size);
	        var bplist = await generator.Make();
	        var filename = $"Ranked {minStar}-{maxStar} {DateTimeOffset.Now.ToUnixTimeSeconds()}.bplist";
            
	        Console.WriteLine("\n\n================ Write out bplist ================\n\n");
	        Console.WriteLine(filename);
	        Console.WriteLine($"Size {bplist.Size}");
            
	        File.WriteAllText(filename, JObject.FromObject(bplist).ToString());
	        
	        
            // Console.Beep();
            Console.WriteLine("Done \a");
            Console.WriteLine("Press Enter to Finish...");
            Console.ReadLine();
        }
    }
}