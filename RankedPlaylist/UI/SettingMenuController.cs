using BeatSaberMarkupLanguage.Attributes;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using TMPro;

namespace RankedPlaylist.UI
{
    internal class SettingMenuController : BSMLResourceViewController
    {
        public override string ResourceName => "RankedPlaylist.UI.BSML.menu.bsml";

        private int count = 0;
        
        [UIParams]
        private BSMLParserParams parserParams;

        [UIComponent("test-text")]
        private TextMeshProUGUI testText;

        [UIAction("on-click")]
        private void OnClick()
        {
            count++;
            testText.text = $"Clicked! {count}";
        }

    }
}