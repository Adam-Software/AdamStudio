using AdamBlocklyLibrary.Enum;
using AdamStudio.Controls.CustomControls.Services;
using AdamStudio.Core;
using AdamStudio.Core.Mvvm;
using AdamStudio.Core.Properties;
using AdamStudio.Services.Interfaces;
using ControlzEx.Theming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Windows.Media;

namespace AdamStudio.Modules.ContentRegion.ViewModels
{
    public class SettingsControlViewModel : RegionViewModelBase 
    {
        #region DelegateCommands

        public DelegateCommand ChangeSpacingToggleSwitchDelegateCommand { get; }
        public DelegateCommand OpenPortSettingsDelegateCommand { get; }
        public DelegateCommand OpenWebApiSettingsDelegateCommand { get; }
        public DelegateCommand OpenUserFolderSettingsDelegateCommand { get; }   
        public DelegateCommand FindRobotDelegateCommand { get; }

        #endregion

        #region Services


        private readonly ILogger<SettingsControlViewModel> mLogger;
        private readonly IFlyoutManager mFlyoutManager;
        private readonly IThemeManagerService mThemeManager;
        private readonly ICultureProvider mCultureProvider;
        private readonly IWebViewProvider mWebViewProvider;
        private readonly IRegionChangeAwareService mRegionChangeAwareService;
        private readonly IFindRobotClientService mFindMeClientService;

        #endregion

        #region Const

        private const string cBaseColorAppThemeLightName = "Light";
        private const string cBaseColorAppDarkLightName = "Dark";

        #endregion


        #region ~

        public SettingsControlViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            mLogger = serviceProvider.GetRequiredService<ILogger<SettingsControlViewModel>>();
            mFlyoutManager = serviceProvider.GetService<IFlyoutManager>(); 
            mThemeManager = serviceProvider.GetService<IThemeManagerService>();
            mCultureProvider = serviceProvider.GetService<ICultureProvider>();
            mWebViewProvider = serviceProvider.GetService<IWebViewProvider>();
            mRegionChangeAwareService = serviceProvider.GetService<IRegionChangeAwareService>();
            mFindMeClientService = serviceProvider.GetService<IFindRobotClientService>();

            ChangeSpacingToggleSwitchDelegateCommand = new DelegateCommand(ChangeSpacingToggleSwitch, ChangeSpacingToggleSwitchCanExecute);
            OpenPortSettingsDelegateCommand = new DelegateCommand(OpenPortSettings, OpenPortSettingsCanExecute);
            OpenWebApiSettingsDelegateCommand = new DelegateCommand(OpenWebApiSettings, OpenWebApiSettingsCanExecute);
            OpenUserFolderSettingsDelegateCommand = new DelegateCommand(OpenUserFolderSettings, OpenUserFolderSettingsCanExecute);
            FindRobotDelegateCommand = new DelegateCommand(FindRobot, FindRobotCanExecute);

            Subscribe();
        }

        #endregion

        #region Subscribe/Unsubscribe

        private void Subscribe()
        {
            mFindMeClientService.RaiseFindStartedEvent += RaiseFindStartedEvent;
            mFindMeClientService.RaiseFindEndedEvent += RaiseFindEndedEvent;

        }

        private void Unsubscribe()
        {
            mFindMeClientService.RaiseFindStartedEvent -= RaiseFindStartedEvent;
            mFindMeClientService.RaiseFindEndedEvent -= RaiseFindEndedEvent;
        }

        #endregion

        #region  DelegateCommand methods

        private void ChangeSpacingToggleSwitch()
        {
            Settings.Default.BlocklyGridSpacing = 20;
        }

        private bool ChangeSpacingToggleSwitchCanExecute()
        {
            return true;
        }

        private void OpenPortSettings()
        {
            mFlyoutManager.OpenFlyout(FlyoutNames.FlyoutPortSettings);
        }

        private bool OpenPortSettingsCanExecute()
        {
            return true;
        }

        private void OpenWebApiSettings()
        {
            mFlyoutManager.OpenFlyout(FlyoutNames.FlyoutWebApiSettings);
        }

        private bool OpenWebApiSettingsCanExecute()
        {
            return true;
        }

        private void OpenUserFolderSettings()
        {
            mFlyoutManager.OpenFlyout(FlyoutNames.FlyoutUserFoldersSettings);
        }

        private bool OpenUserFolderSettingsCanExecute()
        {
            return true;
        }

        private void FindRobot()
        {
            mFindMeClientService.SendBroadcastPing(UseLocalServer);
        }

        private bool FindRobotCanExecute()
        {
            return !IsFindingRobotRun;
        }

        #endregion

        #region RaiseEvents methods

        private void RaiseFindStartedEvent(object sender)
        {
            mLogger.LogInformation("Find Robot started");
            IsFindingRobotRun = true;
        }

        private void RaiseFindEndedEvent(object sender, List<IPAddress> findIpAddresses)
        {
            mLogger.LogInformation("Find Robot ended");

            if (findIpAddresses.Count > 0)
            {
                mLogger.LogInformation("The Find Robot service has found such ip addresses");

                foreach (IPAddress ipAddress in findIpAddresses)
                {
                    mLogger.LogInformation("{ipAddress}", ipAddress);
                }
            }
            else
            {
                mLogger.LogInformation("The Find Robot service did not find anything");
            }

            IsFindingRobotRun = false;
        }

        #endregion

        #region Navigation

        public override void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            base.ConfirmNavigationRequest(navigationContext, continuationCallback);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            mRegionChangeAwareService.RegionNavigationTargetName = ViewNames.SettingsView;

            ThemesCollection = mThemeManager.AppThemesCollection;
            SelectedTheme = mThemeManager.GetCurrentAppTheme();

            LanguageApp = mCultureProvider.SupportAppCultures;
            SelectedLanguageApp = mCultureProvider.CurrentAppCulture;

            base.OnNavigatedTo(navigationContext);
        }

        public override void Destroy()
        {
            Unsubscribe();
            base.Destroy();
        }

        #endregion

        #region Public fields

        private bool useLocalServer = false;
        public bool UseLocalServer
        {
            get => useLocalServer;
            set
            {
                SetProperty(ref useLocalServer, value);
            }
        }

        private List<CultureInfo> languageApp;
        public List<CultureInfo> LanguageApp 
        {
            get => languageApp;
            private set => SetProperty(ref languageApp, value); 
        }

        private CultureInfo selectedLanguageApp;
        public CultureInfo SelectedLanguageApp
        {
            get => selectedLanguageApp;
            set
            {
                bool isNewValue = SetProperty(ref selectedLanguageApp, value);

                if (isNewValue)
                {
                    mWebViewProvider.NeedReloadOnLoad = true;
                    ChangeAppLanguage(SelectedLanguageApp);
                }
            }
        }

        public ReadOnlyObservableCollection<Theme> themesCollection;
        public ReadOnlyObservableCollection<Theme> ThemesCollection 
        {
            get => themesCollection;
            set => SetProperty(ref themesCollection, value);
        }

        public Theme selectedTheme;
        public Theme SelectedTheme
        {
            get => selectedTheme;
            set 
            {
                bool isNewValue = SetProperty(ref selectedTheme, value);

                if (isNewValue)
                {
                    mWebViewProvider.NeedReloadOnLoad = true;
                    ChangeTheme(SelectedTheme);
                }
            } 
        }

        private bool mIsFindingRobotRun;
        private bool IsFindingRobotRun
        {
            get => mIsFindingRobotRun;
            set
            {
                bool isNewValue = SetProperty(ref mIsFindingRobotRun, value);

                if (isNewValue)
                    FindRobotDelegateCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region Private methods

        private void ChangeTheme(Theme theme)
        {
            Theme chagedTheme = mThemeManager.ChangeAppTheme(theme);
            ChangeBlockllyTheme(chagedTheme.BaseColorScheme);

            Settings.Default.AppThemeName = chagedTheme.Name;
        }

        private void ChangeBlockllyTheme(string baseColorAppTheme)
        {
            if (baseColorAppTheme == cBaseColorAppDarkLightName)
            {
                Settings.Default.BlocklyTheme = BlocklyTheme.Dark;
                Settings.Default.BlocklyGridColour = Colors.White.ToString();
            }

            if (baseColorAppTheme == cBaseColorAppThemeLightName)
            {
                Settings.Default.BlocklyTheme = BlocklyTheme.Classic;
                Settings.Default.BlocklyGridColour = Colors.Black.ToString();
            }

        }

        private void ChangeAppLanguage(CultureInfo cultureInfo)
        {
            mCultureProvider.ChangeAppCulture(cultureInfo);
            ChangeBlocklyLanguage(cultureInfo);

            Settings.Default.AppLanguage = cultureInfo.Name;
        }

        private void ChangeBlocklyLanguage(CultureInfo cultureInfo)
        {
            if (cultureInfo.TwoLetterISOLanguageName == "en")
            {
                Settings.Default.BlocklyWorkspaceLanguage = BlocklyLanguage.en;
                Settings.Default.BlocklyToolboxLanguage = BlocklyLanguage.en;
            }
                
            if (cultureInfo.TwoLetterISOLanguageName == "ru")
            {
                Settings.Default.BlocklyWorkspaceLanguage = BlocklyLanguage.ru;
                Settings.Default.BlocklyToolboxLanguage = BlocklyLanguage.ru;
            }
        }

        #endregion
    }
}
