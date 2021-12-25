using BeatSaberMarkupLanguage;
using HMUI;
using System;

namespace RankedPlaylist.UI
{
    // Copied from HRCounter which is copied from BS_PP_BOOSTER
    public class ConfigViewFlowCoordinator : FlowCoordinator
    {
        private SettingMenuController _mainPanel;
        public void Awake()
        {
            if (_mainPanel == null)
            {
                _mainPanel = BeatSaberUI.CreateViewController<SettingMenuController>();
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            try
            {
                if (firstActivation)
                {
                    SetTitle("Ranked Playlist");
                    showBackButton = true;
                }

                if (addedToHierarchy)
                {
                    ProvideInitialViewControllers(_mainPanel);
                }
            }
            catch (Exception e)
            {
                Logger.logger.Error(e);
                throw e;
            }
        }
        
        protected override void BackButtonWasPressed(ViewController topController)
        {
            base.BackButtonWasPressed(topController);
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
        }
        
    }
}