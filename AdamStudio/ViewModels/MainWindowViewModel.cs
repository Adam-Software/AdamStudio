using AdamStudio.Controls.CustomControls.Services;
using AdamStudio.Controls.Enums;
using AdamStudio.Core;
using AdamStudio.Core.Extensions;
using AdamStudio.Core.Model;
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
        private readonly IStatusBarNotificationDeliveryService mStatusBarNotification;
        private readonly ICommunicationProviderService mCommunicationProviderService;
        private readonly IFolderManagmentService mFolderManagment;
        private readonly IWebApiService mWebApiService;
        private readonly IAvalonEditService mAvalonEditService;
        private readonly IThemeManagerService mThemeManager;
        private readonly ICultureProvider mCultureProvider;

        #endregion

        #region ~

        public MainWindowViewModel(IServiceProvider serviceProvider) 
        {
            mRegionManager = serviceProvider.GetService<IRegionManager>(); 
            mWebApiService = serviceProvider.GetService<IWebApiService>(); 
            RegionChangeAwareService = serviceProvider.GetService<IRegionChangeAwareService>(); 
            mStatusBarNotification = serviceProvider.GetService<IStatusBarNotificationDeliveryService>(); 
            mCommunicationProviderService = serviceProvider.GetService<ICommunicationProviderService>();
            mFolderManagment = serviceProvider.GetService<IFolderManagmentService>(); 
            mAvalonEditService = serviceProvider.GetService<IAvalonEditService>();
            mThemeManager = serviceProvider.GetService<IThemeManagerService>();
            mCultureProvider = serviceProvider.GetService<ICultureProvider>();
            ControlHelper = serviceProvider.GetService<IControlHelper>(); 
            mLoggerService = serviceProvider.GetService<ILogger<MainWindowViewModel>>(); 

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
                    ControlHelper.CurrentBlocklyViewMode = BlocklyViewMode.FullScreen;
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

        private void ParseSyslogMessage(string message)
        {
            try
            {
                SyslogMessageModel syslogMessage = message.Parse();
                mStatusBarNotification.CompileLogMessage = $"{syslogMessage.TimeStamp:T} {syslogMessage.Message}";   
            }
            catch
            {
                // If you couldn't read the message, it's okay, no one needs to know about it.
            }
        }

        /// <summary>
        /// Register highlighting for AvalonEdit. You need to call before loading the regions
        /// </summary>
        private void LoadCustomAvalonEditHighlighting()
        {
            mAvalonEditService.RegisterHighlighting(HighlightingName.AdamPython, Resource.AdamPython);
            mLoggerService.LogInformation("Loaded highlighting");
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
            mCommunicationProviderService.RaiseTcpServiceCientConnectedEvent += RaiseTcpServiceCientConnectedEvent;
            mCommunicationProviderService.RaiseUdpServiceServerReceivedEvent += RaiseUdpServiceServerReceivedEvent;

            Application.Current.MainWindow.Loaded += MainWindowLoaded;
            Application.Current.MainWindow.Closed += MainWindowClosed; 
        }

        /// <summary>
        /// #20
        /// </summary>
        private void Unsubscribe()
        {
            mCommunicationProviderService.RaiseTcpServiceCientConnectedEvent -= RaiseTcpServiceCientConnectedEvent;
            mCommunicationProviderService.RaiseUdpServiceServerReceivedEvent -= RaiseUdpServiceServerReceivedEvent;
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
            
            LoadCustomAvalonEditHighlighting();
            LoadAppTheme();

            if (Settings.Default.AutoStartTcpConnect)
                mCommunicationProviderService.ConnectAllAsync();
        }


        private void MainWindowClosed(object sender, EventArgs e)
        {
            Unsubscribe();
        }

        /// <summary>
        /// It is not clear where to put this, so it will not get lost here.
        /// 
        /// Stops a remotely executed script that may have been executing before the connection was lost.
        /// </summary>
        private void RaiseTcpServiceCientConnectedEvent(object sender)
        {
            mWebApiService.StopPythonExecute();
        }

        private void RaiseUdpServiceServerReceivedEvent(object sender, string message)
        {
            ParseSyslogMessage(message);
        }

        #endregion
    }
}
