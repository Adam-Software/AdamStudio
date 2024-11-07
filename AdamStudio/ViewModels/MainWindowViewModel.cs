using AdamStudio.Controls.CustomControls.Services;
using AdamStudio.Controls.Enums;
using AdamStudio.Core;
using AdamStudio.Core.Properties;
using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace AdamStudio.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region DelegateCommands

        public DelegateCommand<string> MoveSplitterDelegateCommand { get; }
        public DelegateCommand SwitchToVideoDelegateCommand { get; }
        public DelegateCommand SwitchToSettingsViewDelegateCommand { get; }

        #endregion

        #region Services

        public IRegionChangeAwareService RegionChangeAwareService { get; }
        public IControlHelper ControlHelper { get; }

        private readonly ILogger<MainWindowViewModel> mLoggerService;
        private readonly IRegionManager mRegionManager;
        private readonly ICommunicationProviderService mCommunicationProviderService;
        private readonly IFolderManagmentService mFolderManagment;
        private readonly IThemeManagerService mThemeManager;
        private readonly ICultureProvider mCultureProvider;
        private readonly IFlyoutManager mFlyoutManager;

        #endregion

        #region ~

        public MainWindowViewModel(IServiceProvider serviceProvider) 
        {
            mLoggerService = serviceProvider.GetService<ILogger<MainWindowViewModel>>();
            mRegionManager = serviceProvider.GetService<IRegionManager>(); 
            RegionChangeAwareService = serviceProvider.GetService<IRegionChangeAwareService>(); 
            mCommunicationProviderService = serviceProvider.GetService<ICommunicationProviderService>();
            mFolderManagment = serviceProvider.GetService<IFolderManagmentService>(); 
            mThemeManager = serviceProvider.GetService<IThemeManagerService>();
            mCultureProvider = serviceProvider.GetService<ICultureProvider>();
            ControlHelper = serviceProvider.GetService<IControlHelper>(); 
            mFlyoutManager = serviceProvider.GetService<IFlyoutManager>();

            MoveSplitterDelegateCommand = new DelegateCommand<string>(MoveSplitter, MoveSplitterCanExecute);
            SwitchToVideoDelegateCommand = new DelegateCommand(SwitchToVideo, SwitchToVideoCanExecute);
            SwitchToSettingsViewDelegateCommand = new DelegateCommand(SwitchToSettingsView, SwitchToSettingsViewCanExecute);            
            Subscribe();
        }


        /*Test*/
        /*private void DeactivateView()
        {
            //IRegion region = mRegionManager.Regions[SubRegionNames.InsideConentRegion];

            ScratchControlView view = _container.Resolve<ScratchControlView>();
            SettingsControlView settings = _container.Resolve<SettingsControlView>();
            
            //bool isActive = region.ActiveViews.FirstOrDefault() != null;

            //var cratch = region.GetView(nameof(ScratchControlView));
            //var settings = region.GetView(nameof(SettingsControlView));

            //object menu = region.Views.ToList();
            // (nameof(ScratchControlView));

            /*if (true)
            {
                region.Deactivate(view);
                region.Activate(settings);
            }
            else
            {
                
            } */           
        //}

        #endregion

        #region Public fields

        public string WindowTitle => $"AdamStudio {Assembly.GetExecutingAssembly().GetName().Version}";

        #endregion

        #region DelegateCommands methods

        private void MoveSplitter(string commandArg)
        {
            BlocklyViewMode currentViewMode = ControlHelper.CurrentBlocklyViewMode;

            if (commandArg == "Left")
            {
                if (currentViewMode == BlocklyViewMode.FullScreen)
                    ControlHelper.CurrentBlocklyViewMode = BlocklyViewMode.MiddleScreen;

                if (currentViewMode == BlocklyViewMode.MiddleScreen)
                    ControlHelper.CurrentBlocklyViewMode = BlocklyViewMode.Hidden;
            }

            if (commandArg == "Right")
            {
                if (currentViewMode == BlocklyViewMode.Hidden)
                    ControlHelper.CurrentBlocklyViewMode = BlocklyViewMode.MiddleScreen;

                if (currentViewMode == BlocklyViewMode.MiddleScreen)
                {
                    ControlHelper.CurrentBlocklyViewMode = BlocklyViewMode.FullScreen;
                    mFlyoutManager.CloseFlyout(FlyoutNames.FlyoutNotification, true);
                }
            }
        }

        private bool MoveSplitterCanExecute(string arg)
        {
            var regionName = RegionChangeAwareService.RegionNavigationTargetName;
            return regionName == ViewNames.ScratchView;
        }

        private void SwitchToVideo()
        {
            if (ControlHelper.IsShowVideo)
            {
                ControlHelper.IsShowVideo = false;
                return;
            }
                
            ControlHelper.IsShowVideo = true;
            return;
        }

        private bool SwitchToVideoCanExecute()
        {
            var regionName = RegionChangeAwareService.RegionNavigationTargetName;
            return regionName == ViewNames.ScratchView;
        }

        private void SwitchToSettingsView()
        {
            var activeViewName = RegionChangeAwareService.RegionNavigationTargetName;
            
            if (activeViewName == ViewNames.ScratchView)
            {
                ShowView(ViewNames.SettingsView);
                return;
            }
            
            if(activeViewName == ViewNames.SettingsView)
            {
                ShowView(ViewNames.ScratchView);
                return;
            }   
        }

        private bool SwitchToSettingsViewCanExecute()
        {
            return true;
        }

        #endregion

        #region Private methods

        private void ShowView(string viewName)
        {
            mRegionManager.RequestNavigate(RegionNames.ContentRegion, viewName);

            MoveSplitterDelegateCommand.RaiseCanExecuteChanged();
            SwitchToVideoDelegateCommand.RaiseCanExecuteChanged(); 
        }

        /// <summary>
        /// starts when the application is first launched
        /// </summary>
        private void SaveFolderPathToSettings()
        {
            if (string.IsNullOrEmpty(Settings.Default.SavedUserWorkspaceFolderPath))
            {
                Settings.Default.SavedUserWorkspaceFolderPath = mFolderManagment.SavedWorkspaceDocumentsDir;
            }

            if (string.IsNullOrEmpty(Settings.Default.SavedUserScriptsFolderPath))
            {
                Settings.Default.SavedUserScriptsFolderPath = mFolderManagment.SavedUserScriptsDocumentsDir;
            }
        }

        private void LoadAppTheme()
        {
            var appThemeName = Settings.Default.AppThemeName;
            mThemeManager.ChangeAppTheme(appThemeName);
        }

        private void LoadDefaultCultureInfo()
        {
            CultureInfo lastLoadLanguage = mCultureProvider.SupportAppCultures.FirstOrDefault(x => x.Name == Settings.Default.AppLanguage);
            lastLoadLanguage ??= mCultureProvider.SupportAppCultures.FirstOrDefault();

            mCultureProvider.ChangeAppCulture(lastLoadLanguage);
        }

        #endregion

        #region Subscriptions

        /// <summary>
        /// #20
        /// </summary>
        private void Subscribe()
        {
            Application.Current.MainWindow.Loaded += MainWindowLoaded;
        }

        #endregion

        #region Event methods
      
        /// <summary>
        /// Load default region at startup
        /// Need to call it after loading the main window
        /// </summary>
        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            LoadDefaultCultureInfo();

            if (Settings.Default.CreateUserDirrectory)
            {
                mFolderManagment.CreateAppDataFolder();
                SaveFolderPathToSettings();
            }

            //Loaded resource 
            ShowView(ViewNames.ScratchView);
            LoadAppTheme();

            if (Settings.Default.AutoStartTcpConnect)
                /// MOVE TO App.xaml
                mCommunicationProviderService.ConnectAllAsync();
        }

        #endregion
    }
}
