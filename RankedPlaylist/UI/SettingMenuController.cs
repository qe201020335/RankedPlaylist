using System;
using BeatSaberMarkupLanguage.Attributes;
using System.IO;
using System.Threading;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using Newtonsoft.Json.Linq;
using RankedPlaylist.Configuration;
using RankedPlaylist.RankedPlaylistGenerator.Events;
using RankedPlaylist.RankedPlaylistGenerator.Models;
using TMPro;
using ErrorEventArgs = RankedPlaylist.RankedPlaylistGenerator.Events.ErrorEventArgs;

namespace RankedPlaylist.UI
{
    [HotReload(RelativePathToLayout = @"BSML\menu.bsml")]
    [ViewDefinition("RankedPlaylist.UI.BSML.menu.bsml")]
    internal class SettingMenuController : BSMLAutomaticViewController
    {
        private RankedPlaylistGenerator.RankedPlaylistGenerator _generator;

        private Thread _worker;

        [UIParams] private BSMLParserParams parserParams;

        [UIValue("max-star")] private float _maxStar = PluginConfig.Instance.MaxStar;

        [UIValue("min-star")] private float _minStar = PluginConfig.Instance.MinStar;

        [UIValue("size")] private int _size = PluginConfig.Instance.Size;

        [UIComponent("info-text")] private TextMeshProUGUI _infoText;

        [UIComponent("info-text2")] private TextMeshProUGUI _infoText2;

        [UIAction("on-generate-click")]
        private void OnGenerateClick()
        {
            SaveValues();
            // _infoText.text = "test text 1";
            // _infoText2.text = "test text long long long long long long long long long long long long long long long long long long long long long long long long";
            if (_worker != null && _worker.IsAlive)
            {
                Logger.logger.Warn("Previous run of RankedPlaylist is still running!");
                return;
            }
            
            Logger.logger.Debug($"{_minStar}, {_maxStar}, {_size}");
            _generator = new RankedPlaylistGenerator.RankedPlaylistGenerator(_minStar, _maxStar, _size);
            _generator.OnSongAdd += OnSongAdd;
            _generator.OnError += OnError;
            
            _worker = new Thread(Generate);
            
            _worker.Start();

        }

        private void SaveValues()
        {
            PluginConfig.Instance.MaxStar = _maxStar;
            PluginConfig.Instance.MinStar = _minStar;
            PluginConfig.Instance.Size = _size;
        }

        private void OnSongAdd(Object sender, SongAddEventArgs eventArgs)
        {

            var text = eventArgs.Difficulty == null
                ? $"{eventArgs.Song.songName} - {eventArgs.Song.levelAuthorName}"
                : $"{eventArgs.Song.songName} - {eventArgs.Song.levelAuthorName} : {eventArgs.Difficulty.name}";

            _infoText2.text = text;

            if (PluginConfig.Instance.DebugLogSpam)
            {
                Logger.logger.Debug(text);
            }

        }
	    
        private void OnError(Object sender, ErrorEventArgs eventArgs)
        {
            _infoText.text = "Error Occured: ";
            _infoText2.text = eventArgs.Exception.Message;
            Logger.logger.Critical("Error Occured while fetching ranked playlist.");
            Logger.logger.Critical(eventArgs.Exception);
        }

        private void OnPlaylistGenerated(Playlist playlist)
        {
            _infoText.text = "Writing Playlist File...";
            _infoText2.text = "Writing Playlist File...";
            
            var bplist = JObject.FromObject(playlist);
            
            // Write to file
            try
            {
                var playlistDirectory = Path.GetFullPath("Playlists");
                Logger.logger.Debug(playlistDirectory);
                
                var path = Path.Combine(playlistDirectory, "__RankedPlaylist_generated.bplist");
                Logger.logger.Info("Writing Playlist");
                Logger.logger.Debug(path);
                File.WriteAllText(path, JObject.FromObject(bplist).ToString());

                _infoText.text = "Playlist Generated!";
                _infoText2.text = $"{playlist.Size} maps in total.";
                Logger.logger.Info($"Playlist generated with {playlist.Size} maps.");
            }
            catch (Exception e)
            {
                _infoText.text = "Error Occured: ";
                _infoText2.text = e.Message;
                Logger.logger.Critical("Error Occured while fetching ranked playlist.");
                Logger.logger.Critical(e);
            }
        }

        private async void Generate()
        {
            Logger.logger.Info("Start Fetching Ranked Songs");
            _infoText.text = "Fetching...";
            var playlist = await _generator.Make();
            OnPlaylistGenerated(playlist);
        } 
    }
}