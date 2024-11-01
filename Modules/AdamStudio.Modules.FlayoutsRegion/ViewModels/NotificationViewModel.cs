using AdamStudio.Controls.CustomControls.Mvvm.FlyoutContainer;
using AdamStudio.Controls.CustomControls.Services;
using AdamStudio.Core.Properties;
using AdamStudio.Services.Interfaces;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.DependencyInjection;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Drawing;
using System.Windows;

namespace AdamStudio.Modules.FlayoutsRegion.ViewModels
{
    public class NotificationViewModel : FlyoutBase
    {
        #region DelegateCommands

        public DelegateCommand ConnectButtonDelegateCommand { get; private set;  }
        public DelegateCommand ReconnectNotificationButtonDelegateCommand { get; private set; }
        public DelegateCommand ResetNotificationsDelegateCommand { get; private set; }

        #endregion

        #region Services

        private readonly ICommunicationProviderService mCommunicationProvider;
        private readonly IStatusBarNotificationDeliveryService mStatusBarNotificationDeliveryService;
        private readonly IFlyoutStateChecker mFlyoutState;
        private readonly ICultureProvider mCultureProvider;

        #endregion

        #region Var

        private bool mIsDisconnectByUserRequest = false;

        private string mFlyoutHeader;
        private string mConnectButtonStatusDisconnected;
        private string mConnectButtonStatusConnected;
        private string mConnectButtonStatusReconnected;

        #endregion

        #region ~

        public NotificationViewModel(IServiceProvider serviceProvider) 
        {
            mCommunicationProvider = serviceProvider.GetService<ICommunicationProviderService>(); ;
            mStatusBarNotificationDeliveryService = serviceProvider.GetService<IStatusBarNotificationDeliveryService>(); 
            mFlyoutState = serviceProvider.GetService<IFlyoutStateChecker>();
            mCultureProvider = serviceProvider.GetService<ICultureProvider>();

            ConnectButtonDelegateCommand = new(ConnectButton, ConnectButtonCanExecute);
            ReconnectNotificationButtonDelegateCommand = new(ReconnectNotificationButton, ReconnectNotificationButtonCanExecute);
            ResetNotificationsDelegateCommand = new(ResetNotifications, ResetNotificationsCanExecute);
        }

        #endregion

        #region Navigation

        protected override void OnChanging(bool isOpening)
        {
            if (isOpening)
            {
                LoadResources();
                LoadFlyoutParametrs();
               
                mFlyoutState.IsNotificationFlyoutOpened = true;
                
                Subscribe();

                UpdateStatusConnection(mCommunicationProvider.IsTcpClientConnected);
                
                return;
            }
            
            if (!isOpening)
            {
                mFlyoutState.IsNotificationFlyoutOpened = false;

                Unsubscribe();

                ConnectButtonDelegateCommand = null;
                ReconnectNotificationButtonDelegateCommand = null;
                ResetNotificationsDelegateCommand = null;

                return;
            }
        }

        #endregion

        #region Public field

        private Visibility noNewNotificationMessageVisibility = Visibility.Visible;
        public Visibility NoNewNotificationMessageVisibility
        {
            get => noNewNotificationMessageVisibility;
            set => SetProperty(ref noNewNotificationMessageVisibility, value);
        }

        private Visibility failConnectNotificationVisibility = Visibility.Collapsed;
        public Visibility FailConnectNotificationVisibility
        {
            get => failConnectNotificationVisibility;
            set
            {
                bool isNewValue = SetProperty(ref failConnectNotificationVisibility, value);

                if (isNewValue)
                {
                    if (FailConnectNotificationVisibility == Visibility.Visible)
                        NoNewNotificationMessageVisibility = Visibility.Collapsed;

                    if (FailConnectNotificationVisibility == Visibility.Collapsed)
                        NoNewNotificationMessageVisibility = Visibility.Visible;
                }
            }
        }

        private string contentConnectButton;
        public string ContentConnectButton
        {
            get => contentConnectButton;
            set => SetProperty(ref contentConnectButton, value);
        }

        private PackIconMaterialKind iconConnectButton = PackIconMaterialKind.Robot;
        public PackIconMaterialKind IconConnectButton
        {
            get => iconConnectButton;
            set => SetProperty(ref iconConnectButton, value);
        }

        #endregion

        #region Private methods

        private void LoadFlyoutParametrs()
        {
            Theme = FlyoutTheme.Adapt;
            Header = mFlyoutHeader;
            IsModal = false;
            BorderBrush = Application.Current.TryFindResource("MahApps.Brushes.Text").ToString(); 
        }

        /// <summary>
        /// Controls the display of the connection status
        /// </summary>
        /// <param name="connectionStatus">true is connected, false disconetcted, null reconected</param>
        /// <param name="reconnectCounter"></param>
        private void UpdateStatusConnection(bool? connectionStatus, bool isDisconnectByUserRequest = false, int reconnectCounter = 0)
        {
            if (connectionStatus == true)
            {
                mIsDisconnectByUserRequest = false;
                ContentConnectButton = mConnectButtonStatusConnected;
                IconConnectButton = PackIconMaterialKind.Robot;
            }

            if(connectionStatus == false && mIsDisconnectByUserRequest == false)
            {
                IconConnectButton = PackIconMaterialKind.RobotDead;
                ContentConnectButton = mConnectButtonStatusDisconnected;

                if (isDisconnectByUserRequest)
                {
                    mIsDisconnectByUserRequest = true;
                    return;
                }
                    
                if (Settings.Default.IsMessageShowOnAbortMainConnection)
                {
                    if (!IsOpen)
                        FailConnectNotificationVisibility = Visibility.Visible;       
                }
            }

            if(connectionStatus == null)
            {
                ContentConnectButton = $"{mConnectButtonStatusReconnected} {reconnectCounter}";
                IconConnectButton = PackIconMaterialKind.RobotConfused;
            }
        }

        private void LoadResources()
        {

            mFlyoutHeader = mCultureProvider.FindResource("NotificationView.ViewModel.Flyout.Header");
            mConnectButtonStatusDisconnected = mCultureProvider.FindResource("NotificationView.ViewModel.Button.Content.StatusDisconnected"); 
            mConnectButtonStatusConnected = mCultureProvider.FindResource("NotificationView.ViewModel.Button.Content.StatusConnected"); 
            mConnectButtonStatusReconnected = mCultureProvider.FindResource("NotificationView.ViewModel.Button.Content.StatusReconnected");
        }

    
        #endregion

        #region Subscription

    
        private void Subscribe()
        {
            mCommunicationProvider.RaiseTcpServiceCientConnectedEvent += OnRaiseTcpServiceCientConnected;
            mCommunicationProvider.RaiseTcpServiceClientReconnectedEvent += OnRaiseTcpServiceClientReconnected;
            mCommunicationProvider.RaiseTcpServiceClientDisconnectEvent += OnRaiseTcpServiceClientDisconnect;
        }

        private void Unsubscribe() 
        {
            mCommunicationProvider.RaiseTcpServiceCientConnectedEvent -= OnRaiseTcpServiceCientConnected;
            mCommunicationProvider.RaiseTcpServiceClientReconnectedEvent -= OnRaiseTcpServiceClientReconnected;
            mCommunicationProvider.RaiseTcpServiceClientDisconnectEvent -= OnRaiseTcpServiceClientDisconnect;
        }

        #endregion

        #region Event methods

        private void OnRaiseTcpServiceCientConnected(object sender)
        {
            UpdateStatusConnection(true);   
        }

        private void OnRaiseTcpServiceClientReconnected(object sender, int reconnectCounter)
        {
            UpdateStatusConnection(null, reconnectCounter: reconnectCounter);
        }

        private void OnRaiseTcpServiceClientDisconnect(object sender, bool isUserRequest)
        {
            UpdateStatusConnection(false, isDisconnectByUserRequest:isUserRequest); 
        }

        private void RaiseCurrentAppCultureLoadOrChangeEvent(object sender)
        {
            LoadResources();
        }


        #endregion

        #region DelegateCommands methods

        private void ConnectButton()
        {
            bool isConnected = mCommunicationProvider.IsTcpClientConnected;

            if (isConnected)
            {
                bool isUserRequest = true;
                mCommunicationProvider.DisconnectAllAsync(isUserRequest);
                return;
            }
            
            mCommunicationProvider.ConnectAllAsync();
        }

        private bool ConnectButtonCanExecute()
        {
            return true;
        }

        private void ResetNotifications()
        {
            mStatusBarNotificationDeliveryService.ResetNotificationCounter();
            FailConnectNotificationVisibility = Visibility.Collapsed;
        }

        private bool ResetNotificationsCanExecute()
        {
            return true;
        }

        private void ReconnectNotificationButton()
        {
            bool isConnected = mCommunicationProvider.IsTcpClientConnected;

            if (!isConnected)
            {
                mCommunicationProvider.ConnectAllAsync();
                ResetNotifications();
            }
        }

        private bool ReconnectNotificationButtonCanExecute()
        {
            return true;
        }

        #endregion
    }




}
