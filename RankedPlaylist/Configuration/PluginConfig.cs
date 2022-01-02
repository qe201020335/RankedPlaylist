using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using RankedPlaylist.RankedPlaylistGenerator.Utils;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace RankedPlaylist.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        // public virtual int IntValue { get; set; } = 42; // Must be 'virtual' if you want BSIPA to detect a value change and save the config automatically.

        public virtual float MaxStar { get; set; } = 7;

        public virtual float MinStar { get; set; } = 5;

        public virtual int Size { get; set; } = 50;

        public virtual Filter Mode { get; set; } = Filter.Both;

        public virtual bool FilterByAcc { get; set; } = true;

        public virtual float TargetAcc { get; set; } = 92f;

        public virtual bool DebugLogSpam { get; set; } = false;
        
        /*
        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        public virtual void OnReload()
        {
            // Do stuff after config is read from disk.
        }

        /// <summary>
        /// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        /// </summary>
        public virtual void Changed()
        {
            // Do stuff when the config is changed.
        }

        /// <summary>
        /// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
        /// </summary>
        public virtual void CopyFrom(PluginConfig other)
        {
            // This instance's members populated from other
        }
        */
    }
}
