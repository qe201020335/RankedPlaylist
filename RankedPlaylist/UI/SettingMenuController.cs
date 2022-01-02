using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using BS_Utils.Gameplay;
using RankedPlaylist.Configuration;
using RankedPlaylist.RankedPlaylistGenerator.Events;
using RankedPlaylist.RankedPlaylistGenerator.Models;
using RankedPlaylist.RankedPlaylistGenerator.Utils;
using TMPro;
using ErrorEventArgs = RankedPlaylist.RankedPlaylistGenerator.Events.ErrorEventArgs;

namespace RankedPlaylist.UI
{
    [HotReload(RelativePathToLayout = @"BSML\menu.bsml")]
    [ViewDefinition("RankedPlaylist.UI.BSML.menu.bsml")]
    internal class SettingMenuController : BSMLAutomaticViewController
    {
        private RankedPlaylistGenerator.RankedPlaylistGenerator _generator;

        private bool _running = false;

        [UIParams] private BSMLParserParams parserParams;

        [UIValue("max-star")] private float _maxStar = PluginConfig.Instance.MaxStar;

        [UIValue("min-star")] private float _minStar = PluginConfig.Instance.MinStar;

        [UIValue("size")] private int _size = PluginConfig.Instance.Size;

        [UIComponent("info-text")] private TextMeshProUGUI _infoText;

        [UIComponent("info-text2")] private TextMeshProUGUI _infoText2;


        [UIValue("list-options")]
        private List<object> _filterOptions = new object[] { Filter.UnplayedOnly, Filter.PlayedOnly, Filter.Both }.ToList();

        [UIValue("list-choice")] private Filter _filterChoice = PluginConfig.Instance.Mode;

        [UIValue("by-acc")] private bool _byAcc = PluginConfig.Instance.FilterByAcc;

        [UIValue("target-acc")] private float _targetAcc = PluginConfig.Instance.TargetAcc;

        
        [UIAction("on-generate-click")]
        private async void OnGenerateClick()
        {
            SaveValues();
            // _infoText.text = "test text 1";
            // _infoText2.text = "test text long long long long long long long long long long long long long long long long long long long long long long long long";
            
            await Generate();
            
            Logger.logger.Debug("Done");

        }

        private void SaveValues()
        {
            PluginConfig.Instance.MaxStar = _maxStar;
            PluginConfig.Instance.MinStar = _minStar;
            PluginConfig.Instance.Size = _size;

            PluginConfig.Instance.Mode = _filterChoice;
            PluginConfig.Instance.FilterByAcc = _byAcc;
            PluginConfig.Instance.TargetAcc = _targetAcc;
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

        private async Task Generate()
        {
            if (_running)
            {
                Logger.logger.Warn("Previous run of RankedPlaylist is still running!");
                return;
            }
            _running = true;
            var userInfo = await GetUserInfo.GetUserAsync();
            Logger.logger.Debug($"{_minStar}, {_maxStar}, {_size}, {_filterChoice}, {userInfo.platformUserId}, {_byAcc}, {_targetAcc}");
            _generator = new RankedPlaylistGenerator.RankedPlaylistGenerator(_minStar, _maxStar, _size, "__RankedPlaylist_generated");
            _generator.OnSongAdd += OnSongAdd;
            _generator.OnError += OnError;
            
            _generator.SetFilters(_filterChoice, userInfo.platformUserId, _byAcc, _targetAcc / 100);
            
            Logger.logger.Info("Start Fetching Ranked Songs");
            _infoText.text = "Fetching...";
            _infoText2.text = "Please be patient.";
            try
            {
                var count = await _generator.Make();
                _infoText.text = "Playlist Generated!";
                _infoText2.text = $"{count} maps in total.";
                Logger.logger.Info($"Playlist generated with {count} maps.");
            }
            catch (Exception e)
            {
                _infoText.text = "Error Occured: ";
                _infoText2.text = e.Message;
                Logger.logger.Critical("Error Occured while fetching ranked playlist.");
                Logger.logger.Critical(e);
            }
            finally
            {
                _running = false;
            }
        }

        // [UIAction("test-button")]
        // private void TestButtonPress()
        // {
        //     Logger.logger.Debug("Test Test");
        // }
    }
}