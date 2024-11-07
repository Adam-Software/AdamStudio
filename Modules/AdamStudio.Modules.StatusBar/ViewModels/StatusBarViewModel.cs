using AdamStudio.Controls.CustomControls.Services;
using AdamStudio.Controls.Enums;
using AdamStudio.Core;
using AdamStudio.Core.Mvvm;
using AdamStudio.Services.Interfaces;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Regions;
using System;

namespace AdamStudio.Modules.StatusBarRegion.ViewModels
{
    public class StatusBarViewModel : RegionViewModelBase
    {
        #region DelegateCommands

        public DelegateCommand OpenNotificationPanelDelegateCommand { get; }

        #endregion

        #region Services

        private readonly IFlyoutManager mFlyoutManager;
        private readonly ICommunicationProviderService mCommunicationProviderService;
        private readonly IStatusBarNotificationDeliveryService mStatusBarNotificationDelivery;
        private readonly IFlyoutStateChecker mFlyoutState;
        private readonly ICultureProvider mCultureProvider;
        private readonly IControlHelper mControlHelper;
        private readonly ILogWriteEventAwareService mLogWriteEventAwareService;
        private readonly ILogger<StatusBarViewModel> mLogger;

        #endregion

        #region Var

        private string mTextOnStatusConnectToolbarDisconnected;
        private string mTextOnStatusConnectToolbarConnected;
        private string mTextOnStatusConnectToolbarReconnected;
        private string mAppLogStatusBar;
        private string mChangAppLanguageLogMessage;

        #endregion

        #region ~

        public StatusBarViewModel(IServiceProvider serviceProvider) : base(serviceProvider)        
        {
            mLogger = serviceProvider.GetService<ILogger<StatusBarViewModel>>();
            mFlyoutManager = serviceProvider.GetService<IFlyoutManager>(); 
            mCommunicationProviderService = serviceProvider.GetService<ICommunicationProviderService>(); 
            mStatusBarNotificationDelivery = serviceProvider.GetService<IStatusBarNotificationDeliveryService>(); 
            mFlyoutState = serviceProvider.GetService<IFlyoutStateChecker>(); 
            mCultureProvider = serviceProvider.GetService<ICultureProvider>();
            mControlHelper = serviceProvider.GetService<IControlHelper>();
            mLogWriteEventAwareService = serviceProvider.GetService<ILogWriteEventAwareService>();
          
            OpenNotificationPanelDelegateCommand = new DelegateCommand(OpenNotificationPanel, OpenNotificationPanelCanExecute);

            LoadDefaultFieldValue();
        }

        #endregion

        #region DelegateCommands methods

        private void OpenNotificationPanel()
        {
            if(mControlHelper.CurrentBlocklyViewMode == BlocklyViewMode.FullScreen)
            {
                mControlHelper.CurrentBlocklyViewMode = BlocklyViewMode.MiddleScreen;
            }

            mFlyoutManager.OpenFlyout(FlyoutNames.FlyoutNotification);

        }

        private bool OpenNotificationPanelCanExecute()
        {
            return !mFlyoutState.IsFlyoutsOpened;
        }

        #endregion

        #region Navigation

        public override void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            base.ConfirmNavigationRequest(navigationContext, continuationCallback);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Subscribe();

            base.OnNavigatedTo(navigationContext);
        }

        public override void Destroy()
        {
            Unsubscribe();
        }

        #endregion

        #region Public fields

        private bool progressRingStart;
        public bool ProgressRingStart
        {
            get { return progressRingStart; }
            set { SetProperty(ref progressRingStart, value); }
        }

        private string appLogStatusBar; 
        public string AppLogStatusBar
        {
            get { return appLogStatusBar; }
            set { SetProperty(ref appLogStatusBar, value); }
        }

        private PackIconModernKind connectIcon;
        public PackIconModernKind ConnectIcon
        {
            get { return connectIcon; }
            set { SetProperty(ref connectIcon, value); }
        }

        private string textOnStatusConnectToolbar;
        public string TextOnStatusConnectToolbar
        {
            get { return textOnStatusConnectToolbar; }
            set { SetProperty(ref textOnStatusConnectToolbar, value); }
        }

        private string notificationBadge;
        public string NotificationBadge
        {
            get { return notificationBadge; }
            set { SetProperty(ref notificationBadge, value); }
        }

        #endregion

        #region Private fields

        private int badgeCounter = 0;
        private int BadgeCounter
        {
            get { return badgeCounter; }
            set
            {
                var isNewValue = SetProperty(ref badgeCounter, value);

                if (isNewValue)
                    UpdateNotificationBagde();

                if (BadgeCounter == 0)
                    NotificationBadge = string.Empty;
            }
        }

        private bool? isConnected = false;
        public bool? IsConnected
        {
            get => isConnected;
            set
            {
                var isNewValue = SetProperty(ref isConnected, value);

                if (isNewValue)
                    UpdateStatusConnectToolbar();
            }
        }

        #endregion

        #region Private methods

        private void LoadDefaultFieldValue()
        {
            AppLogStatusBar = mAppLogStatusBar;
            TextOnStatusConnectToolbar = mTextOnStatusConnectToolbarDisconnected;
            ConnectIcon = PackIconModernKind.Connect;

        }

        /// <summary>
        /// <code>BadgeCounter < 2</code>
        /// Restricts the notification counter so that it is not updated on repeated connections. 
        /// Only one notification will work. When the second one appears, they will need to be distinguished somehow.
        /// </summary>
        private void UpdateNotificationBagde()
        {
            if(BadgeCounter < 2)
                NotificationBadge = $"{BadgeCounter}";
        }

        private void LoadResource()
        {
            mTextOnStatusConnectToolbarDisconnected = mCultureProvider.FindResource("StatusBarViewModel.TextOnStatusConnectToolbarDisconnected");
            mTextOnStatusConnectToolbarConnected = mCultureProvider.FindResource("StatusBarViewModel.TextOnStatusConnectToolbarConnected");
            mTextOnStatusConnectToolbarReconnected = mCultureProvider.FindResource("StatusBarViewModel.TextOnStatusConnectToolbarReconnected");

            mAppLogStatusBar = mCultureProvider.FindResource("StatusBarViewModel.AppLogStatusBar");
 
            mChangAppLanguageLogMessage = mCultureProvider.FindResource("StatusBarViewModel.ChangAppLanguage.LogMessage");
        }

        private void UpdateStatusConnectToolbar()
        {
            if(IsConnected == true)
            {
                TextOnStatusConnectToolbar = mTextOnStatusConnectToolbarConnected;
                ConnectIcon = PackIconModernKind.Disconnect;
            }

            if (IsConnected == false)
            {
                TextOnStatusConnectToolbar = mTextOnStatusConnectToolbarDisconnected;
                ConnectIcon = PackIconModernKind.Connect;
            }

            mLogger.LogInformation(mChangAppLanguageLogMessage);
        }

        #endregion

        #region Subscribes

        private void Subscribe()
        {
            mCommunicationProviderService.RaiseTcpServiceCientConnectedEvent += RaiseAdamTcpCientConnectedEvent;
            mCommunicationProviderService.RaiseTcpServiceClientDisconnectEvent += RaiseAdamTcpClientDisconnectEvent;
            mCommunicationProviderService.RaiseTcpServiceClientReconnectedEvent += RaiseTcpServiceClientReconnectedEvent;

            mStatusBarNotificationDelivery.RaiseChangeProgressRingStateEvent += RaiseChangeProgressRingStateEvent;
            mStatusBarNotificationDelivery.RaiseUpdateNotificationCounterEvent += RaiseUpdateNotificationCounterEvent;

            mFlyoutState.IsFlyoutsOpenedStateChangeEvent += IsOpenedStateChangeEvent;

            mCultureProvider.RaiseCurrentAppCultureLoadOrChangeEvent += RaiseCurrentAppCultureLoadOrChangeEvent;
            mLogWriteEventAwareService.RaiseNewLogMessageWriteEvent += RaiseNewLogMessageWriteEvent;
        }



        private void Unsubscribe() 
        {
            mCommunicationProviderService.RaiseTcpServiceCientConnectedEvent -= RaiseAdamTcpCientConnectedEvent;
            mCommunicationProviderService.RaiseTcpServiceClientDisconnectEvent -= RaiseAdamTcpClientDisconnectEvent;
            mCommunicationProviderService.RaiseTcpServiceClientReconnectedEvent -= RaiseTcpServiceClientReconnectedEvent;

            mStatusBarNotificationDelivery.RaiseChangeProgressRingStateEvent -= RaiseChangeProgressRingStateEvent;
            //mStatusBarNotificationDelivery.RaiseNewCompileLogMessageEvent -= RaiseNewCompileLogMessageEvent;

            mFlyoutState.IsFlyoutsOpenedStateChangeEvent -= IsOpenedStateChangeEvent;

            mCultureProvider.RaiseCurrentAppCultureLoadOrChangeEvent -= RaiseCurrentAppCultureLoadOrChangeEvent;
            mLogWriteEventAwareService.RaiseNewLogMessageWriteEvent -= RaiseNewLogMessageWriteEvent;
        }

        #endregion

        #region Event methods

        private void RaiseAdamTcpCientConnectedEvent(object sender)
        {
            IsConnected = true;
        }

        private void RaiseTcpServiceClientReconnectedEvent(object sender, int reconnectCounter)
        {
            mStatusBarNotificationDelivery.NotificationCounter++;
            ConnectIcon = PackIconModernKind.TransitConnectionDeparture;
            TextOnStatusConnectToolbar = $"{mTextOnStatusConnectToolbarReconnected} {reconnectCounter}";
        }

        private void RaiseAdamTcpClientDisconnectEvent(object sender, bool isUserRequest)
        {
           
            IsConnected = false;
            
            if(!isUserRequest)
                mStatusBarNotificationDelivery.NotificationCounter++;
        }

        private void RaiseChangeProgressRingStateEvent(object sender, bool newState)
        {
            ProgressRingStart = newState;
        }

        /*private void RaiseNewCompileLogMessageEvent(object sender, string message)
        {
            //CompileLogStatusBar = message;
        }*/

        private void RaiseNewAppLogMessageEvent(object sender, string message)
        {
            AppLogStatusBar = message;
        }

        private void RaiseUpdateNotificationCounterEvent(object sender, int counter)
        {
            BadgeCounter = counter;
        }

        private void IsOpenedStateChangeEvent(object sender)
        {
            OpenNotificationPanelDelegateCommand.RaiseCanExecuteChanged();
        }

        private void RaiseCurrentAppCultureLoadOrChangeEvent(object sender)
        {
            LoadResource();
            UpdateStatusConnectToolbar();
        }

        private void RaiseNewLogMessageWriteEvent(object sender, string message)
        {
            AppLogStatusBar = message;
        }

        #endregion
    }
}
