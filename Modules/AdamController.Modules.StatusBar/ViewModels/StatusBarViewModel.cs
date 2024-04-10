﻿using AdamController.Controls.CustomControls.Services;
using AdamController.Core;
using AdamController.Core.Mvvm;
using AdamController.Services.Interfaces;
using MahApps.Metro.IconPacks;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace AdamController.Modules.StatusBarRegion.ViewModels
{
    public class StatusBarViewModel : RegionViewModelBase
    {
        #region DelegateCommands

        public DelegateCommand OpenNotificationPanelDelegateCommand { get; }

        #endregion

        #region Services

        private readonly IFlyoutManager mFlyoutManager;
        private readonly ICommunicationProviderService mCommunicationProviderService;

        #endregion

        #region Const

        private const string cTextOnStatusConnectToolbarDisconnected = "Робот Адам: отключен";
        private const string cTextOnStatusConnectToolbarConnected = "Робот Адам: подключен";
        private const string cTextOnStatusConnectToolbarReconnected = "Робот Адам: переподключение";

        private const string cCompileLogStatusBar = "Лог робота";
        private const string cAppLogStatusBar = "Лог приложения";

        #endregion

        #region ~

        public StatusBarViewModel(IRegionManager regionManager, IDialogService dialogService, IFlyoutManager flyoutManager, ICommunicationProviderService communicationProviderService) : base(regionManager, dialogService)
        {
            mFlyoutManager = flyoutManager;
            mCommunicationProviderService = communicationProviderService;

            OpenNotificationPanelDelegateCommand = new DelegateCommand(OpenNotificationPanel, OpenNotificationPanelCanExecute);

            //ComunicateHelper.OnAdamTcpConnectedEvent += OnTcpConnected;
            //ComunicateHelper.OnAdamTcpDisconnectedEvent += OnTcpDisconnected;
            //ComunicateHelper.OnAdamTcpReconnected += OnTcpReconnected;

            mCommunicationProviderService.RaiseAdamTcpCientConnected += RaiseAdamTcpCientConnected;
            mCommunicationProviderService.RaiseAdamTcpClientDisconnect += RaiseAdamTcpClientDisconnect;
        }

        #endregion

        #region public fields

        private bool progressRingStart;
        public bool ProgressRingStart
        {
            get { return progressRingStart; }
            set
            {
                SetProperty(ref progressRingStart, value);
            }
        }

        private string compileLogStatusBar = cCompileLogStatusBar;
        public string CompileLogStatusBar
        {
            get { return compileLogStatusBar; }
            set
            {
                SetProperty(ref compileLogStatusBar, value);
            }
        }

        private string appLogStatusBar = cAppLogStatusBar;
        public string AppLogStatusBar
        {
            get { return appLogStatusBar; }
            set
            {   
                SetProperty(ref appLogStatusBar, value);
            }
        }

        private PackIconModernKind connectIcon = PackIconModernKind.Connect;
        public PackIconModernKind ConnectIcon
        {
            get { return connectIcon; }
            set
            {
                SetProperty(ref connectIcon, value);
            }
        }

        private string textOnStatusConnectToolbar = cTextOnStatusConnectToolbarDisconnected;
        public string TextOnStatusConnectToolbar
        {
            get { return textOnStatusConnectToolbar; }
            set
            {
                SetProperty(ref textOnStatusConnectToolbar, value);
            }
        }

        private string notificationBadge;
        public string NotificationBadge
        {
            get { return notificationBadge; }
            set
            {
                SetProperty(ref notificationBadge, value);
            }
        }

        #endregion

        #region Private fields

        private int badgeCounter = 0;
        private int BadgeCounter
        {
            get { return badgeCounter; }
            set
            {
                if (badgeCounter == value) 
                    return;

                if (value == 0)
                {
                    badgeCounter = value;
                    NotificationBadge = "";
                    return;
                }

                badgeCounter = value;

                NotificationBadge = $"{BadgeCounter}";
            }
        }

        #endregion

        #region Event methods

        private void RaiseAdamTcpClientDisconnect(object sender)
        {
            ConnectIcon = PackIconModernKind.Connect;
            TextOnStatusConnectToolbar = cTextOnStatusConnectToolbarDisconnected;
        }

        private void RaiseAdamTcpCientConnected(object sender)
        {
            ConnectIcon = PackIconModernKind.Disconnect;
            TextOnStatusConnectToolbar = cTextOnStatusConnectToolbarConnected;
        }

        #endregion

        #region DelegateCommands methods

        private void OpenNotificationPanel()
        {
            mFlyoutManager.OpenFlyout(FlyoutNames.FlyoutNotification);
        }

        private bool OpenNotificationPanelCanExecute()
        {
            return true;
        }

        #endregion

    }
}
